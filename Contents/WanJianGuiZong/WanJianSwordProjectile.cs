using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Utilities;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「万剑归宗」的飞剑弹幕：贴图与伤害都取自对应的剑。
/// <para>盘旋阶段每把剑沿自己的一条三维大圆轨道绕玩家飞行：轨道平面（「切面」）与相位由弹幕 identity 决定，
/// 所以看起来是一团绕人乱转的剑球，而不是一圈平铺的剑。近半球的剑更大更亮、画在玩家之前，
/// 远半球的剑更小更暗、画在玩家之后，形成前后穿插的立体感。</para>
/// <para>ai[0] = 对应剑的物品类型（贴图用），ai[1] = 「穿过去」计时（帧），ai[2] = 状态（0 出鞘 / 1 盘旋 / 2 放出）；
/// localAI[0] = 自转偏移角，localAI[1] = 放出首帧标记，localAI[2] = 当前深度（-1 ~ 1）。</para>
/// </summary>
public class WanJianSwordProjectile : ModProjectile
{
    /// <summary>
    /// 弹幕自身的默认贴图：直接引用原版天顶剑的物品贴图，不额外打包 png。
    /// 实际绘制在 <see cref="PreDraw"/> 里被完全接管（画的是每把剑自己的物品贴图），
    /// 这个只是给 tML 资源加载留的落点，正常看不到它。
    /// </summary>
    public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;

    /// <summary>刚从玩家身上飞向自己轨道的阶段。</summary>
    public const float StateLaunching = 0f;

    /// <summary>绕玩家盘旋的阶段。</summary>
    public const float StateOrbiting = 1f;

    /// <summary>松开左键后朝鼠标位置飞出的阶段。</summary>
    public const float StateReleased = 2f;

    /// <summary>放出时的初速度。</summary>
    public const float ReleaseSpeed = 15f;

    /// <summary>放出后的存活时间（帧）。10 秒，够它们追着敌人绕好几圈。</summary>
    private const int ReleasedLifeTime = 600;

    /// <summary>放出后的最后这么多帧用来渐隐，不是啪一下消失。</summary>
    private const int ReleasedFadeTime = 60;

    /// <summary>球壳层数（同心球层，相邻层反向公转）。</summary>
    private const int OrbitShellMax = 3;

    /// <summary>最内层球壳半径。</summary>
    private const float OrbitRadiusBase = 74f;

    /// <summary>相邻球壳之间的半径差。</summary>
    private const float OrbitShellGap = 38f;

    /// <summary>半径随机浮动，免得每层都是一丝不差的球面。</summary>
    private const float OrbitRadiusJitter = 10f;

    /// <summary>半径下限（别糊在玩家脸上）。</summary>
    private const float OrbitRadiusMin = 50f;

    /// <summary>半径上限。</summary>
    private const float OrbitRadiusMax = 240f;

    /// <summary>椭圆轨道的扁率下限（短半轴 / 长半轴），每把剑在 0.35 ~ 1 之间随机。</summary>
    private const float OrbitEllipseMin = 0.35f;

    /// <summary>椭圆长轴在轨道平面内还要再随机转个角度，免得扁的方向都一致。</summary>
    private const float OrbitEllipseTilt = 1f;

    /// <summary>基础公转角速度（弧度 / 秒）。</summary>
    private const float OrbitAngularSpeed = 1.9f;

    /// <summary>每把剑的公转速度倍率浮动范围（0.75 ~ 1.6 倍）。</summary>
    private const float OrbitSpeedMin = 0.75f;
    private const float OrbitSpeedRange = 0.85f;

    /// <summary>公转速度脉动：幅度与频率，让剑时快时慢而不是匀速转圈。</summary>
    private const float OrbitPulseAmount = 0.55f;
    private const float OrbitPulseFrequency = 1.6f;

    /// <summary>半径吐纳：幅度与频率，剑会自己一进一出地画螺旋。</summary>
    private const float OrbitBreatheAmount = 0.14f;
    private const float OrbitBreatheFrequency = 0.9f;

    /// <summary>轨道平面绕镜头轴慢慢进动的最大角速度（弧度 / 秒），路线会缓缓漂移。</summary>
    private const float OrbitPrecessionSpeed = 0.35f;

