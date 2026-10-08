using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.LittlePrince;

/// <summary>
/// 小王子死亡拦截的冷却提示 Buff。
/// 仅用于向玩家展示「复活冷却剩余时间」，不参与任何效果判定——
/// 真正的冷却由 <see cref="LittlePrincePlayer.ReviveCooldownTimer"/> 独立维护，
/// 因此即使此 Buff 被外力清除，冷却也不会提前结束、饰品也不会提前恢复。
/// 视作负面 Buff，且设为护士无法移除，避免玩家在护士处把冷却提示洗掉造成认知混乱。
/// </summary>
public class LittlePrinceWitheredRose : ModBuff
{
    public override void SetStaticDefaults()
    {
        // 负面 Buff：走减益的常规判定与图标着色
        Main.debuff[Type] = true;

        // 不写入存档：冷却本身也不跨存档保留，退出重进后两者状态一致
        Main.buffNoSave[Type] = true;

        // 护士无法移除
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;

        
    }

    // 无任何 Update 逻辑：纯展示
}