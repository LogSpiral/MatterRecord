using MatterRecord.Contents.Recorder;
namespace MatterRecord.Contents.TheOldManAndTheSea;
public class TheOldManAndTheSeaRecordSpawnNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        if (npc.type != NPCID.DukeFishron) return;
        if (npc.lastInteraction != 255 
            && Main.player[npc.lastInteraction] is { active: true } player 
            && RecorderSystem.ShouldSpawnRecordItem<TheOldManAndTheSea>())
            player.QuickSpawnItem(npc.GetItemSource_Loot(), ModContent.ItemType<TheOldManAndTheSea>());
    }
}
