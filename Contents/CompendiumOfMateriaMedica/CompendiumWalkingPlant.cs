using System.Collections.Generic;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

/// <summary>
/// 行走种植功能：玩家装备本草纲目（或同队共享效果）且背包有草药种子时，
/// 在行走过程中在脚下的可种植位置（下方为特定可种植物块或种植盆）种植对应草药，并消耗种子。
/// 普通种植概率 1/20，手持再生法杖(213)或草镐(5295)时概率100%。
/// </summary>
public class CompendiumWalkingPlant : ModPlayer
{
    // 下方物块类型 -> 草药样式映射表（仅这些物块可触发种植）
    private static readonly Dictionary<int, int> TileToHerb = [];

    // 所有草药样式列表（用于种植盆随机生成）
    private static readonly int[] AllHerbStyles =
    [
        0, // 太阳花
        1, // 月光草
        2, // 闪耀根
        3, // 死亡草
        4, // 水叶草
        5, // 火焰花
        6  // 寒颤棘
    ];

    static CompendiumWalkingPlant()
    {
        // 初始化映射表：只有这些物块才允许种植，并指定对应的草药样式
        foreach (int tile in new[] { 2, 109, 477, 492 }) TileToHerb[tile] = 0; // 太阳花
        TileToHerb[60] = 1; // 月光草
        foreach (int tile in new[] { 0, 59 }) TileToHerb[tile] = 2; // 闪耀根
        foreach (int tile in new[] { 23, 661, 199, 662, 25, 203 }) TileToHerb[tile] = 3; // 死亡草
        foreach (int tile in new[] { 53, 116 }) TileToHerb[tile] = 4; // 水叶草
        foreach (int tile in new[] { 57, 633 }) TileToHerb[tile] = 5; // 火焰花
        foreach (int tile in new[] { 147, 161, 163, 164, 200 }) TileToHerb[tile] = 6; // 寒颤棘
    }

    /// <summary>
    /// 每帧更新，在玩家移动时尝试种植。
    /// </summary>
    public override void PostUpdate()
    {
        // 只处理装备了本草纲目或同队有人装备的情况
        var compPlayer = Player.GetModPlayer<CompendiumPlayer>();
        if (!compPlayer.ShouldGainHerbEffects())
            return;

        // 仅在移动时尝试种植（与花靴机制类似）
        if (Player.velocity.Y != 0f)                // 未接触地面
            return;
        if (Player.grappling[0] != -1)               // 正在使用钩爪
            return;
        if (Player.velocity.X == 0f)                 // 没有水平移动
            return;
        if (Player.miscCounter % 2 != 0)             // 每两帧尝试一次，控制频率
            return;

        // 获取玩家脚下的图格坐标
        int x = (int)(Player.Center.X / 16);
        int y = (int)((Player.position.Y + Player.height - 1f) / 16);
        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
            return;

        // 检查背包是否有草药种子
        if (!HasAnyHerbSeed())
            return;

        // 获取当前脚下方块和其下方方块
        Tile currentTile = Main.tile[x, y];
        int belowX = x;
        int belowY = y + 1;
        if (belowY < 0 || belowY >= Main.maxTilesY)
            return;

        Tile belowTile = Main.tile[belowX, belowY];
        bool isCurrentEmpty = !currentTile.HasTile;
        bool isBelowPlantPot = belowTile.HasTile && belowTile.TileType == 380; // 种植盆ID

        // 可种植条件：当前位置为空，且（下方为种植盆 或 下方物块在映射表中）
        bool isValidBelow = isBelowPlantPot || (belowTile.HasTile && TileToHerb.ContainsKey(belowTile.TileType));
        if (!isCurrentEmpty || !isValidBelow)
            return;

        // 确定种植概率：手持再生法杖(213)或草镐(5295)时100%，否则1/20
        bool forcePlant = Player.HeldItem.type == 213 || Player.HeldItem.type == 5295;
        float chance = forcePlant ? 1f : 0.05f; // 1/20 = 0.05
        if (Main.rand.NextFloat() >= chance)
            return;

        // 消耗任意一颗草药种子
        if (ConsumeAnyHerbSeed())
        {
            if (isBelowPlantPot)
            {
                // 种植盆：随机生成一种草药（生长期，TileID 82）
                int randomStyle = AllHerbStyles[Main.rand.Next(AllHerbStyles.Length)];
                PlaceHerb(x, y, 82, randomStyle);
            }
            else
            {
                // 特定物块：生成对应的草药（生长期，TileID 82）
                int style = TileToHerb[belowTile.TileType];
                PlaceHerb(x, y, 82, style);
            }
        }
    }

    /// <summary>
    /// 检查背包中是否有任何草药种子。
    /// </summary>
    private bool HasAnyHerbSeed()
    {
        for (int i = 0; i < 58; i++)
        {
            Item item = Player.inventory[i];
            if (item.stack > 0 && IsHerbSeed(item.type))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 消耗背包中第一个可用的草药种子（按物品栏顺序）。
    /// </summary>
    private bool ConsumeAnyHerbSeed()
    {
        for (int i = 0; i < 58; i++)
        {
            Item item = Player.inventory[i];
            if (item.stack > 0 && IsHerbSeed(item.type))
            {
                item.stack--;
                if (item.stack <= 0)
                    item.TurnToAir();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 判断物品是否为草药种子。
    /// </summary>
    private static bool IsHerbSeed(int type)
    {
        return type == ItemID.DaybloomSeeds ||
               type == ItemID.MoonglowSeeds ||
               type == ItemID.BlinkrootSeeds ||
               type == ItemID.DeathweedSeeds ||
               type == ItemID.WaterleafSeeds ||
               type == ItemID.FireblossomSeeds ||
               type == ItemID.ShiverthornSeeds;
    }

    /// <summary>
    /// 在指定位置放置草药图格。
    /// </summary>
    private static void PlaceHerb(int x, int y, int tileId, int style)
    {
        // 仅在服务器或单机模式下执行，客户端不操作图格
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        Tile tile = Main.tile[x, y];
        tile.HasTile = true;
        tile.TileType = (ushort)tileId;
        tile.TileFrameX = (short)(style * 18);
        tile.TileFrameY = 0;

        // 服务器模式下同步给所有客户端
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendTileSquare(-1, x, y, 1);
    }
}