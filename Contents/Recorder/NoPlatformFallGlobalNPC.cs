using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.Recorder
{
    public class NoPlatformFallGlobalNPC : GlobalNPC
    {
        public override bool? CanFallThroughPlatforms(NPC npc)
        {
            // 永久禁止 Recorder 与 DreamSlime 穿透任何平台
            if (npc.type == ModContent.NPCType<Recorder>() ||
                npc.type == ModContent.NPCType<TheInterpretationOfDreams.DreamSlime>())
                return false;

            // 其他 NPC 保持原版行为（返回 null 表示不干预）
            return null;
        }
    }
}