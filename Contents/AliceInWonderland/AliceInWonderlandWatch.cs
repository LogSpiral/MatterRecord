using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;

namespace MatterRecord.Contents.AliceInWonderland;

public class AliceInWonderlandWatch : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.AliceInWonderland;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.PlatinumWatch);
        Item.accessory = true;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        base.SetDefaults();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (player.whoAmI != Main.myPlayer) return;
        var mplr = player.GetModPlayer<AliceInWonderlandPlayer>();

        if (mplr.CurrentPortalStart.HasValue && Vector2.Distance(mplr.CurrentPortalStart.Value, player.MountedCenter) < 1000)
            return;

        if ((int)(Main.GlobalTimeWrappedHourly * 60) % 60 == 0 && !mplr.PortalSpawnLock && mplr.PortalSpawnedToday < 3 && Main.rand.NextBool(100))
        {
            var end = FindTargetPoint(out bool failed);
            if (failed)
                return;
            var start = FindStartPoint(player.Center, out failed);
            if (failed)
                return;

            mplr.CurrentPortalEnd = end;
            mplr.CurrentPortalStart = start;
            mplr.DustHintTimer = 600;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                mplr.SyncPlayer(-1, player.whoAmI, false);
        }
        base.UpdateAccessory(player, hideVisual);
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.SilverWatch);
        this.RegisterBookRecipe(ItemID.TungstenWatch);
        base.AddRecipes();
    }

    // ---------- 起点生成（上半圆） ----------
    private static Vector2 FindStartPoint(Vector2 currentCenter, out bool failed)
    {
        failed = false;
        Vector2 resultPoint;
        int tryTime = 0;
        bool condition;
        do
        {
            // 角度 0~π（上半部分）
            float angle = Main.rand.NextFloat(0f, MathHelper.Pi);
            Vector2 direction = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            float radius = Main.rand.NextFloat(1200f, 1400f);
            resultPoint = currentCenter + direction * radius;

            tryTime++;
            var coord = resultPoint.ToTileCoordinates();
            condition = false;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    var tile = Framing.GetTileSafely(new Point(coord.X + i, coord.Y + j));
                    condition |= !WorldGen.InWorld(coord.X, coord.Y);
                    condition |= tile.LiquidAmount > 0 && tile.LiquidType != LiquidID.Water;
                    condition |= tile.HasTile;
                    if (condition) break;
                }
            }

            if (!condition)
            {
                bool flag = false;
                for (int n = 1; n < 5; n++)
                {
                    var tile = Framing.GetTileSafely(new Point(coord.X, coord.Y + n));
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        flag = true;
                        break;
                    }
                }
                condition = !flag;
            }
        } while (condition && tryTime < 500);

        if (tryTime >= 500) failed = true;
        return resultPoint;
    }

    // ---------- 终点生成（三层尝试） ----------
    private static Vector2 FindTargetPoint(out bool failed)
    {
        failed = false;

        // 1. 优先尝试未揭示安全点（8次）
        for (int attempt = 0; attempt < 8; attempt++)
        {
            bool canSpawn = false;
            Vector2 candidate = Main.LocalPlayer.CheckForGoodTeleportationSpot(
                ref canSpawn,
                100, Main.maxTilesX - 200,
                100, Main.UnderworldLayer,
                new Player.RandomTeleportationAttemptSettings
                {
                    avoidLava = true,
                    avoidHurtTiles = true,
                    maximumFallDistanceFromOrignalPoint = 100,
                    attemptsBeforeGivingUp = 1000
                });
            if (!canSpawn)
                continue;

            Point coord = candidate.ToTileCoordinates();
            if (!Main.Map.IsRevealed(coord.X, coord.Y) && IsValidExitPoint(candidate))
                return candidate;
        }

        // 2. 尝试普通安全点（不要求未揭示，5次）
        for (int attempt = 0; attempt < 5; attempt++)
        {
            bool canSpawn = false;
            Vector2 candidate = Main.LocalPlayer.CheckForGoodTeleportationSpot(
                ref canSpawn,
                100, Main.maxTilesX - 200,
                100, Main.UnderworldLayer,
                new Player.RandomTeleportationAttemptSettings
                {
                    avoidLava = true,
                    avoidHurtTiles = true,
                    maximumFallDistanceFromOrignalPoint = 100,
                    attemptsBeforeGivingUp = 1000
                });
            if (!canSpawn)
                continue;

            if (IsValidExitPoint(candidate))
                return candidate;
        }

        // 3. 宝箱路径（宝箱或随机后备）
        Chest targetChest = null;
        int tryTime = 0;
        int tryTime2 = 0;
        bool condition;
        do
        {
            targetChest = Main.rand.Next(Main.chest);
            if (targetChest is not null)
                tryTime++;
            else tryTime2++;
            condition = targetChest is null || Main.Map.IsRevealed(targetChest.x, targetChest.y) || targetChest.y < Main.worldSurface;
            if (targetChest != null)
            {
                var tile = Framing.GetTileSafely(targetChest.x, targetChest.y);
                int wall = tile.WallType;
                condition |= wall == 7 || wall == 8 || wall == 9 || wall == 87 || Main.wallDungeon[wall];
            }
        } while (condition && tryTime < 500 && tryTime2 < 5000);

        Vector2 resultPoint;
        if (targetChest != null)
        {
            resultPoint = new Vector2(targetChest.x + 1, targetChest.y) * 16;
            if (IsValidExitPoint(resultPoint))
                return resultPoint;
        }

        // 随机后备点（50次尝试）
        for (int attempt = 0; attempt < 50; attempt++)
        {
            resultPoint = Main.rand.NextVector2FromRectangle(new(160, 160, Main.maxTilesX * 16 - 160, Main.maxTilesY * 16 - 160));
            var coord = resultPoint.ToTileCoordinates();
            var tile = Framing.GetTileSafely(coord);
            bool valid = WorldGen.InWorld(coord.X, coord.Y);
            valid &= !(tile.LiquidAmount > 0 && tile.LiquidType != LiquidID.Water);
            valid &= !tile.HasTile;
            if (valid && IsValidExitPoint(resultPoint))
                return resultPoint;
        }

        // 全部失败
        failed = true;
        return Vector2.Zero;
    }

    // ---------- 验证函数：墙壁排除 + 高度限制 ----------
    private static bool IsValidExitPoint(Vector2 position)
    {
        Point coord = position.ToTileCoordinates();

        // 墙壁排除（7,8,9,87）
        int wall = Framing.GetTileSafely(coord).WallType;
        if (wall == 7 || wall == 8 || wall == 9 || wall == 87)
            return false;

        // 高度检查：正下方最近实心块距离 ≤ 100 像素
        for (int y = coord.Y + 1; y < Main.maxTilesY; y++)
        {
            Tile tile = Framing.GetTileSafely(coord.X, y);
            if (tile.HasTile && Main.tileSolid[tile.TileType])
            {
                float distance = (y - coord.Y) * 16f;
                return distance <= 100f;
            }
        }
        return false; // 未找到实心块
    }
}