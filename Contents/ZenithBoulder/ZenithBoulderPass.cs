using System.Collections.Generic;
using Terraria.Localization;
using Terraria.WorldBuilding;
namespace MatterRecord.Contents.ZenithBoulder;
public class ZenithBoulderPass(string str, float value) : GenPass(str, value)
{
    public override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.MatterRecord.Items.ZenithBoulder.Sisyphus");
        var type = ModContent.TileType<ZenithBoulderTile>();

        List<(int, int)> selectedTiles = [];

        for (int x = 0; x < Main.maxTilesX; x++)
            for (int y = 0; y < Main.maxTilesY; y++)
                if (Framing.GetTileSafely(x, y).TileType == TileID.Boulder && !selectedTiles.Contains((x, y)))
                {
                    selectedTiles.Add((x, y));
                    selectedTiles.Add((x + 1, y));
                    selectedTiles.Add((x, y + 1));
                    selectedTiles.Add((x + 1, y + 1));

                    if (WorldGen.genRand.NextBool(WorldGen.noTrapsWorldGen ? 20 : 50))
                    {
                        WorldGen.KillTile(x, y);
                        WorldGen.KillTile(x + 1, y);
                        WorldGen.KillTile(x, y + 1);
                        WorldGen.KillTile(x + 1, y + 1);

                        WorldGen.PlaceTile(x, y, type, true, true);
                        WorldGen.PlaceTile(x + 1, y, type, true, true);
                        WorldGen.PlaceTile(x, y + 1, type, true, true);
                        WorldGen.PlaceTile(x + 1, y + 1, type, true, true);
                    }
                }
    }
}