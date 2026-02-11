using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;
namespace MatterRecord.Contents.LittlePrince;

public class LittlePrinceRoseSpawn : GlobalTile
{
    private static bool HasValidGroundForAbigailsFlowerBelowSpot(int x, int y)
    {
        if (!WorldGen.InWorld(x, y, 2))
            return false;
        Tile tile = Framing.GetTileSafely(x, y + 1);
        if (tile == null || !tile.HasTile)
            return false;

        ushort type = tile.TileType;
        if (type < 0)
            return false;

        if (type != TileID.MushroomGrass && type != TileID.AshGrass && !TileID.Sets.Conversion.Grass[type])
            return false;
        return WorldGen.SolidTileAllowBottomSlope(x, y + 1);
    }

    private static bool NoNearbyAbigailsFlower(int i, int j)
    {
        int num = Utils.Clamp(i - 120, 10, Main.maxTilesX - 1 - 10);
        int num2 = Utils.Clamp(i + 120, 10, Main.maxTilesX - 1 - 10);
        int num3 = Utils.Clamp(j - 120, 10, Main.maxTilesY - 1 - 10);
        int num4 = Utils.Clamp(j + 120, 10, Main.maxTilesY - 1 - 10);
        for (int k = num; k <= num2; k++)
        {
            for (int l = num3; l <= num4; l++)
            {
                Tile tile = Framing.GetTileSafely(k, l);
                if (tile.HasTile && tile.TileType == ModContent.TileType<LittlePrinceRose>())
                    return false;
            }
        }

        return true;
    }

    public override void RandomUpdate(int i, int j, int type)
    {
        if (type != TileID.Tombstones) return;
        Vector2 spt = new Vector2(Main.spawnTileX, Main.spawnTileY);
        var l = Vector2.Distance(new Vector2(i, j), spt);
        var m = 0f;
        for (int n = 0; n < 4; n++)
            m = Math.Max(Vector2.Distance(new Vector2(Main.maxTilesX, Main.maxTilesY) * new Vector2(n % 2, n / 2), spt), m);
        int chance = (int)MathHelper.Lerp(5, 60, l / m);
        int times = (int)MathHelper.Lerp(4, 1, l / m);

        for (int k = 0; k < times; k++)
        {
            if (!Main.rand.NextBool(chance)) continue;
            int num2 = WorldGen.genRand.Next(Math.Max(10, i - 10), Math.Min(Main.maxTilesX - 10, i + 10));
            int num3 = WorldGen.genRand.Next(Math.Max(10, j - 10), Math.Min(Main.maxTilesY - 10, j + 10));
            if (HasValidGroundForAbigailsFlowerBelowSpot(num2, num3) && NoNearbyAbigailsFlower(num2, num3) && RecorderSystem.ShouldSpawnRecordItem<LittlePrince>() && WorldGen.PlaceTile(num2, num3, ModContent.TileType<LittlePrinceRose>(), mute: true))
            {
                if (Main.dedServ && Framing.GetTileSafely(num2, num3) != null && Framing.GetTileSafely(num2, num3).HasTile)
                    NetMessage.SendTileSquare(-1, num2, num3);
            }
        }
    }
}
