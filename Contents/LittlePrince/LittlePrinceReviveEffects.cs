using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace MatterRecord.Contents.LittlePrince;

/// <summary>
/// 小王子死亡拦截触发瞬间的世界效果：推开周围敌怪、反弹敌对弹幕。
/// 只应由权威端调用（单人直接调用，多人由 <see cref="LittlePrinceReviveSync"/> 请求服务器执行），
/// 世界状态的变更全部依靠原版同步链路（SyncNPC / SyncProjectile / 图格广播）下发到各客户端。
/// <para>
/// 这两个效果不是「放一次就完」：它们由 <see cref="LittlePrinceWallSystem"/> 在落罩前的
/// <see cref="LittlePrinceWallSystem.WallSpawnDelayFrames"/> 帧里逐帧重复施加。
/// 单次赋速度会被敌怪自身的 AI 在下一帧覆盖掉，只有逐帧重推才能保证罩子落下时范围内没有敌怪占位。
/// </para>
/// </summary>
public static class LittlePrinceReviveEffects
{
    /// <summary>
    /// 效果判定范围相对罩子外沿额外外扩的像素数。
    /// 判定用「罩子矩形 + 外扩」而不是「中心点距离 ≤ 半径」：体积大的敌怪即使中心点离得远，
    /// 只要 Hitbox 已经伸进罩子范围就必须被推开，否则它压住的格子会砌不上玻璃。
    /// </summary>
    private const float EffectMargin = 48f;

    /// <summary>
    /// 敌怪被推开时的速度大小（像素/帧）。
    /// 取值偏大是为了在落罩前把敌怪顶出去；因为每帧都会重新施加，不依赖单次滑行的距离。
    /// </summary>
    private const float KnockbackSpeed = 24f;

    /// <summary>
    /// 注册一次复活效果：在 <see cref="LittlePrinceWallSystem.WallSpawnDelayFrames"/> 帧的窗口内
    /// 逐帧推开范围内敌怪、反弹敌对弹幕，窗口结束时在玩家位置落罩。
    /// </summary>
    /// <param name="player">触发拦截的玩家（权威端本地玩家）。</param>
    /// <param name="buildWall">是否生成玻璃罩。由请求发起端携带——服务器端读不到远程玩家的饰品可见性设置。</param>
    public static void Apply(Player player, bool buildWall)
    {
        LittlePrinceWallSystem.ScheduleRevive(player, buildWall);
    }

    /// <summary>
    /// 把罩子范围内的敌怪沿「玩家 → 敌怪」方向推开。由
    /// <see cref="LittlePrinceWallSystem"/> 在落罩前的窗口内逐帧调用。
    /// <para>
    /// 直接覆盖 <see cref="Entity.velocity"/> 而不走 <c>NPC.StrikeNPC</c>：后者会带上伤害数字、
    /// 无敌帧与受击 AI 等副作用。代价是 <see cref="NPC.knockBackResist"/> 不参与计算，
    /// 但这里本来就是全量覆盖速度，抗性不该影响结果。
    /// </para>
    /// </summary>
    /// <param name="player">效果中心玩家。</param>
    public static void KnockbackEnemies(Player player)
    {
        Rectangle range = LittlePrinceWallSystem.GetEffectBounds(player.Center, EffectMargin);

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.townNPC || npc.dontTakeDamage)
                continue;
            if (!range.Intersects(npc.Hitbox))
                continue;

            // 与玩家完全重叠时无法归一化方向，退化为按玩家朝向水平推出
            Vector2 direction = npc.Center - player.Center;
            direction = direction == Vector2.Zero
                ? new Vector2(player.direction, 0f)
                : Vector2.Normalize(direction);

            npc.velocity = direction * KnockbackSpeed;
            npc.netUpdate = true; // 服务器端置位后由引擎自动广播 SyncNPC
        }
    }

    /// <summary>
    /// 把罩子范围内的敌对弹幕沿「玩家 → 弹幕」方向弹出去（只改方向、不改速率），
    /// 而不是直接销毁，使其原速飞离玩家。同样由 <see cref="LittlePrinceWallSystem"/> 逐帧调用。
    /// </summary>
    /// <param name="player">效果中心玩家。</param>
    public static void ReflectHostileProjectiles(Player player)
    {
        Rectangle range = LittlePrinceWallSystem.GetEffectBounds(player.Center, EffectMargin);

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile proj = Main.projectile[i];
            if (!proj.active || !proj.hostile)
                continue;
            if (proj.friendly || proj.minion || proj.sentry)
                continue;
            if (Main.projPet[proj.type])
                continue;
            if (!range.Intersects(proj.Hitbox))
                continue;

            float speed = proj.velocity.Length();
            if (speed < 1f)
                speed = 1f; // 静止弹幕极罕见，给个最小速率避免归一化后方向失效

            Vector2 outward = proj.Center - player.Center;
            outward = outward.LengthSquared() < 0.01f
                ? new Vector2(player.direction, 0f) // 与玩家重叠时按玩家朝向弹出
                : Vector2.Normalize(outward);

            // 保持原速率、仅改方向：避免反射后变得更危险（弹幕状态本身不翻转，行为差异最小）
            proj.velocity = outward * speed;
            proj.netUpdate = true; // 敌对弹幕的权威端是服务器，置位后由引擎广播 SyncProjectile
        }
    }
}
