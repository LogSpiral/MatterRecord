using System.Collections.Generic;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

/// <summary>
/// 行走种植功能：玩家自己佩戴本草纲目且背包有草药种子时，
/// 在行走过程中在脚下的可种植位置（下方为特定可种植物块或种植盆，
/// 且该格为空或是可覆盖的杂草/藤蔓）种植对应草药，并消耗种子。
/// 普通种植概率 1/20，手持再生法杖(213)或草镐(5295)时概率100%。
/// <para>注意：</para>
/// <list type="bullet">
/// <item>同队共享的只有草药增益本身（药水触发、附近图格触发），行走种植不共享，只有佩戴者本人会种植。</item>
/// <item>只有本机玩家参与判定：联机时客户端扣自己背包里的种子，再把落地请求（<see cref="WalkingPlantRequest"/>）
/// 发给服务端；图格改动一律由服务端执行，否则会出现「扣了种子却种不出草」。</item>
/// </list>
/// </summary>
public class CompendiumWalkingPlant : ModPlayer
{
    // 下方物块类型 -> 草药样式映射表（仅这些物块可触发种植）
    private static readonly Dictionary<int, int> TileToHerb = [];

    // 种植盆图格样式 -> 草药样式映射表
    // 草药样式：0太阳花 1月光草 2闪耀根 3死亡草 4水叶草 5火焰花 6寒颤棘
    // 种植盆样式：0太阳花 1月光草 2死亡草 3死亡草 4闪耀根 5水叶草 6寒颤棘 7火焰花
    private static readonly Dictionary<int, int> PlantPotStyleToHerb = new()
    {
        { 0, 0 }, // 太阳花种植盆  -> 太阳花
        { 1, 1 }, // 月光草种植盆  -> 月光草
        { 2, 3 }, // 死亡草种植盆  -> 死亡草
        { 3, 3 }, // 死亡草种植盆  -> 死亡草
        { 4, 2 }, // 闪耀根种植盆  -> 闪耀根
        { 5, 4 }, // 水叶草种植盆  -> 水叶草
        { 6, 6 }, // 寒颤棘种植盆  -> 寒颤棘
        { 7, 5 }, // 火焰花种植盆  -> 火焰花
    };

    /// <summary>
    /// 行走种植可以直接覆盖掉的杂草与藤蔓图格。
    /// <para>故意不含 82/83/84（草药本身），否则会把已成株的草药铲掉重种。</para>
    /// </summary>
    private static readonly HashSet<int> OverwritablePlants =
    [
        TileID.Plants,          // 3   花 / 杂草
        TileID.Plants2,         // 73  高杂草
        TileID.CorruptPlants,   // 24  腐化杂草
        TileID.JunglePlants,    // 61  丛林植物
        TileID.JunglePlants2,   // 74  丛林高草
        TileID.MushroomPlants,  // 71  蘑菇草
        TileID.HallowedPlants,  // 110 神圣植物
        TileID.HallowedPlants2, // 113 神圣高草
        TileID.CrimsonPlants,   // 201 猩红杂草
        TileID.AshPlants,       // 637 灰烬植物
        TileID.Vines,           // 52  藤蔓
        TileID.JungleVines,     // 62
        TileID.HallowedVines,   // 115
        TileID.CrimsonVines,    // 205
        TileID.MushroomVines,   // 528
        TileID.CorruptVines,    // 636
        TileID.AshVines,        // 638
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
    /// <para>只有本机玩家参与判定：联机时客户端扣自己背包里的种子（客户端对自己背包有权威），
    /// 再把落地请求发给服务端；图格改动一律由服务端执行。</para>
    /// </summary>
    public override void PostUpdate()
    {
        // 只处理佩戴者本人（同队共享不包括种植，见类注释）
        var compPlayer = Player.GetModPlayer<CompendiumPlayer>();
        if (!compPlayer.hasCompendium)
            return;

        // 每个进程只判定本机玩家，避免服务端与客户端各摇一次：那样客户端会白扣种子
        if (Player.whoAmI != Main.myPlayer)
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

        // 检查背包是否有草药种子
        if (!HasAnyHerbSeed())
            return;

        // 位置能否种植、该种什么草药：客户端判定与服务端复核共用同一套规则
        if (!TryGetPlantTarget(x, y, out int herbStyle, out bool needKillFlower))
            return;

        // 确定种植概率：手持再生法杖(213)或草镐(5295)时100%，否则1/20
        bool forcePlant = Player.HeldItem.type == 213 || Player.HeldItem.type == 5295;
        float chance = forcePlant ? 1f : 0.05f;
        if (Main.rand.NextFloat() >= chance)
            return;

        // 先扣种子，扣不动就不种
        if (!ConsumeAnyHerbSeed())
            return;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            // 客户端改图格不会被承认，落地交给服务端；
            // 草药样式由服务端按下方物块重新推导，不采信客户端传来的值。
            WalkingPlantRequest.Get(x, y).Send();
            return;
        }

        ApplyPlant(x, y, herbStyle, needKillFlower);
    }

