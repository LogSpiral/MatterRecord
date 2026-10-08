using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace MatterRecord.Contents.LittlePrince;

/// <summary>
/// 小王子死亡拦截触发瞬间的世界效果：推开周围敌怪、反弹敌对弹幕、在玩家周围立起临时玻璃罩。
/// 只应由权威端调用（单人直接调用，多人由 <see cref="LittlePrinceReviveSync"/> 请求服务器执行），
/// 世界状态的变更全部依靠原版同步链路（SyncNPC / SyncProjectile / 图格广播）下发到各客户端。
/// </summary>
public static class LittlePrinceReviveEffects
{
    /// <summary>效果作用半径（像素）：敌怪击退与弹幕反弹的判定范围。</summary>
    private const float EffectRadius = 160f;

    /// <summary>
    /// 敌怪被推开时的速度大小（像素/帧）。参照 RedDoubleClickProj 的写法，
    /// 方向取「玩家 → 敌怪」的归一化向量，因此这里是速度模长而非仅水平分量。
    /// 取值偏大是为了让敌怪在围墙落下前飞得更远，彻底脱离围墙占位。
    /// </summary>
    private const float KnockbackSpeed = 24f;

    /// <summary>
    /// 依次执行复活瞬间的三段效果：推开敌怪 → 反弹敌弹 → 排队砌罩。
    /// 砌罩本身由 <see cref="LittlePrinceWallSystem.ScheduleWall"/> 延迟若干帧执行，
    /// 让击退先把敌怪推出去，避免罩子落下时和敌怪重叠（既保护玩家，也不会出现漏格）。
    /// 若玩家关闭了饰品可见性，则跳过砌罩——击退与弹幕反弹仍然生效。
    /// </summary>
    /// <param name="player">触发拦截的玩家（权威端本地玩家）。</param>
    /// <param name="buildWall">是否生成玻璃罩。由请求发起端携带——服务器端读不到远程玩家的饰品可见性设置。</param>
    public static void Apply(Player player, bool buildWall)
    {
        // 矩形粗筛 + 距离精筛：先用外接正方形快速排除，再做圆形判定，避免逐实体开方
        Rectangle range = Utils.CenteredRectangle(player.Center, new Vector2(EffectRadius * 2f));

        // 先推怪：确保敌怪在罩子落下之前被推出
        KnockbackEnemies(player, range);
        ReflectHostileProjectiles(player, range);

        // 只有饰品可见时才生成玻璃罩（可见性由请求端告知，权威端无法自行判断）
        if (buildWall)
            LittlePrinceWallSystem.ScheduleWall(player);
    }

    /// <summary>
    /// 把范围内的敌怪沿「玩家 → 敌怪」方向推开。
    /// 采用与 <c>RedDoubleClickProj</c> 相同的做法：临时把 <see cref="NPC.knockBackResist"/>
    /// 压到 0.01f 以突破抗性，设置速度后立刻恢复原值，避免污染敌怪状态。
    /// </summary>
    /// <param name="player">效果中心玩家。</param>
    /// <param name="range">粗筛用的正方形范围。</param>
    private static void KnockbackEnemies(Player player, Rectangle range)
    {
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.townNPC || npc.dontTakeDamage)
                continue;
            if (!range.Intersects(npc.Hitbox))
                continue;
            if (Vector2.DistanceSquared(player.Center, npc.Center) > EffectRadius * EffectRadius)
                continue;

            // 与玩家完全重叠时无法归一化方向，退化为按玩家朝向水平推出
            Vector2 direction = npc.Center - player.Center;
            direction = direction == Vector2.Zero
                ? new Vector2(player.direction, 0f)
                : Vector2.Normalize(direction);

            // 临时压低击退抗性 → 施加击退 → 立刻恢复（参照 RedDoubleClickProj）
            float originalResist = npc.knockBackResist;
            npc.knockBackResist = 0.01f;

            npc.velocity = direction * KnockbackSpeed;

            npc.knockBackResist = originalResist;

            npc.netUpdate = true; // 服务器端置位后由引擎自动广播 SyncNPC
        }
    }

    /// <summary>
    /// 把范围内的敌对弹幕沿「玩家 → 弹幕」的反方向弹出去（只改方向、不改速率），
    /// 而不是直接销毁，使其原速飞离玩家。
    /// </summary>
    /// <param name="player">效果中心玩家。</param>
    /// <param name="range">粗筛用的正方形范围。</param>
    private static void ReflectHostileProjectiles(Player player, Rectangle range)
    {
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
            if (Vector2.DistanceSquared(player.Center, proj.Center) > EffectRadius * EffectRadius)
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