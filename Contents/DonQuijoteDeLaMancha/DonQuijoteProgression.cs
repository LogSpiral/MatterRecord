using Terraria;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

/// <summary>
/// 堂吉诃德进度锁：根据世界 Boss 击杀进度解锁各项能力。
/// 所有属性均为静态，直接读取原版 downed 标记，无需额外同步。
/// </summary>
public static class DonQuijoteProgression
{
    /// <summary>1. 持握时10%概率格挡（接触35%，射弹20%）—— 史莱姆王</summary>
    public static bool Tier1_Block => NPC.downedSlimeKing;

    /// <summary>2. 冲锋后进入突刺状态，可右键取消 —— 克眼</summary>
    public static bool Tier2_StabAfterDash => NPC.downedBoss1;

    /// <summary>3. 受伤时风车嘲讽敌人3秒 —— 邪恶Boss（世界吞噬者/克脑）</summary>
    public static bool Tier3_TauntOnHit => NPC.downedBoss2;

    /// <summary>4. 突刺时获得20%伤害减免，按住↑可位移 —— 蜂后</summary>
    public static bool Tier4_StabDRAndMove => NPC.downedQueenBee;

    /// <summary>5. 造成伤害获得连击，每连击+10%武器大小，上限10 —— 骷髅王</summary>
    public static bool Tier5_ComboSystem => NPC.downedBoss3;

    /// <summary>6. 突刺命中减少冲锋冷却 —— 肉山（困难模式）</summary>
    public static bool Tier6_StabReduceCooldown => Main.hardMode;

    /// <summary>7. 挥砍/突刺命中恢复飞行时间 —— 史莱姆皇后</summary>
    public static bool Tier7_HitRestoreWing => NPC.downedQueenSlime;

    /// <summary>8. 投掷长矛（预留，未实现） —— 任意机械Boss</summary>
    public static bool Tier8_SpearThrow => NPC.downedMechBossAny;

    /// <summary>9. 连击衰减改为-5，上限提升至20 —— 世花</summary>
    public static bool Tier9_ComboDecayAndCap20 => NPC.downedPlantBoss;

    /// <summary>10. 风车摧毁接触的敌对弹幕 —— 石巨人</summary>
    public static bool Tier10_WindmillErase => NPC.downedGolemBoss;

    /// <summary>11. 冲锋摧毁弹幕并回血，且增加连击 —— 猪鲨</summary>
    public static bool Tier11_DashEraseHealCombo => NPC.downedFishron;

    /// <summary>12. 投掷长矛加强（预留） —— 光女</summary>
    public static bool Tier12_SpearUpgrade => NPC.downedEmpressOfLight;

    /// <summary>13. 致命伤害时若连击>15则复活 —— 教徒</summary>
    public static bool Tier13_Revive => NPC.downedAncientCultist;

    /// <summary>14. 风车存在时格挡概率提升 —— 月总</summary>
    public static bool Tier14_BlockBoostWithWindmill => NPC.downedMoonlord;
}