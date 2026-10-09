using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;
using Terraria.DataStructures;
using static Terraria.ModLoader.BackupIO;

namespace MatterRecord.Contents.LittlePrince;

public class LittlePrinceAbigailFlowerDrop : GlobalTile
{
    public override void Drop(int i, int j, int type)
    {
        if (type == TileID.AbigailsFlower && RecorderSystem.ShouldSpawnRecordItem<LittlePrince>())
        {
            int index = Item.NewItem(new EntitySource_TileBreak(i, j, "MatterRecord: LittlePrinceDrop"), new Rectangle(i * 16, j * 16, 16, 16), ModContent.ItemType<LittlePrince>());
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
            RecorderSystem.SetCooldown<LittlePrince>();
        }
    }
}
