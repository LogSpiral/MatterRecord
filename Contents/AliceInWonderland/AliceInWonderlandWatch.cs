using Microsoft.Xna.Framework;
using Terraria.ID;

namespace MatterRecord.Contents.AliceInWonderland;
internal class AliceInWonderlandWatch : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.PlatinumWatch}";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.PlatinumWatch);
        Item.accessory = true;

        base.SetDefaults();
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (player.whoAmI != Main.myPlayer) return;
        var mplr = player.GetModPlayer<AliceInWonderlandPlayer>();
        if ((int)Main.time % 60 == 0 && !mplr.PortalSpawnLock && mplr.PortalSpawnedToday < 3 && Main.rand.NextBool(180))
        {
            mplr.CurrentPortalEnd = FindTargetPoint();
            mplr.CurrentPortalStart = FindStartPoint(player.Center);
            mplr.DustHintTimer = 600;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                mplr.SyncPlayer(-1, player.whoAmI, false);
        }
        base.UpdateAccessory(player, hideVisual);
    }

    static Vector2 FindStartPoint(Vector2 currentCenter)
    {
        Vector2 resultPoint;
        int tryTime = 0;
        bool condition;
        do
        {
            resultPoint = currentCenter + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1200, 1400);
            tryTime++;
            var coord = resultPoint.ToTileCoordinates();
            var tile = Framing.GetTileSafely(coord);
            condition = !WorldGen.InWorld(coord.X, coord.Y);
            condition |= tile.LiquidAmount > 0 && tile.LiquidType != LiquidID.Water;
            condition |= tile.HasTile;

            if (!condition) 
            {
                bool flag = false;
                for (int n = 1; n < 5; n++) 
                {
                    tile = Framing.GetTileSafely(new Point(coord.X, coord.Y + n));
                    if (tile.HasTile)
                    {
                        flag = true;
                        break;
                    }
                }
                condition = !flag;
            }

        } while (condition && tryTime < 50);

        return resultPoint;
    }

    static Vector2 FindTargetPoint()
    {
        Chest targetChest;
        int tryTime = 0;
        bool condition;
        do
        {
            targetChest = Main.rand.Next(Main.chest);
            if (targetChest is not null)
                tryTime++;
            condition = targetChest is null || Main.Map.IsRevealed(targetChest.x, targetChest.y) || targetChest.y < Main.worldSurface;
            if (targetChest != null)
            {
                var tile = Framing.GetTileSafely(targetChest.x, targetChest.y);
                condition |= tile.wall == 87 || Main.wallDungeon[tile.wall];
            }
        } while (condition && tryTime < 50);

        Vector2 resultPoint;
        if (targetChest is null)
        {
            tryTime = 0;
            do
            {
                resultPoint = Main.rand.NextVector2FromRectangle(new(160, 160, Main.maxTilesX * 16 - 160, Main.maxTilesY * 16 - 160));
                tryTime++;
                var coord = resultPoint.ToTileCoordinates();
                var tile = Framing.GetTileSafely(coord);
                condition = !WorldGen.InWorld(coord.X, coord.Y);
                condition |= tile.LiquidAmount > 0 && tile.LiquidType != LiquidID.Water;
                condition |= tile.HasTile;

            } while (condition && tryTime < 50);
        }
        else
            resultPoint = new Vector2(targetChest.x + 1, targetChest.y) * 16;
        return resultPoint;
    }
}
