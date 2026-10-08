using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「天剑」的化身弹幕：弹幕自己不动，位置永远贴着玩家——真正在飞的是玩家本人。
/// <para>长按阶段（ai[1] = 0）：每帧算出「鼠标方向 × 速度」这一步位移，直接加在玩家坐标上。
/// 速度由鼠标离屏幕中心的距离决定：贴着中心是 <see cref="MinSpeed"/>，到屏幕四角（半对角线）吃满
/// <see cref="MaxSpeed"/>，中心与边缘的差距是实打实拉开的。</para>
/// <para>松手后（ai[1] = 1）保持松手那一刻的位移，逐帧衰减到停下为止。</para>
/// <para>位移是每帧整份覆盖玩家坐标、不走原版碰撞的，所以飞行期间无视地形、可以穿墙。
/// 松手滑行完就地停下——哪怕停在石头里也停着，不做任何「顶出来」的处理。</para>
/// <para>ai[0] = 化身用的剑的物品类型（贴图，0 表示退回天顶剑），ai[1] = 阶段，ai[2] = 滑行计时。
/// 玩家的隐身、减伤与免击退由 <see cref="TianJianPlayer"/> 负责。</para>
/// </summary>
public class TianJianProjectile : ModProjectile
{
    /// <summary>
    /// 默认贴图：直接引用原版天顶剑，不额外打包 png。
    /// 实际绘制在 <see cref="PreDraw"/> 里被接管（画的是化身那把剑的物品贴图），这个只是给 tML 资源加载留的落点。
    /// </summary>
    public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;

    /// <summary>鼠标贴着屏幕中心时的速度（像素 / 帧）。</summary>
    public const float MinSpeed = 10f;

    /// <summary>鼠标到屏幕四角时的速度。</summary>
    public const float MaxSpeed = 70f;

    /// <summary>残影长度（帧）。</summary>
    private const int TrailLength = 8;

    /// <summary>最旧那道残影的不透明度。</summary>
    private const float TrailAlpha = 0.35f;

    /// <summary>松手后每帧保留的速度比例。</summary>
    private const float GlideDecay = 0.96f;

    /// <summary>滑行速度低于这个值就收回来。</summary>
    private const float GlideStopSpeed = 1.5f;

    /// <summary>滑行最长帧数，防止低速卡着不收。</summary>
    private const float GlideMaxTime = 120f;

    /// <summary>化身贴图的基准尺寸（像素）：按贴图最大边缩放到这附近。</summary>
    private const float RideSpriteSize = 64f;

    /// <summary>
    /// 同一个敌人被撞一次之后，多少帧内不再结算第二次。
    /// 比一般弹幕的 20 帧短——化身是贴在玩家身上持续接触的，冷却长了就只剩贴着磨。
    /// </summary>
    private const int HitCooldown = 8;

    /// <summary>长按飞行阶段。</summary>
    private const float PhaseFlying = 0f;

    /// <summary>松手后的滑行阶段。</summary>
    private const float PhaseGliding = 1f;

    /// <summary>上一帧玩家中心，只有非本人端算朝向时用得到（各端各自的实例字段，不同步）。</summary>
    private Vector2 _lastCenter;
    private bool _hasLastCenter;

    /// <summary>本人端自己维护的化身坐标，每帧整份写回 player.position（各端各自的实例字段，不同步）。</summary>
    private Vector2 _ridePosition;
    private Vector2 _lastAppliedPosition;
    private bool _ridePositionReady;

    /// <summary>player.position 与自己上次写入的值差这么远，就当成被传送了，重新对齐。</summary>
    private const float TeleportResetDistance = 256f;

    /// <inheritdoc />
    public override void SetStaticDefaults()
    {
        // 只改数组长度。TrailingMode 保持 -1：残影由 AI 自己滚动，不跟原版那套自动记录走
        ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        ProjectileID.Sets.TrailingMode[Type] = -1;
    }

    /// <inheritdoc />
    public override void SetDefaults()
    {
        Projectile.width = 48;
        Projectile.height = 48;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;          // 无限穿透，撞到敌人不消失
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 60;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = HitCooldown;

        // 别被「离玩家 5000 像素以外」的距离过滤吃掉，多人下队友那边才看得到
        Projectile.netImportant = true;
    }

    /// <inheritdoc />
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        // 死亡 / 换掉武器就直接收回来，不留滑行
        if (!player.active || player.dead || player.HeldItem.type != ModContent.ItemType<TianJian>())
        {
            Projectile.Kill();
            return;
        }

