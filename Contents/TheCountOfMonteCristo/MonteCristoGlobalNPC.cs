using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheCountOfMonteCristo;

/// <summary>
/// 「基督山伯爵」的击杀获取判定。
/// <para>玩家击杀一只「曾拾取过玩家掉落金币」的敌怪时，若该敌怪位于玩家最近一次死亡点
/// 为中心的 1024×1024 像素区域（死亡点 ±512）内，则掉落一件《基督山伯爵》。</para>
/// </summary>
public class MonteCristoGlobalNPC : GlobalNPC
{
    /// <summary>
    /// 敌怪死亡时执行获取判定并产出《基督山伯爵》。
    /// </summary>
    /// <param name="npc">刚刚死亡的敌怪。</param>
    public override void OnKill(NPC npc)
    {
        // 多人模式下由服务器统一裁决并广播掉落，客户端只接收同步结果；
        // 若各端都自行生成会导致同一只敌怪掉出多件。
        if (Main.netMode == NetmodeID.MultiplayerClient) return;

        // 只处理「带金币的敌怪」：原版敌怪拾取玩家掉落的金币后，会把金币折算成铜币的
        // 价值累加到 extraValue 上，因此 extraValue > 0 即表示它抢过玩家的钱。
        if (npc.extraValue <= 0f) return;

        // 友好单位（城镇 NPC、宠物等）不是玩家讨伐的对象，排除以免误触发
        if (npc.friendly || npc.townNPC) return;

        // 定位击杀者：沿用项目既有写法，兼容武器 / 弹幕 / 召唤物等所有击杀方式
        if (npc.lastInteraction == 255 || Main.player[npc.lastInteraction] is not { active: true } player) return;

        var mp = player.GetModPlayer<MonteCristoPlayer>();
        if (!mp.HasLastDeathPosition) return;

        // 以死亡点为中心的 1024×1024 区域判定：按 X / Y 分量分别比较，
        // 等价于「矩形半边长 512 像素」的正方形范围（比圆形更贴合用户描述的 1024×1024）。
        Vector2 delta = npc.Center - mp.LastDeathPosition;
        if (Math.Abs(delta.X) > MonteCristoPlayer.RecordKillRangeHalfSize
            || Math.Abs(delta.Y) > MonteCristoPlayer.RecordKillRangeHalfSize) return;

        // 复用记录系统既有的防重复（已持有则不生成）与冷却机制，避免短时间内重复产出
        if (!RecorderSystem.ShouldSpawnRecordItem<TheCountOfMonteCristo>()) return;

        player.QuickSpawnItem(npc.GetItemSource_Loot(), ModContent.ItemType<TheCountOfMonteCristo>());
        RecorderSystem.SetCooldown<TheCountOfMonteCristo>();
    }
}
