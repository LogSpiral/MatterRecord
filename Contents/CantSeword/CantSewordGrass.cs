using System;
using System.Linq;
using Terraria.DataStructures;

namespace MatterRecord.Contents.CantSeword;

public class CantSewordGrass : GlobalTile
{
    private static readonly int[] grassTypes = [3, 24, 61, 110, 201, 529, 637, 73, 74, 113];

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        bool flag = grassTypes.Contains(Framing.GetTileSafely(i, j).TileType);
        if (!flag || !Main.rand.NextBool(5000)) return;
        int number = Item.NewItem(new EntitySource_ItemUse(Main.LocalPlayer, Main.LocalPlayer.HeldItem), i * 16, j * 16, 16, 16, ModContent.ItemType<CantSeword>());
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
        base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
    }
}
