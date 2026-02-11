#if false
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.EtherChest;

public class EtherChestPass : ILoadable
{
    private void Detour_Shimmer(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
    {
        orig(self, progress, configuration);
        List<Rectangle> structArea = typeof(StructureMap).GetField("_structures", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenVars.structures) as List<Rectangle>;
        List<Rectangle> protectArea = typeof(StructureMap).GetField("_protectedStructures", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenVars.structures) as List<Rectangle>;
        Rectangle s = structArea.Last();
        Rectangle p = protectArea.Last();
        structArea.Remove(s);
        protectArea.Remove(p);
        var point = GenVars.shimmerPosition.ToPoint();
        for (int i = -1; i < 3; i++)
            for (int j = 0; j < 3; j++)
                WorldGen.KillTile(point.X + i, point.Y + 30 - j, false, false, true);

        for (int i = 0; i < 2; i++)
            WorldGen.PlaceTile(point.X + i * 3 - 1, point.Y + 28, TileID.Torches, false, true, -1, 23);

        int id = WorldGen.PlaceChest(point.X, point.Y + 30, (ushort)ModContent.TileType<EtherChest_Tile>());
        var chest = Main.chest[id];
        chest.item[0] = new Item(ModContent.ItemType<EmeraldTablet.EmeraldTablet>());

        structArea.Add(s);
        protectArea.Add(p);
    }

    public void Load(Mod mod)
    {
        WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Shimmer"], Detour_Shimmer);
    }

    public void Unload()
    { }
}
#endif