        if (Projectile.localAI[0] == 0f)
        {
            Projectile.localAI[0] = 1f;
            if (Main.myPlayer == Projectile.owner)
                SoundEngine.PlaySound(SoundID.Item1 with { Volume = 0.6f, Pitch = -0.4f }, player.Center);
        }

        // 化身标记交给 ModPlayer：它负责隐藏玩家本体、给减伤与免击退
        player.GetModPlayer<TianJianPlayer>().Riding = true;

        // 弹幕始终贴在玩家身上，位置的推进由玩家自己负责
        Projectile.Center = player.Center;
        Projectile.timeLeft = 60;

        bool owner = Main.myPlayer == Projectile.owner;

        if (Projectile.ai[1] == PhaseFlying)
            UpdateFlying(player, owner);
        else
            UpdateGliding(player, owner);

        // 位移直接写在自己的坐标上，绕过原版的瓦片碰撞 => 穿墙
        if (owner)
            MovePlayer(player);

        // 剑身朝飞行方向摆正；停下来时保持最后朝向，不乱转。
        // 本人端有自己算的步长，其他端看不到鼠标，只能拿同步过来的位置差当方向
        Vector2 movement = owner
            ? Projectile.velocity
            : (_hasLastCenter ? player.Center - _lastCenter : Vector2.Zero);

        _lastCenter = player.Center;
        _hasLastCenter = true;

        if (movement.LengthSquared() > 0.04f)
        {
            float desiredRotation = movement.ToRotation() + MathHelper.PiOver4;
            Projectile.rotation = Projectile.rotation.AngleLerp(desiredRotation, 0.35f);
        }

        ScrollTrail(player);

