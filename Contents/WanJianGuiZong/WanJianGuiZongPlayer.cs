using System.Collections.Generic;
using MatterRecord.Contents.TodaySword;
using Microsoft.Xna.Framework;
using Terraria.Audio;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「万剑归宗」的玩家逻辑：收集背包 / 猪猪存钱罐 / 保险箱 / 护卫熔炉 / 虚空保险库里的剑，
/// 长按期间以固定间隔逐把放出，松手时把还在盘旋的飞剑全部放出去。
/// 只在本地玩家所在端运行（服务器与旁观端不跑）。
/// </summary>
public class WanJianGuiZongPlayer : ModPlayer
{
    /// <summary>两把飞剑之间的出鞘间隔（帧）。</summary>
    public const int SpawnInterval = 5;

    /// <summary>
    /// 一次最多唤出多少把剑。背包 50 格加四个副背包各 40 格，理论上能凑出两百多把，
    /// 那会占掉 1000 个弹幕槽位的一大块、渲染和联机同步也很重，所以设个上限：
    /// 超了就从最弱的开始砍，留下的仍是伤害最高的那些。
    /// </summary>
    public const int MaxSummonCount = 150;

    /// <summary>待飞出的剑（进入长按时做的快照）。</summary>
    private readonly List<Item> _pending = new();

    /// <summary>已经飞出、还在盘旋的飞剑的弹幕槽位。</summary>
    private readonly List<int> _flying = new();

    /// <summary>本轮已经从背包里收进来的剑类型，用来避免保底剑重复。</summary>
    private readonly HashSet<int> _collectedTypes = new();

    private int _spawnTimer;
    private bool _channeling;

    /// <summary>是否正处于一轮长按唤剑中。</summary>
    public bool Channeling => _channeling;

    /// <summary>开始一轮唤剑：背包 + 副背包里的剑，再加保底剑池里缺的那些剑。整轮按伤害从低到高出场。</summary>
    public void StartChannel()
    {
        if (_channeling)
            return;

        _channeling = true;
        _pending.Clear();
        _flying.Clear();
        _collectedTypes.Clear();
        _spawnTimer = 0;

        foreach (Item[] container in EnumerateContainers(Player))
            CollectSwords(container);

        foreach (int type in WanJianSwordPool.BaseSwords)
        {
            if (_collectedTypes.Add(type))
                _pending.Add(new Item(type));
        }

        // 弱剑先出、天顶剑压轴
        _pending.Sort(static (a, b) => a.damage.CompareTo(b.damage));

        // 超过上限就从最弱的一头砍掉，留下的还是伤害最高的那些
        if (_pending.Count > MaxSummonCount)
            _pending.RemoveRange(0, _pending.Count - MaxSummonCount);
    }

    /// <inheritdoc />
    public override void PostUpdate()
    {
        // 只认「本地玩家」这一端。不用 Main.netMode == NetmodeID.Server 判断：
        // 主机开房间时主机进程 netMode 就是 2，房主自己会因此用不了。
        // 专用服务器上 Main.myPlayer 是 255，这里会直接返回。
        if (Player.whoAmI != Main.myPlayer)
            return;

        if (!_channeling)
            return;

        // 松手 / 换武器 / 死亡 => 全部放出
        if (Player.dead ||
            Player.HeldItem.type != ModContent.ItemType<WanJianGuiZong>() ||
            !Player.channel ||
            !Player.controlUseItem)
        {
            ReleaseAll();
            return;
        }

        PruneFlying();

        if (_pending.Count == 0)
            return;

        if (_spawnTimer > 0)
        {
            _spawnTimer--;
            return;
        }

        SpawnNext();
        _spawnTimer = SpawnInterval;
    }

