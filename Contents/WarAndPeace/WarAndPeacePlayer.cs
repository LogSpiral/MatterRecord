using MatterRecord.Contents.WarAndPeace;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

/// <summary>
/// 《战争与和平》饰品的玩家侧逻辑。
/// <para>职责分两块，彼此**解耦**：</para>
/// <para>1. 属性结算：依据装备期间获得的「和平 / 战争」buff 做减伤与增伤。
/// 这部分仍与 buff 绑定 —— 装备在时装栏时饰品不提供 buff，该链路自然失效，符合「时装栏只提供弹幕」的设计。</para>
/// <para>2. 弹幕维持：猫猫弹幕的存续只看「是否装备了战争与和平」（装备栏可见，或时装栏装备），不再依赖 buff。</para>
/// </summary>
public class WarAndPeacePlayer : ModPlayer
{
    /// <summary>
    /// 本帧是否应出现猫猫弹幕。
    /// <para>由 <see cref="WarAndPeace.UpdateAccessory"/>（装备在功能性饰品槽且可见时）或
    /// <see cref="WarAndPeace.UpdateVanity"/>（装备在时装栏时）置为 true。</para>
    /// <para>时装栏固定显示，故时装栏分支不判断可见性；装备栏分支则跟随饰品的可见性开关。</para>
    /// </summary>
    public bool CatVisible;

    /// <summary>
    /// 每帧复位旗标，等待物品钩子重新点亮。
    /// 这是 tModLoader 判断「本帧是否装备了某饰品」的标准写法：先清空、再由物品钩子置位，
    /// 这样取下饰品或切换可见性的下一帧旗标就会自动失效，无需额外的清理逻辑。
    /// </summary>
    public override void ResetEffects()
    {
        CatVisible = false;
        base.ResetEffects();
    }

    /// <summary>
    /// 猫猫弹幕的每帧维持点：只要旗标仍被点亮，就确保场上有对应的猫猫弹幕。
    /// <para>选 <see cref="PostUpdateEquips"/> 是因为它每帧在装备效果结算完毕后才执行，
    /// 此时物品的 <c>UpdateAccessory</c> / <c>UpdateVanity</c> 都已跑过，旗标值一定是最新的。</para>
    /// <para>弹幕生成后自身负责续命（见 <see cref="PeaceCat"/> / <see cref="WarCat"/> 的 AI），
    /// 本方法只在「场上还没有」时补一只，避免每帧重复生成。</para>
    /// </summary>
    public override void PostUpdateEquips()
    {
        // 只有本地玩家负责生成，避免多人模式下各端重复生成同一只猫。
        // 服务器端 Main.myPlayer 不等于任何玩家的 whoAmI，因此不会进入此分支。
        if (Player.whoAmI != Main.myPlayer) return;

        if (!CatVisible || Player.dead) return;

        // 今天该出哪只猫：与饰品提供 buff 的日期规则共用同一份判定，避免两处规则不一致
        int catType = WarAndPeace.IsPeaceDay
            ? ModContent.ProjectileType<PeaceCat>()
            : ModContent.ProjectileType<WarCat>();

        // 场上已有同类猫就不再补。跨日切换时旧猫会因日期条件不满足而自行退场，新猫由这里补上
        if (Player.ownedProjectileCounts[catType] <= 0)
            Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, catType, 0, 0f, Player.whoAmI);
    }

    /// <summary>
    /// 受到敌怪伤害时结算「和平」减伤。
    /// </summary>
    /// <param name="npc">造成伤害的敌怪。</param>
    /// <param name="modifiers">可修改的受伤参数。</param>
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Player.HasBuff<Peace>())
        {
            float reduction = Player.statDefense * 0.1f;   // 防御值 10% 的减免
            modifiers.FinalDamage.Flat -= reduction;
        }
    }

    /// <summary>
    /// 对敌怪造成伤害时结算「战争」增伤。
    /// </summary>
    /// <param name="target">被命中的敌怪。</param>
    /// <param name="modifiers">可修改的命中参数。</param>
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Player.HasBuff<War>())
        {
            Item weapon = Player.HeldItem;
            if (weapon != null && !weapon.IsAir && weapon.damage > 0)
            {
                float extraDamage = weapon.damage * 0.1f;   // 武器基础伤害的 10%
                modifiers.FlatBonusDamage += extraDamage;
            }
        }
    }
}
