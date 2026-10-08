using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.LittlePrince;

/// <summary>
/// 小王子复活时的临时玻璃罩：负责在玩家周围立起 <see cref="WallLayout"/> 描述的形状，并在 5 秒后自动拆除。
/// 图格的生成与拆除都只在服务器/单人权威端执行，通过原版图格广播（SendTileSquare）同步给所有客户端；
/// 客户端不参与计时与拆除，避免各端计时漂移导致同一格被拆多次或拆不干净。
/// 砌罩本身延迟 <see cref="WallSpawnDelayFrames"/> 帧执行，让击退先作用出去，
/// 避免敌怪在罩子落下的瞬间正好卡在玻璃格位置、导致某格砌不上留下缺口。
/// </summary>
public class LittlePrinceWallSystem : ModSystem
{
    /// <summary>罩子存在时长（帧）：300 帧 = 5 秒。</summary>
    private const int WallLifetimeFrames = 300;

    /// <summary>砌罩前的延迟帧数：先让击退把敌怪推出去，再落罩。</summary>
    public const int WallSpawnDelayFrames = 24;

    /// <summary>
    /// 罩子形状：'#' 表示砌玻璃，'.' 表示留空。
    /// 行数即罩子高度、每行字符数即罩子宽度，改动此表即可改变形状。
    /// </summary>
    private static readonly string[] WallLayout =
    [
         ".#####.",
         "#.....#",
         "#.....#",
         "#.....#",
         "#.....#",
         "#.....#",
         "#.....#",
         "#.....#",
        "#########",
    ];

    /// <summary>单块待拆除玻璃图格的记录。</summary>
    /// <param name="x">图格横坐标。</param>
    /// <param name="y">图格纵坐标。</param>
    private sealed class WallTile(int x, int y)
    {
        /// <summary>图格横坐标。</summary>
        public int X = x;

        /// <summary>图格纵坐标。</summary>
        public int Y = y;

        /// <summary>距离自动拆除的剩余帧数；归零即拆除。</summary>
        public int Life = WallLifetimeFrames;
    }

    /// <summary>等待延迟执行的砌罩请求。</summary>
    private sealed class PendingWall
    {
        /// <summary>触发拦截的玩家索引（权威端本地玩家）。</summary>
        public int PlayerIndex;

        /// <summary>剩余延迟帧数；归零即执行砌罩。</summary>
        public int Delay;
    }

    /// <summary>当前仍存在的罩子图格记录（仅权威端有数据）。</summary>
    private static readonly List<WallTile> _pendingTiles = [];

    /// <summary>等待延迟执行的砌罩请求（仅权威端有数据）。</summary>
    private static readonly List<PendingWall> _pendingWalls = [];

    /// <summary>
    /// 卸载世界时清空记录，避免记录带进下一个世界（图格本身随世界卸载自然消失）。
    /// </summary>
    public override void OnWorldUnload()
    {
        _pendingTiles.Clear();
        _pendingWalls.Clear();
        base.OnWorldUnload();
    }

    /// <summary>
    /// 请求在 <see cref="WallSpawnDelayFrames"/> 帧后于玩家位置生成玻璃罩。
    /// 延迟期间击退已经把敌怪推远，罩子落下时不会和敌怪重叠，因而不会出现漏格。
    /// </summary>
    /// <param name="player">触发拦截的玩家（权威端本地玩家）。</param>
    public static void ScheduleWall(Player player)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return; // 图格由权威端生成，客户端等广播

        _pendingWalls.Add(new PendingWall
        {
            PlayerIndex = player.whoAmI,
            Delay = WallSpawnDelayFrames,
        });
    }

    /// <summary>
    /// 以给定中心为基准，按 <see cref="WallLayout"/> 逐格砌玻璃。
    /// 遇到实心方块保留不动；遇到非实心图格（草、花、藤蔓、火把等）先按原版掉落逻辑清掉再砌玻璃，
    /// 从而避免留下因装饰占位而砌不上的缺口，同时保证被清掉的方块正常掉落物品。
    /// </summary>
    /// <param name="center">罩子中心的世界坐标。</param>
    private static void PlaceWallAt(Vector2 center)
    {
        int width = WallLayout[0].Length;
        int height = WallLayout.Length;

        int centerX = (int)(center.X / 16f);
        int centerY = (int)(center.Y / 16f);
        int left = centerX - width / 2;
        int top = centerY - height / 2;

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                if (WallLayout[row][col] != '#')
                    continue;

                int x = left + col;
                int y = top + row;

                if (!WorldGen.InWorld(x, y, 1))
                    continue;

                Tile tile = Framing.GetTileSafely(x, y);

                if (tile.HasTile)
                {
                    // 实心方块（地面、建筑）保留，不破坏
                    if (Main.tileSolid[tile.TileType])
                        continue;

                    // 非实心图格（草、花、藤蔓、火把等）：清掉腾位，并让它们正常掉落物品
                    WorldGen.KillTile(x, y, fail: false, effectOnly: false, noItem: false);
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, x, y);

                    // 若清理后该格仍有方块（某些不可破坏图格），跳过，不强行覆盖
                    if (Framing.GetTileSafely(x, y).HasTile)
                        continue;
                }

                if (!WorldGen.PlaceTile(x, y, TileID.Glass, mute: true))
                    continue;

                _pendingTiles.Add(new WallTile(x, y));
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, x, y);
            }
        }
    }

    /// <summary>
    /// 每帧推进延迟砌罩与罩子寿命：延迟到期的砌罩请求立即执行，到期的玻璃格自动拆除并广播。
    /// </summary>
    public override void PostUpdateEverything()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return; // 生成与拆除都只由权威端负责

        // 先处理延迟砌罩：到达延迟帧数后再真正落罩
        for (int i = _pendingWalls.Count - 1; i >= 0; i--)
        {
            PendingWall pending = _pendingWalls[i];
            if (--pending.Delay > 0)
                continue;

            _pendingWalls.RemoveAt(i);

            Player player = Main.player[pending.PlayerIndex];
            if (!player.active || player.dead)
                continue; // 玩家在延迟期间掉线/再次死亡：不再砌罩

            PlaceWallAt(player.Center);
        }

        // 再处理罩子拆除
        if (_pendingTiles.Count == 0)
            return;

        for (int i = _pendingTiles.Count - 1; i >= 0; i--)
        {
            WallTile record = _pendingTiles[i];
            if (--record.Life > 0)
                continue;

            _pendingTiles.RemoveAt(i);

            // 玩家可能已经把这块玻璃挖掉、甚至换成了别的方块：
            // 只有它仍是玻璃时才拆除，避免误删玩家后来放置的建筑
            Tile tile = Framing.GetTileSafely(record.X, record.Y);
            if (!tile.HasTile || tile.TileType != TileID.Glass)
                continue;

            // 罩子自身的拆除不掉落玻璃（noItem: true），避免玩家靠反复触发刷玻璃
            WorldGen.KillTile(record.X, record.Y, fail: false, effectOnly: false, noItem: true);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, record.X, record.Y);
        }
    }
}