    /// <summary>
    /// 判断 (x, y) 能否种植，并推导出要种下的草药样式。
    /// <para>客户端判定与（服务端收到请求后的）复核共用这一套规则，草药样式一律由下方物块决定。</para>
    /// </summary>
    /// <param name="x">目标图格 X。</param>
    /// <param name="y">目标图格 Y（玩家脚下那一格）。</param>
    /// <param name="herbStyle">可种植时输出草药样式（0太阳花 … 6寒颤棘）。</param>
    /// <param name="needKillFlower">可种植时输出是否需要先清掉当前格的杂草/藤蔓。</param>
    /// <returns>可以种植返回 true。</returns>
    internal static bool TryGetPlantTarget(int x, int y, out int herbStyle, out bool needKillFlower)
    {
        herbStyle = 0;
        needKillFlower = false;

        if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
            return false;

        int belowY = y + 1;
        if (belowY < 0 || belowY >= Main.maxTilesY)
            return false;

        // 当前位置与下方位置
        Tile currentTile = Main.tile[x, y];
        Tile belowTile = Main.tile[x, belowY];

        bool isCurrentEmpty = !currentTile.HasTile;
        // 当前格是杂草/藤蔓时可以直接覆盖（见 OverwritablePlants，草药本身不在表里）
        bool isCurrentOverwritable = currentTile.HasTile && OverwritablePlants.Contains(currentTile.TileType);

        bool isBelowPlantPot = belowTile.HasTile && belowTile.TileType == TileID.PlanterBox;

        // 普通图格：必须是完整方块（非半砖、非斜坡），且在映射表中
        bool isBelowFullBlock = !belowTile.IsHalfBlock && belowTile.Slope == 0;
        bool isBelowValidBlock = belowTile.HasTile
                                  && TileToHerb.ContainsKey(belowTile.TileType)
                                  && isBelowFullBlock;

        // 下方必须是种植盆或可种植物块
        if (!isBelowPlantPot && !isBelowValidBlock)
            return false;

        // 当前格必须为空，或是可以覆盖掉的杂草/藤蔓；种植盆与普通图格一视同仁
        if (!isCurrentEmpty && !isCurrentOverwritable)
            return false;

        needKillFlower = !isCurrentEmpty;

        if (isBelowPlantPot)
        {
            // 种植盆：家具图格样式使用 TileFrameY 计算
            int potStyle = belowTile.TileFrameY / 18;
            return PlantPotStyleToHerb.TryGetValue(potStyle, out herbStyle);
        }

        herbStyle = TileToHerb[belowTile.TileType];
        return true;
    }

    /// <summary>
    /// 在世界侧真正落地：必要时先清掉当前格的杂草/藤蔓，再放上生长期的草药图格。
    /// <para>只允许单机 / 服务端执行，客户端调用直接返回（客户端改图格不会被承认）。</para>
    /// </summary>
    /// <param name="x">目标图格 X。</param>
    /// <param name="y">目标图格 Y。</param>
    /// <param name="herbStyle">草药样式。</param>
    /// <param name="needKillFlower">是否需要先清掉当前格的杂草/藤蔓。</param>
    internal static void ApplyPlant(int x, int y, int herbStyle, bool needKillFlower)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        // 若当前位置是杂草/藤蔓，先摧毁它
        if (needKillFlower)
        {
            WorldGen.KillTile(x, y, fail: false, effectOnly: false, noItem: true);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, x, y, 1);
        }

        // 放置对应草药（生长期）
        PlaceHerb(x, y, TileID.ImmatureHerbs, herbStyle);
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