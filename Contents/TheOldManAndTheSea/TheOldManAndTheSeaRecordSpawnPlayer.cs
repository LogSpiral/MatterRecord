using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TheOldManAndTheSea;
public class TheOldManAndTheSeaRecordSpawnPlayer : ModPlayer
{
    public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        int index = NPC.FindFirstNPC(NPCID.DukeFishron);
        if (index != -1)
        {
            var npc = Main.npc[index];
            bool flag = true;
            foreach (var player in Main.player)
            {
                if (!player.active || player.dead || Vector2.Distance(player.Center, npc.Center) > 5600f)
                    continue;
                flag = false;
                break;
            }
            if (flag && RecorderSystem.ShouldSpawnRecordItem<TheOldManAndTheSea>())
                Player.QuickSpawnItem(npc.GetItemSource_Loot(), ModContent.ItemType<TheOldManAndTheSea>());

        }
    }
}
