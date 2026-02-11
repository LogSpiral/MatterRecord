using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ObjectData;

namespace MatterRecord.Contents.ZenithBoulder;
public class ZenithBoulderTile : ModTile
{
    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Projectile.NewProjectile(new EntitySource_TileBreak(i, j), (float)(i * 16) + 15.5f, j * 16 + 16, 0f, 0f, ModContent.ProjectileType<ZenithBoulderProjectile>(), 30, 10f, Main.myPlayer);

        base.KillMultiTile(i, j, frameX, frameY);
    }

    public override bool IsTileDangerous(int i, int j, Player player) => true;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = true;
        DustType = DustID.MoonBoulder; // No dust when mined.

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(152, 171, 198), Language.GetText("MapObject.Trap"));
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j) => null;
}