    /// <summary>把还在盘旋的飞剑全部沿「玩家中心 → 鼠标」这条直线射出去。</summary>
    private void ReleaseAll()
    {
        _channeling = false;
        _pending.Clear();

        PruneFlying();

        // 所有飞剑方向统一：路径与这条直线平行，正好在直线上的剑就沿直线本身飞
        Vector2 launchDirection = Main.MouseWorld - Player.Center;
        if (launchDirection.LengthSquared() < 16f)
            launchDirection = Vector2.UnitX * Player.direction;
        launchDirection = launchDirection.SafeNormalize(Vector2.UnitX * Player.direction);

        // 本地先放出去，保证自己这一端立刻有反馈
        int released = ReleaseSwords(Player.whoAmI, launchDirection, broadcast: false);

        if (released > 0)
            SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.7f, Pitch = -0.5f }, Player.Center);

        // 多人下再请服务器改它那份副本并广播：客户端单靠 netUpdate 传不到队友那边
        if (Main.netMode == NetmodeID.MultiplayerClient)
            WanJianReleaseSync.Get(Player.whoAmI, launchDirection).Send();

        _flying.Clear();
    }

    /// <summary>
    /// 把某个玩家手上还在盘旋的飞剑全部放出去。
    /// <paramref name="broadcast"/> 为 true 时（服务器调用）顺带显式广播同步，
    /// 因为服务器端这些弹幕的 owner 不是服务器自己，netUpdate 不会自动发。
    /// </summary>
    public static int ReleaseSwords(int whoAmI, Vector2 direction, bool broadcast)
    {
        int projType = ModContent.ProjectileType<WanJianSwordProjectile>();
        int released = 0;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile projectile = Main.projectile[i];
            if (!projectile.active || projectile.type != projType || projectile.owner != whoAmI)
                continue;

            if (projectile.ai[2] == WanJianSwordProjectile.StateReleased)
                continue;

            projectile.ai[2] = WanJianSwordProjectile.StateReleased;
            projectile.velocity = direction * WanJianSwordProjectile.ReleaseSpeed;
            released++;

            if (broadcast)
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, i);
            else
                projectile.netUpdate = true;
        }

        return released;
    }

    /// <summary>放出一把剑。</summary>
    private void SpawnNext()
    {
        Item sword = _pending[0];
        _pending.RemoveAt(0);

        int damage = (int)Player.GetTotalDamage(DamageClass.Melee).ApplyTo(sword.damage);
        if (damage < 1)
            damage = 1;

        Vector2 spawnPosition = Player.Center + Main.rand.NextVector2Circular(10f, 10f);

        int index = Projectile.NewProjectile(
            Player.GetSource_ItemUse(Player.HeldItem),
            spawnPosition,
            Vector2.Zero,
            ModContent.ProjectileType<WanJianSwordProjectile>(),
            damage,
            sword.knockBack,
            Player.whoAmI,
            ai0: sword.type);

        if (Main.projectile.IndexInRange(index))
            _flying.Add(index);

        SoundEngine.PlaySound(SoundID.Item1 with
        {
            Volume = 0.35f,
            Pitch = Main.rand.NextFloat(-0.35f, 0.15f)
        }, spawnPosition);

        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.SilverFlame);
            dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
            dust.noGravity = true;
            dust.scale = 0.9f;
        }
    }

    /// <summary>清掉已经消失的飞剑记录。</summary>
    private void PruneFlying()
    {
        int projType = ModContent.ProjectileType<WanJianSwordProjectile>();
        for (int i = _flying.Count - 1; i >= 0; i--)
        {
            int index = _flying[i];
            if (!Main.projectile.IndexInRange(index))
            {
                _flying.RemoveAt(i);
                continue;
            }

            Projectile projectile = Main.projectile[index];
            if (!projectile.active || projectile.type != projType || projectile.owner != Player.whoAmI)
                _flying.RemoveAt(i);
        }
    }

    /// <summary>把一个容器里的剑加入待飞出列表。</summary>
    private void CollectSwords(Item[] items)
    {
        for (int i = 0; i < items.Length; i++)
        {
            Item item = items[i];
            if (item == null || item.IsAir)
                continue;

            if (!IsUsableSword(item))
                continue;

            _collectedTypes.Add(item.type);
            _pending.Add(item.Clone());
        }
    }

    /// <summary>统计玩家当前可用的剑：背包与副背包里的剑，加上背包里没有的天顶剑材料保底剑。</summary>
    public static int CountAvailableSwords(Player player)
    {
        HashSet<int> collectedTypes = new();
        int count = 0;

        foreach (Item[] container in EnumerateContainers(player))
        {
            for (int i = 0; i < container.Length; i++)
            {
                Item item = container[i];
                if (item == null || item.IsAir || !IsUsableSword(item))
                    continue;

                collectedTypes.Add(item.type);
                count++;
            }
        }

        foreach (int type in WanJianSwordPool.BaseSwords)
        {
            if (!collectedTypes.Contains(type))
                count++;
        }

        return count;
    }

    /// <summary>
    /// 背包（含副背包）里第一把可用剑的物品类型，用来当「天剑」化身时的贴图与伤害。
    /// 一把剑都没有时返回 <see cref="ItemID.None"/>。
    /// </summary>
    public static int FindFirstSwordType(Player player)
    {
        foreach (Item[] container in EnumerateContainers(player))
        {
            for (int i = 0; i < container.Length; i++)
            {
                Item item = container[i];
                if (item == null || item.IsAir || !IsUsableSword(item))
                    continue;

                return item.type;
            }
        }

        return ItemID.None;
    }

    /// <summary>
    /// 背包（含副背包）里可用剑的最高伤害，用来当「天剑」化身的伤害。
    /// <para>取的是物品实例上的 <c>damage</c>，所以带前缀的剑按加成后的数值算；
    /// 一把剑都没有时返回 0（调用方再拿天剑自己的伤害兜底）。</para>
    /// </summary>
    public static int FindStrongestSwordDamage(Player player)
    {
        int best = 0;

        foreach (Item[] container in EnumerateContainers(player))
        {
            for (int i = 0; i < container.Length; i++)
            {
                Item item = container[i];
                if (item == null || item.IsAir || !IsUsableSword(item))
                    continue;

                if (item.damage > best)
                    best = item.damage;
            }
        }

        return best;
    }

    /// <summary>可用飞剑 = 通用「剑」判定命中的物品，或保底剑池的根（天顶剑与它的合成材料）。</summary>
    private static bool IsUsableSword(Item item)
    {
        return DailySwordSystem.IsSword(item) || WanJianSwordPool.IsPoolRoot(item.type);
    }

    /// <summary>背包 + 猪猪存钱罐 / 保险箱 / 护卫熔炉 / 虚空保险库。</summary>
    private static IEnumerable<Item[]> EnumerateContainers(Player player)
    {
        yield return player.inventory;

        if (player.bank != null)
            yield return player.bank.item;
        if (player.bank2 != null)
            yield return player.bank2.item;
        if (player.bank3 != null)
            yield return player.bank3.item;
        if (player.bank4 != null)
            yield return player.bank4.item;
    }
}
