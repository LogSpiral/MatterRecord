using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ObjectData;

namespace MatterRecord.Contents.LittlePrince;

public class LittlePrinceRose : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        //Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(128, 128, 128), name);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.AnchorValidTiles = [
            TileID.Grass,
            TileID.HallowedGrass,
            TileID.CorruptGrass,
            TileID.AshGrass,
            TileID.CrimsonGrass,
            TileID.MushroomGrass,
            TileID.GolfGrass,
            TileID.GolfGrassHallowed
        ];
        TileObjectData.newTile.AnchorAlternateTiles = [
            TileID.ClayPot,
            TileID.PlanterBox
        ];
        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        return [new Item(ModContent.ItemType<LittlePrince>())];
    }
}
