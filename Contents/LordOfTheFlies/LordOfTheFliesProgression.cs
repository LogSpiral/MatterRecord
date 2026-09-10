using Terraria;

namespace MatterRecord.Contents.LordOfTheFlies;

/// <summary>
/// 蝇王进度锁：根据世界 Boss 击杀进度（原版 downed 标记）判定各强化项是否启用。
/// downed 标记为世界级数据，由原版自动保存与同步，无需额外持久化或网络同步。
/// </summary>
public static class LordOfTheFliesProgression
{
    /// <summary>1 命中提高防御 —— 史莱姆王</summary>
    public static bool Tier1_DefenseOnHit => NPC.downedSlimeKing;
    /// <summary>2 湮灭弹 —— 克眼</summary>
    public static bool Tier2_AnnihilationBullet => NPC.downedBoss1;
    /// <summary>3 穿透 2 + 无视 20 护甲 —— 邪恶 Boss</summary>
    public static bool Tier3_Penetration => NPC.downedBoss2;
    /// <summary>4 别西卜协助 —— 蜂后</summary>
    public static bool Tier4_Beelzebub => NPC.downedQueenBee;
    /// <summary>5 暴击额外生命值百分比 —— 骷髅王</summary>
    public static bool Tier5_CritLifePercent => NPC.downedBoss3;
    /// <summary>6 攻速随护甲 —— 肉山</summary>
    public static bool Tier6_AttackSpeed => Main.hardMode;
    /// <summary>7 传送 —— 史莱姆皇后</summary>
    public static bool Tier7_Teleport => NPC.downedQueenSlime;
    /// <summary>8 审判之刃 —— 任意机械 Boss</summary>
    public static bool Tier8_JudgmentBlade => NPC.downedMechBossAny;
    /// <summary>9 湮灭弹额外生命值 + 穿墙 —— 世花</summary>
    public static bool Tier9_AnnihilationLifePercent => NPC.downedPlantBoss;
    /// <summary>10 审判模式 10% 爆炸 —— 石巨人</summary>
    public static bool Tier10_Explosion => NPC.downedGolemBoss;
    /// <summary>11 别西卜弹药追踪 —— 猪鲨</summary>
    public static bool Tier11_Homing => NPC.downedFishron;
    /// <summary>12 源质充盈强化 —— 光女</summary>
    public static bool Tier12_SourceFull => NPC.downedEmpressOfLight;
    /// <summary>13 融合子弹 —— 教徒</summary>
    public static bool Tier13_Fusion => NPC.downedAncientCultist;
    /// <summary>14 不消耗 + 抹杀 —— 月总</summary>
    public static bool Tier14_NoConsume => NPC.downedMoonlord;
}