    /// <summary>自转角度增量（弧度 / 帧）。0 = 剑身始终顺着飞行方向；改成正数就是边盘旋边翻滚。</summary>
    private static readonly float OrbitSpinSpeed = 0f;

    /// <summary>跟随轨道目标点的最大速度，避免重新分布时瞬移。</summary>
    private const float OrbitMaxSpeed = 30f;

    /// <summary>近处放大、远处缩小的幅度。</summary>
    private const float OrbitDepthScale = 0.28f;

    /// <summary>拖尾长度（帧）。</summary>
    private const int TrailLength = 6;

    /// <summary>放出后的追踪索敌半径。</summary>
    private const float HomingRange = 640f;

    /// <summary>远距离追踪时的飞行速度上限。</summary>
    private const float HomingSpeedFar = 26f;

    /// <summary>贴到目标附近时的飞行速度下限（要高于多数敌人的逃跑速度，否则会等速僵持追不上）。</summary>
    private const float HomingSpeedNear = 14f;

    /// <summary>速度随距离的缩放：speed = 距离 × 这个系数（再夹到上下限之间）。</summary>
    private const float HomingApproachRatio = 0.18f;

    /// <summary>
    /// 转弯半径随距离的缩放：转弯半径 = 距离 × 这个系数（再夹到上下限之间）。
    /// 只要转弯半径明显小于到目标的距离，追踪曲线就是收敛的；否则剑会绕着敌人兜圈。
    /// </summary>
    private const float HomingTurnRadiusRatio = 0.15f;

    /// <summary>转弯半径的下限（贴脸时的最小圈）。</summary>
    private const float HomingTurnRadiusMin = 7f;

    /// <summary>转弯半径的上限。</summary>
    private const float HomingTurnRadiusMax = 80f;

    /// <summary>每帧转向量的上限，免得远远地就开始剧烈拐弯。</summary>
    private const float HomingTurnRateMax = 0.9f;

    /// <summary>离目标这么近就不再修正方向，直接捅过去（否则会绕着敌人贴着转）。</summary>
    private const float PassThroughRange = 60f;

    /// <summary>穿过去之后保持直线飞多久（帧），飞完再回头找下一个目标。</summary>
    private const float PassThroughTime = 20f;

    /// <summary>同一帧内共享的追踪目标，避免每把剑都扫一遍 NPC。</summary>
    private static NPC _homingTarget;
    private static uint _homingTick = uint.MaxValue;