        Lighting.AddLight(player.Center, 0.35f, 0.35f, 0.5f);
    }

    /// <summary>长按阶段：跟着鼠标转向，速度由鼠标离屏幕中心的距离决定。</summary>
    private void UpdateFlying(Player player, bool owner)
    {
        // 第一帧不判「还按着没」：弹幕刚生成时原版的 channel 标记未必已经落定，
        // 误判会把整轮飞行当场掐成滑行
        bool firstFrame = Projectile.localAI[1] == 0f;
        Projectile.localAI[1] = 1f;

        if (!firstFrame && (!player.channel || !player.controlUseItem))
        {
            Projectile.ai[1] = PhaseGliding;
            Projectile.netUpdate = true;      // 把滑行的初速度一起同步出去
            return;
        }

        // 长按期间手动续上使用动画，否则原版的 channel 会在动画走完后断掉
        player.itemTime = player.itemAnimation = 180;

        // 别人的鼠标我们看不到，那就不用算——位置由对方的客户端自己推，其他人只是看
        if (owner)
            Projectile.velocity = ComputeFlyStep(player);
    }

    /// <summary>滑行阶段：保持松手那一刻的位移，逐帧衰减到停下。</summary>
    private void UpdateGliding(Player player, bool owner)
    {
        Projectile.ai[2]++;
        Projectile.velocity *= GlideDecay;

        if (owner)
        {
            player.itemTime = 0;
            player.itemAnimation = 0;
        }

        if (Projectile.velocity.Length() >= GlideStopSpeed && Projectile.ai[2] <= GlideMaxTime)
            return;

        // 停在物块里就停着，不把人往外顶
        Projectile.Kill();
    }

    /// <summary>
    /// 把这一步位移写进玩家坐标。位置由自己维护、每帧整份覆盖，不拿 <c>player.position</c> 做基准——
    /// 玩家更新时原版的瓦片碰撞会把卡在物块里的玩家往外推一点，
    /// 若拿被修正过的位置做加法，就变成「推进 40 被拉回一半」，又慢又一顿一顿的。
    /// 整份覆盖之后无论原版怎么推，下一帧都从我们自己的坐标重新开始，穿墙也就必成。
    /// </summary>
    private void MovePlayer(Player player)
    {
        // 被传送 / 被别的机制挪走时重新对齐，别硬把人拽回来
        if (_ridePositionReady && Vector2.DistanceSquared(player.position, _lastAppliedPosition) > TeleportResetDistance * TeleportResetDistance)
            _ridePositionReady = false;

        if (!_ridePositionReady)
        {
            _ridePosition = player.position;
            _ridePositionReady = true;
        }

        _ridePosition += Projectile.velocity;

        // 位置既然整份由我们写，世界边界也得自己钳：原版那套边界处理会被我们覆盖掉
        _ridePosition.X = MathHelper.Clamp(_ridePosition.X, Main.leftWorld + 16f, Main.rightWorld - 32f - player.width);
        _ridePosition.Y = MathHelper.Clamp(_ridePosition.Y, Main.topWorld + 16f, Main.bottomWorld - 32f - player.height);

        player.position = _ridePosition;
        _lastAppliedPosition = _ridePosition;

        player.velocity = Vector2.Zero;                      // 原版别再照速度推一次，否则一帧位移翻倍
        player.fallStart = (int)(player.position.Y / 16f);    // 免得穿地时攒出坠落伤害
    }

    /// <summary>
    /// 本帧的位移 = 鼠标方向 × 速度。速度只看鼠标离屏幕中心多远：
    /// 贴着中心最慢，到屏幕四角吃满，中间线性过渡。
    /// </summary>
    private static Vector2 ComputeFlyStep(Player player)
    {
        Vector2 screenCenter = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
        float distance = Vector2.Distance(Main.MouseScreen, screenCenter);
        float speed = MathHelper.Lerp(MinSpeed, MaxSpeed, MathHelper.Clamp(distance / ScreenHalfDiagonal(), 0f, 1f));

        Vector2 direction = Main.MouseWorld - player.Center;
        if (direction.LengthSquared() < 16f)
            direction = new Vector2(player.direction, 0f);

        return direction.SafeNormalize(new Vector2(player.direction, 0f)) * speed;
    }

    /// <summary>屏幕中心到四角的距离，当速度量程用：只有把鼠标甩到角上才是满速。</summary>
    private static float ScreenHalfDiagonal()
    {
        float halfWidth = Main.screenWidth * 0.5f;
        float halfHeight = Main.screenHeight * 0.5f;
        return MathF.Sqrt(halfWidth * halfWidth + halfHeight * halfHeight);
    }

    /// <summary>残影环：每帧往前推一格，[0] 是当前位置。</summary>
    private void ScrollTrail(Player player)
    {
        Vector2[] oldPos = Projectile.oldPos;
        for (int i = oldPos.Length - 1; i > 0; i--)
        {
            oldPos[i] = oldPos[i - 1];
            Projectile.oldRot[i] = Projectile.oldRot[i - 1];
        }

        oldPos[0] = player.Center;
        Projectile.oldRot[0] = Projectile.rotation;
    }

    /// <inheritdoc />
    public override bool PreDraw(ref Color lightColor)
    {
        Player player = Main.player[Projectile.owner];

        // 化身贴图 = 长按开始时定下来的那把剑；没有剑就退回天顶剑
        int itemType = (int)Projectile.ai[0];
        if (itemType <= ItemID.None || itemType >= TextureAssets.Item.Length || TextureAssets.Item[itemType] == null)
            itemType = ItemID.Zenith;

        Main.instance.LoadItem(itemType);
        Texture2D texture = TextureAssets.Item[itemType].Value;
        if (texture == null)
            return false;

        Vector2 origin = texture.Size() * 0.5f;
        float scale = MathHelper.Clamp(RideSpriteSize / Math.Max(texture.Width, texture.Height), 0.5f, 3f);
        SpriteEffects effects = player.gravDir < 0f ? SpriteEffects.FlipVertically : SpriteEffects.None;

        // 化身带一点自发光，免得在暗处完全看不见自己
        Color color = Color.Lerp(Lighting.GetColor(player.Center.ToTileCoordinates()), Color.White, 0.25f);

        // 残影：从最旧的一帧往近处画，越旧越淡越小
        int length = Projectile.oldPos.Length;
        for (int i = length - 1; i >= 1; i--)
        {
            Vector2 oldPosition = Projectile.oldPos[i];
            if (oldPosition == Vector2.Zero)
                continue;

            float fade = 1f - i / (float)length;
            Main.EntitySpriteDraw(
                texture,
                oldPosition - Main.screenPosition,
                null,
                color * (fade * TrailAlpha),
                Projectile.oldRot[i],
                origin,
                scale * (0.7f + 0.3f * fade),
                effects,
                0);
        }

        Main.EntitySpriteDraw(
            texture,
            player.Center - Main.screenPosition,
            null,
            color,
            Projectile.rotation,
            origin,
            scale,
            effects,
            0);

        return false;
    }

    /// <inheritdoc />
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SilverFlame);
            dust.velocity = Main.rand.NextVector2Circular(3.5f, 3.5f);
            dust.noGravity = true;
            dust.scale = 0.9f;
        }
    }
}
