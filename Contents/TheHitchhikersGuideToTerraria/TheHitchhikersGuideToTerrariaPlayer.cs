using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.TheHitchhikersGuideToTerraria;

/// <summary>
/// 《泰拉瑞亚漫游指南》的发放与领取记录模块。
/// <para>职责：</para>
/// <para>1. 新角色在菜单中被创建时，把漫游指南放进初始背包（<see cref="AddStartingItems"/>）。</para>
/// <para>2. 玩家进入世界时，若该角色从未记录过领取、且背包里确实没有这本书，则补发一次（<see cref="OnEnterWorld"/>），
/// 用于兼容功能上线前创建的老角色，以及物品被丢弃的情况。</para>
/// <para>3. 领取状态写入角色存档，保证每个角色终身只发放一次。</para>
/// <para>多人模式下：创建角色那一份由游戏引擎随角色数据同步；进世界补发则交由服务端执行（见 <see cref="TheHitchhikersGuideSync"/>），
/// 避免客户端本地改背包被服务端数据覆盖。</para>
/// </summary>
public class TheHitchhikersGuideToTerrariaPlayer : ModPlayer
{
    /// <summary>
    /// 该角色是否已领取过漫游指南。
    /// <para>true 表示已领取：之后进入世界不再补发。该值会随角色存档一起保存。</para>
    /// </summary>
    public bool receivedGuide;

    /// <summary>
    /// 该角色是否已领取过漫游指南赠送的一次性小礼物。
    /// <para>true 表示已领取：左键使用不再发放，只给出「已经领取过了」的提示。该值会随角色存档一起保存。</para>
    /// </summary>
    public bool giftClaimed;

    /// <summary>
    /// 该角色是否正在显示本模组指南的 tooltip。
    /// <para>true 表示物品 tip 已替换为指南文本，再次背包内右键恢复。</para>
    /// </summary>
    public bool showGuideTooltip; // ← 就是这一行，确保它存在且为 public

    /// <summary>
    /// 角色在菜单中被创建时调用，返回值会直接进入角色的初始背包。
    /// <para>⚠️ 中核（Mediumcore）角色死亡重生时本方法也会被调用（此时 <paramref name="mediumCoreDeath"/> 为 true）。
    /// 若在那种情况下仍然返回物品，玩家每次重生都会多拿到一份，因此该分支必须返回空集合。</para>
    /// </summary>
    /// <param name="mediumCoreDeath">是否为中核角色死亡后的重生初始化。</param>
    /// <returns>需要加入初始背包的物品序列（中核重生时为空）。</returns>
    public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
    {
        // 中核重生分支：这里刻意不给任何物品，避免重复发放
        if (mediumCoreDeath)
            yield break;

        yield return new Item(ModContent.ItemType<TheHitchhikersGuideToTerraria>());
    }

    /// <summary>
    /// 玩家进入世界时调用（仅在单人模式或多人客户端的本地玩家上触发，服务端不会调用）。
    /// <para>判定顺序：已记录领取 → 直接返回；背包已有 → 只补记录；多人客户端 → 交由服务端发放；其余情况本地发放。</para>
    /// </summary>
    public override void OnEnterWorld()
    {
        // 已领取过：这是「每个角色仅一次」的关键
        if (receivedGuide)
            return;

        // 背包（含装备栏、银行、垃圾桶）里已经有这本书：说明该角色本来就持有，只把记录补上，不再多发
        if (HasGuide(Player))
        {
            receivedGuide = true;
            return;
        }

        // 多人客户端不做本地发放：客户端改背包不会同步给服务器，稍后会被服务器数据覆盖。
        // 改为向服务端请求，由服务端校验并发放（服务端权威）。
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            TheHitchhikersGuideSync.Request(Player.whoAmI);
            return;
        }

        // 单人模式（或服务端侧的其他调用场景）：直接发放
        GrantGuide(Player);
    }

    /// <summary>
    /// 把漫游指南放入指定玩家的背包，并写入领取记录。
    /// <para>幂等：若该玩家已记录领取、或背包中已存在该书，则不会重复发放。</para>
    /// </summary>
    /// <param name="player">目标玩家（服务端可传入任一在线玩家）。</param>
    /// <returns>本次是否真的发放了物品。</returns>
    internal static bool GrantGuide(Player player)
    {
        var modPlayer = player.GetModPlayer<TheHitchhikersGuideToTerrariaPlayer>();

        if (modPlayer.receivedGuide)
            return false;

        if (HasGuide(player))
        {
            modPlayer.receivedGuide = true;
            return false;
        }

        // 必须使用 Player.GetItem 才是「放入背包」；
        // Player.QuickSpawnItem 的内部实现是 Item.NewItem，会把物品掉到世界地面上。
        player.GetItem(player.whoAmI, new Item(ModContent.ItemType<TheHitchhikersGuideToTerraria>()), GetItemSettings.PickupItemFromWorld);
        modPlayer.receivedGuide = true;

        // 服务端直接改了玩家背包，必须把该玩家的数据同步给客户端，否则客户端本地看不到这份物品
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, player.whoAmI);

        return true;
    }

    /// <summary>
    /// 检查玩家的各个物品容器中是否已经存在漫游指南。
    /// <para>查找范围与项目内既有的记录判定保持一致：主背包、装备栏、染料栏、坐骑/宠物等杂项装备栏、
    /// 杂项染料栏、垃圾桶，以及四个存储单元（猪猪存钱罐 / 保险箱 / 护卫熔炉 / 虚空保险库）。</para>
    /// </summary>
    /// <param name="player">要检查的玩家。</param>
    /// <returns>已持有时返回 true。</returns>
    internal static bool HasGuide(Player player)
    {
        int guideType = ModContent.ItemType<TheHitchhikersGuideToTerraria>();

        Item[][] containers =
        [
            player.inventory,
            player.armor,
            player.dye,
            player.miscEquips,
            player.miscDyes,
            [player.trashItem],
            player.bank.item,
            player.bank2.item,
            player.bank3.item,
            player.bank4.item
        ];

        foreach (var container in containers)
            foreach (var item in container)
                if (item.type == guideType)
                    return true;

        return false;
    }

    /// <summary>
    /// 把领取状态写入角色存档。
    /// <para>注意：必须与 <see cref="LoadData"/> 成对覆写，否则 tModLoader 的
    /// MustOverrideTogether 校验会在加载时报错。</para>
    /// </summary>
    /// <param name="tag">角色存档数据。</param>
    public override void SaveData(TagCompound tag)
    {
        // 只在已领取时写入，减小存档体积（与项目内其它模块的短键名习惯一致）
        if (receivedGuide)
            tag["HG"] = true;

        // 小礼物的领取状态同样只在已领取时写入
        if (giftClaimed)
            tag["HGift"] = true;

        // 指南 tooltip 显示状态
        if (showGuideTooltip)
            tag["HGTip"] = true;

        base.SaveData(tag);
    }

    /// <summary>
    /// 从角色存档读取领取状态。
    /// </summary>
    /// <param name="tag">角色存档数据。</param>
    public override void LoadData(TagCompound tag)
    {
        receivedGuide = tag.TryGet("HG", out bool value) && value;
        giftClaimed = tag.TryGet("HGift", out bool claimed) && claimed;
        showGuideTooltip = tag.TryGet("HGTip", out bool tip) && tip;

        base.LoadData(tag);
    }
}