    /// <inheritdoc />
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }

    /// <inheritdoc />
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;          // 无限穿透，命中不消失
        Projectile.timeLeft = 90;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;

        // 让服务器转发这些剑的状态时不受「离玩家 5000 像素以内」的距离过滤影响，
        // 多人下队友离得远也能看到放出后的飞剑
        Projectile.netImportant = true;
    }

    /// <inheritdoc />
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead)
        {
            Projectile.Kill();
            return;
        }

        if (Projectile.ai[2] == StateReleased)
            UpdateReleased(owner);
        else
            UpdateOrbit(owner);

        // 蓄力（出鞘 / 盘旋）阶段不造成伤害，只有松手放出去之后才打人。
        // friendly 本身不参与 SyncProjectile 同步，但各端都由同步过来的 ai[2] 推出同一个值，不会跑偏。
        Projectile.friendly = Projectile.ai[2] == StateReleased;

        // 前半球的剑画在玩家之前，后半球的剑画在玩家之后
        Projectile.hide = Projectile.localAI[2] > 0f;

        Lighting.AddLight(Projectile.Center, 0.25f, 0.25f, 0.4f);
    }

    /// <summary>出鞘 / 盘旋阶段：沿自己的轨道平面公转，剑身顺着飞行方向。</summary>
    private void UpdateOrbit(Player owner)
    {
        // 每把剑的个性：相邻 identity 之间也完全无关
        int id = Projectile.identity * 31 + Projectile.owner * 7;

        int shell = (int)(Hash01(id, 1) * OrbitShellMax);
        float radiusBase = MathHelper.Clamp(
            OrbitRadiusBase + shell * OrbitShellGap + (Hash01(id, 2) * 2f - 1f) * OrbitRadiusJitter,
            OrbitRadiusMin,
            OrbitRadiusMax);

        // 任意切面上的轨道 = 随机一个单位法线，再用它张成一组正交基
        float yaw = Hash01(id, 3) * MathHelper.TwoPi;
        float pitch = MathF.Acos(Hash01(id, 4) * 2f - 1f);
        Vector3 normal = new Vector3(
            MathF.Sin(pitch) * MathF.Cos(yaw),
            MathF.Sin(pitch) * MathF.Sin(yaw),
            MathF.Cos(pitch));

        // 进动：轨道平面绕镜头轴慢慢转，剑的路线会缓缓漂移，不会一直贴着同一条圆
        float precession = (float)Main.GlobalTimeWrappedHourly * (Hash01(id, 9) * 2f - 1f) * OrbitPrecessionSpeed;
        float precessionCos = MathF.Cos(precession);
        float precessionSin = MathF.Sin(precession);
        normal = new Vector3(
            normal.X * precessionCos - normal.Y * precessionSin,
            normal.X * precessionSin + normal.Y * precessionCos,
            normal.Z);

        Vector3 helper = MathF.Abs(normal.Z) < 0.9f ? Vector3.UnitZ : Vector3.UnitX;
        Vector3 axisU = Vector3.Normalize(Vector3.Cross(helper, normal));
        Vector3 axisV = Vector3.Cross(normal, axisU);

        // 椭圆轨道：长轴 axisU、短半轴在 0.35~1 倍之间随机，长轴再在平面里随机转个角度
        float semiMajor = radiusBase;
        float semiMinor = semiMajor * MathHelper.Lerp(OrbitEllipseMin, 1f, Hash01(id, 11));
        float ellipseTilt = Hash01(id, 12) * OrbitEllipseTilt;
        float tiltCos = MathF.Cos(ellipseTilt);
        float tiltSin = MathF.Sin(ellipseTilt);
        Vector3 majorAxis = axisU * tiltCos + axisV * tiltSin;
        Vector3 minorAxis = -axisU * tiltSin + axisV * tiltCos;

        // 相邻球壳反向交错，速度倍率每把剑不同，层与层、剑与剑都会互相错切
        float direction = shell % 2 == 0 ? 1f : -1f;
        float angularSpeed = OrbitAngularSpeed * direction * (OrbitSpeedMin + Hash01(id, 5) * OrbitSpeedRange);

        float time = (float)Main.GlobalTimeWrappedHourly;

        // 公转相位带上正弦脉动：角速度在 (1±OrbitPulseAmount) 倍之间来回，剑会时快时慢
        float phaseOffset = Hash01(id, 6) * MathHelper.TwoPi;
        float pulsePhase = Hash01(id, 8) * MathHelper.TwoPi;
        float phase = phaseOffset + angularSpeed *
            (time + OrbitPulseAmount * MathF.Sin(time * OrbitPulseFrequency + pulsePhase) / OrbitPulseFrequency);

        // 半径吐纳：一进一出地画螺旋
        float breathePhase = Hash01(id, 7) * MathHelper.TwoPi;
        float breathe = 1f + OrbitBreatheAmount * MathF.Sin(time * OrbitBreatheFrequency + breathePhase);

        Vector3 OrbitPoint(float at)
        {
            float cos = MathF.Cos(at);
            float sin = MathF.Sin(at);
            return majorAxis * (semiMajor * cos * breathe) + minorAxis * (semiMinor * sin * breathe);
        }

        Vector3 offset = OrbitPoint(phase);
        Vector3 ahead = OrbitPoint(phase + 0.02f * MathF.Sign(angularSpeed));

        Vector2 target = owner.Center + new Vector2(offset.X, offset.Y);
        Vector2 tangent = new Vector2(ahead.X - offset.X, ahead.Y - offset.Y);

        // 屏幕纵轴向下、Z 轴指向镜头；深度存下来给绘制阶段做前后缩放与绘制顺序
        Projectile.localAI[2] = MathHelper.Clamp(offset.Z / semiMajor, -1f, 1f);

        Vector2 delta = target - Projectile.Center;

        if (Projectile.ai[2] == StateLaunching)
        {
            Projectile.velocity = delta * 0.12f;
            if (delta.Length() < 24f)
            {
                Projectile.ai[2] = StateOrbiting;
                Projectile.netUpdate = true;
            }
        }
        else
        {
            // 跟随力度也每把剑略不同，收轨道的动作有快有慢
            Vector2 velocity = delta * (0.3f + Hash01(id, 10) * 0.25f);
            float speed = velocity.Length();
            if (speed > OrbitMaxSpeed)
                velocity = velocity / speed * OrbitMaxSpeed;

            Projectile.velocity = velocity;
        }

        // 盘旋期间不会自然消失，只有松手或玩家死亡才会进入放出阶段
        Projectile.timeLeft = 90;

        // 沿轨道切线方向摆正剑身。投影后的切线在「剑正朝镜头里/外飞」时会短到接近 0，
        // 这时候保持原朝向、并用 AngleLerp 过渡，免得剑身翻个 180 度。
        // 自转默认关闭；把 OrbitSpinSpeed 调大就会边盘旋边翻滚。
        if (OrbitSpinSpeed != 0f)
            Projectile.localAI[0] = MathHelper.WrapAngle(Projectile.localAI[0] + OrbitSpinSpeed * MathF.Sign(angularSpeed));

        float minTangent = semiMajor * 0.002f;
        float desiredRotation = tangent.LengthSquared() > minTangent * minTangent
            ? tangent.ToRotation() + MathHelper.PiOver4 + Projectile.localAI[0]
            : Projectile.rotation;

        Projectile.rotation = Projectile.rotation.AngleLerp(desiredRotation, 0.35f);
    }

    /// <summary>
    /// 由 identity 派生 [0,1) 的随机量。
    /// <para>这里不用 <see cref="UnifiedRandom"/>：.NET 那套 Random 对等差种子（identity 正好是 0,1,2…）
    /// 的前几个输出高度相关——实测 yaw 只落在两条相差 0.3125 的窄带上，十几把剑的轨道平面会几乎重合，
    /// 看上去就只有两条大轨迹。换成整数哈希后，相邻 identity 之间也是完全无关的。</para>
    /// </summary>
    private static float Hash01(int seed, int salt)
    {
        unchecked
        {
            uint x = (uint)seed * 2654435761u + (uint)salt * 2246822519u;
            x ^= x >> 16;
            x *= 0x7FEB352Du;
            x ^= x >> 15;
            x *= 0x846CA68Bu;
            x ^= x >> 16;
            return (x >> 8) * (1f / 16777216f);
        }
    }

    /// <summary>放出阶段：沿松手定下的方向加速飞行，并追踪附近的敌人，剑身摆正到飞行方向。</summary>
    private void UpdateReleased(Player owner)
    {
        if (Projectile.localAI[1] == 0f)
        {
            Projectile.localAI[1] = 1f;
            Projectile.timeLeft = ReleasedLifeTime;
        }

        // 放出的剑都在镜头前，画在玩家之前
        Projectile.localAI[2] = 1f;

        if (Projectile.velocity == Vector2.Zero)
            Projectile.velocity = Vector2.UnitX * ReleaseSpeed;

        // 追踪：每帧朝最近的敌人偏转。转弯半径按到目标的距离缩放——
        // 固定转向量配高速会让转弯半径（v / ω）远大于敌人身位，
        // 剑就只会绕着敌人兜大圈，看上去是一条圆形大轨迹。
        // ai[1] 是「穿过去」计时：命中或贴脸后先直线飞一小段，别黏在敌人身上反复摩擦。
        if (Projectile.ai[1] > 0f)
        {
            Projectile.ai[1] -= 1f;

            Vector2 heading = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Projectile.velocity = heading * MathHelper.Min(Projectile.velocity.Length() * 1.02f, HomingSpeedFar * 1.4f);
        }
        else
        {
            NPC target = FindHomingTarget(owner);
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();

                if (distance < PassThroughRange)
                    Projectile.ai[1] = PassThroughTime;

                float turnRadius = MathHelper.Clamp(distance * HomingTurnRadiusRatio, HomingTurnRadiusMin, HomingTurnRadiusMax);
                float speed = MathHelper.Clamp(distance * HomingApproachRatio, HomingSpeedNear, HomingSpeedFar);
                float turnRate = MathHelper.Clamp(speed / turnRadius, 0f, HomingTurnRateMax);

                Vector2 heading = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                Vector2 desired = toTarget.SafeNormalize(heading);
                float newAngle = heading.ToRotation().AngleTowards(desired.ToRotation(), turnRate);

                Projectile.velocity = newAngle.ToRotationVector2() * speed;
            }
            else
            {
                // 附近没有敌人：沿松手那条直线继续加速飞
                Projectile.velocity *= 1.015f;
                float maxSpeed = ReleaseSpeed * 2.2f;
                if (Projectile.velocity.LengthSquared() > maxSpeed * maxSpeed)
                    Projectile.velocity = Vector2.Normalize(Projectile.velocity) * maxSpeed;
            }
        }

        float desiredRotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        Projectile.rotation = Projectile.rotation.AngleLerp(desiredRotation, 0.35f);
    }

    /// <summary>找玩家附近最近的敌人（每帧只扫一次，所有飞剑共用）。</summary>
    private static NPC FindHomingTarget(Player owner)
    {
        if (_homingTick == Main.GameUpdateCount)
            return _homingTarget;

        _homingTick = Main.GameUpdateCount;
        _homingTarget = null;

        float bestDistance = HomingRange * HomingRange;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || !npc.CanBeChasedBy())
                continue;

            float distance = Vector2.DistanceSquared(npc.Center, owner.Center);
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            _homingTarget = npc;
        }

        return _homingTarget;
    }

    /// <inheritdoc />
    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (Projectile.hide)
            overPlayers.Add(index);
    }

    /// <inheritdoc />
    public override bool PreDraw(ref Color lightColor)
    {
        int itemType = (int)Projectile.ai[0];
        if (itemType <= ItemID.None || itemType >= TextureAssets.Item.Length || TextureAssets.Item[itemType] == null)
            return false;

        Main.instance.LoadItem(itemType);
        Texture2D texture = TextureAssets.Item[itemType].Value;
        if (texture == null)
            return false;

        Vector2 origin = texture.Size() * 0.5f;
        float baseScale = MathHelper.Clamp(38f / Math.Max(texture.Width, texture.Height), 0.5f, 1.3f);

        float depth = MathHelper.Clamp(Projectile.localAI[2], -1f, 1f);
        float depth01 = depth * 0.5f + 0.5f;                       // 0 = 最远，1 = 最近
        float depthScale = 1f + depth * OrbitDepthScale;
        Color color = lightColor * (0.45f + 0.55f * depth01);

        // 放出后的最后一段时间渐隐
        if (Projectile.ai[2] == StateReleased)
            color *= MathHelper.Clamp(Projectile.timeLeft / (float)ReleasedFadeTime, 0f, 1f);

        // 拖尾：把沿途经过的位置画成一串逐渐变淡的剑影
        Vector2 centerOffset = new Vector2(Projectile.width, Projectile.height) * 0.5f;
        for (int i = TrailLength - 1; i >= 0; i--)
        {
            Vector2 oldPosition = Projectile.oldPos[i];
            if (oldPosition == Vector2.Zero)
                continue;

            float fade = 1f - (i + 1f) / (TrailLength + 1f);
            Main.EntitySpriteDraw(
                texture,
                oldPosition + centerOffset - Main.screenPosition,
                null,
                color * fade * 0.35f,
                Projectile.oldRot[i],
                origin,
                baseScale * depthScale * (0.85f + 0.15f * fade),
                SpriteEffects.None,
                0);
        }

        Main.EntitySpriteDraw(
            texture,
            Projectile.Center - Main.screenPosition,
            null,
            color,
            Projectile.rotation,
            origin,
            baseScale * depthScale,
            SpriteEffects.None,
            0);

        return false;
    }

    /// <inheritdoc />
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // 命中后立刻进入「穿过去」状态：不再修正方向，直接飞过目标再回头
        if (Projectile.ai[2] == StateReleased && Projectile.ai[1] <= 0f)
            Projectile.ai[1] = PassThroughTime;
    }

    /// <inheritdoc />
    public override void OnKill(int timeLeft)
    {
        Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.6f);

        for (int i = 0; i < 6; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SilverFlame);
            dust.velocity = Main.rand.NextVector2Circular(3.5f, 3.5f);
            dust.noGravity = true;
            dust.scale = 0.8f;
        }
    }
}
