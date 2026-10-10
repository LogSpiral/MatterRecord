using System;
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
/// <para>
/// 击退、弹幕反弹与砌罩共用一个 <see cref="WallSpawnDelayFrames"/> 帧的窗口：
/// 窗口内每帧都把范围内的敌怪重新推一次（单次推力会被敌怪 AI 在下一帧覆盖），
/// 窗口结束时才落罩，因此敌怪不会卡在玻璃格位置。
/// </para>
/// <para>
/// 即便如此仍可能有实体压在格子上，而 <see cref="WorldGen.PlaceTile"/> 内部的
/// <c>Collision.EmptyTile</c> 会检查玩家与 NPC 占位并直接失败；这类格子会进入
/// <see cref="WallRetryWindowFrames"/> 帧的重试队列逐帧补砌，避免罩子留下永久缺口。
/// </para>
/// </summary>
public class LittlePrinceWallSystem : ModSystem
{
    /// <summary>罩子存在时长（帧）：300 帧 = 5 秒。</summary>
    private const int WallLifetimeFrames = 300;

    /// <summary>落罩前的窗口帧数：这段时间里持续击退，把敌怪推出去之后再落罩。</summary>
    public const int WallSpawnDelayFrames = 24;

    /// <summary>砌罩失败的图格的重试窗口（帧）；窗口用尽即放弃该格。</summary>
    private const int WallRetryWindowFrames = 120;

    /// <summary>
    /// 罩子形状：'#' 表示砌玻璃，'.' 表示留空。
    /// 行数即罩子高度；每行按**自身长度**独立居中，因此允许上下行不等长
    /// （例如底边比主体宽出一格，形成外扩的底盘）。
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

    /// <summary>罩子的最大行宽（图格数）；判定范围与整体居中偏移都以它为准。</summary>
    private static readonly int MaxRowWidth = GetMaxRowWidth();

    private static int GetMaxRowWidth()
    {
        int width = 0;
        foreach (string row in WallLayout)
            width = Math.Max(width, row.Length);
        return width;
    }

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

    /// <summary>因实体占位而砌失败、等待下一帧重试的图格。</summary>
    /// <param name="x">图格横坐标。</param>
    /// <param name="y">图格纵坐标。</param>
    private sealed class RetryTile(int x, int y)
    {
        /// <summary>图格横坐标。</summary>
        public int X = x;

        /// <summary>图格纵坐标。</summary>
        public int Y = y;

        /// <summary>剩余重试窗口帧数；归零即放弃该格。</summary>
        public int Life = WallRetryWindowFrames;
    }

    /// <summary>等待逐帧推进的复活效果。</summary>
    private sealed class PendingRevive
    {
        /// <summary>触发拦截的玩家索引（权威端本地玩家）。</summary>
        public int PlayerIndex;

        /// <summary>剩余窗口帧数；归零即落罩。</summary>
        public int Delay;

        /// <summary>是否需要落罩；玩家关闭了饰品可见性时为 false，此时只保留击退与弹幕反弹。</summary>
        public bool BuildWall;
    }

    /// <summary>当前仍存在的罩子图格记录（仅权威端有数据）。</summary>
    private static readonly List<WallTile> _pendingTiles = [];

    /// <summary>等待补砌的图格记录（仅权威端有数据）。</summary>
    private static readonly List<RetryTile> _retryTiles = [];

    /// <summary>等待逐帧推进的复活效果（仅权威端有数据）。</summary>
    private static readonly List<PendingRevive> _pendingRevives = [];

    /// <summary>
    /// 卸载世界时清空记录，避免记录带进下一个世界（图格本身随世界卸载自然消失）。
    /// </summary>
    public override void OnWorldUnload()
    {
        _pendingTiles.Clear();
        _retryTiles.Clear();
        _pendingRevives.Clear();
        base.OnWorldUnload();
    }

    /// <summary>
    /// 罩子（含外扩边距）在世界坐标中的矩形范围，供击退与敌对弹幕反弹的判定使用。
    /// 用矩形相交而不是「中心点距离 ≤ 半径」，是为了让体积大的敌怪只要 Hitbox 沾到罩子范围就会被推开，
    /// 不会因为中心点落在半径之外被整个跳过，进而压住格子导致漏格。
    /// </summary>
    /// <param name="center">罩子中心的世界坐标。</param>
    /// <param name="margin">相对罩子外沿额外外扩的像素数。</param>
    public static Rectangle GetEffectBounds(Vector2 center, float margin)
    {
        int width = MaxRowWidth;
        int height = WallLayout.Length;

        int centerX = (int)(center.X / 16f);
        int centerY = (int)(center.Y / 16f);
        int left = centerX - width / 2;
        int top = centerY - height / 2;

        return new Rectangle(
            (int)(left * 16 - margin),
            (int)(top * 16 - margin),
            (int)(width * 16 + margin * 2),
            (int)(height * 16 + margin * 2));
    }

    /// <summary>
    /// 请求推进一次复活效果：在 <see cref="WallSpawnDelayFrames"/> 帧的窗口内逐帧击退敌怪、反弹敌对弹幕，
    /// 窗口耗尽时于玩家位置落罩（<paramref name="buildWall"/> 为 false 时只做前者）。
    /// </summary>
    /// <param name="player">触发拦截的玩家（权威端本地玩家）。</param>
    /// <param name="buildWall">是否需要落罩。</param>
    public static void ScheduleRevive(Player player, bool buildWall)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return; // 效果与图格都由权威端执行，客户端等广播

        _pendingRevives.Add(new PendingRevive
        {
            PlayerIndex = player.whoAmI,
            Delay = WallSpawnDelayFrames,
            BuildWall = buildWall,
        });
    }

    /// <summary>
    /// 以给定中心为基准，按 <see cref="WallLayout"/> 逐格砌玻璃。
    /// 每行按自身长度独立居中，因此底边可以比主体宽。
    /// 遇到实心方块保留不动；遇到非实心图格（草、花、藤蔓、火把等）先按原版掉落逻辑清掉再砌玻璃；
    /// 被玩家或敌怪压住而砌不上的格子转交重试队列，避免留下永久缺口。
    /// </summary>
    /// <param name="center">罩子中心的世界坐标。</param>
    private static void PlaceWallAt(Vector2 center)
    {
        int height = WallLayout.Length;

        int centerX = (int)(center.X / 16f);
        int centerY = (int)(center.Y / 16f);
        int top = centerY - height / 2;

        for (int row = 0; row < height; row++)
        {
            string line = WallLayout[row];
            int left = centerX - line.Length / 2;

            for (int col = 0; col < line.Length; col++)
            {
                if (line[col] != '#')
                    continue;

                int x = left + col;
                int y = top + row;

                if (!WorldGen.InWorld(x, y, 1))
                    continue;

                Tile tile = Framing.GetTileSafely(x, y);

                if (tile.HasTile)
                {
                    // 实心方块（地面、建筑）保留，不破坏，也不再尝试这一格
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

                // 图格层面已经空了，此处砌不上只可能是被玩家/NPC 占位（PlaceTile 内部走 Collision.EmptyTile）
                if (!WorldGen.PlaceTile(x, y, TileID.Glass, mute: true))
                {
                    _retryTiles.Add(new RetryTile(x, y));
                    continue;
                }

                _pendingTiles.Add(new WallTile(x, y));
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, x, y);
            }
        }
    }

    /// <summary>
    /// 补砌之前被实体占位而失败的格子：窗口内每帧重试一次，
    /// 成功后转为正常罩子图格参与拆除计时（寿命从砌上那一刻才开始算）。
    /// </summary>
    private static void RetryBlockedTiles()
    {
        for (int i = _retryTiles.Count - 1; i >= 0; i--)
        {
            RetryTile retry = _retryTiles[i];

            if (--retry.Life <= 0)
            {
                _retryTiles.RemoveAt(i); // 窗口用尽：放弃该格
                continue;
            }

            if (Framing.GetTileSafely(retry.X, retry.Y).HasTile)
            {
                _retryTiles.RemoveAt(i); // 已被别的途径占上（玩家自己放方块等），不再补
                continue;
            }

            if (!WorldGen.PlaceTile(retry.X, retry.Y, TileID.Glass, mute: true))
                continue; // 还被实体压着，下一帧再试

            _retryTiles.RemoveAt(i);
            _pendingTiles.Add(new WallTile(retry.X, retry.Y));
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, retry.X, retry.Y);
        }
    }

    /// <summary>
    /// 推进所有等待中的复活效果：每帧重新推开范围内敌怪、反弹敌对弹幕，窗口耗尽时落罩。
    /// </summary>
    private static void AdvancePendingRevives()
    {
        for (int i = _pendingRevives.Count - 1; i >= 0; i--)
        {
            PendingRevive pending = _pendingRevives[i];

            Player player = Main.player[pending.PlayerIndex];
            if (!player.active || player.dead)
            {
                _pendingRevives.RemoveAt(i); // 玩家在窗口期间掉线/再次死亡：不再继续
                continue;
            }

            LittlePrinceReviveEffects.KnockbackEnemies(player);
            LittlePrinceReviveEffects.ReflectHostileProjectiles(player);

            if (--pending.Delay > 0)
                continue;

            _pendingRevives.RemoveAt(i);

            if (pending.BuildWall)
                PlaceWallAt(player.Center);
        }
    }

    /// <summary>
    /// 拆除到期的玻璃格：只拆仍属于罩子的玻璃（玩家后来自己放的玻璃不受影响），且不掉落玻璃物品。
    /// </summary>
    private static void ExpireWallTiles()
    {
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

    /// <summary>
    /// 每帧推进：先维持击退并补砌缺口，再拆除到期的罩子玻璃。
    /// </summary>
    public override void PostUpdateEverything()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return; // 效果、生成与拆除都只由权威端负责

        AdvancePendingRevives();
        RetryBlockedTiles();
        ExpireWallTiles();
    }
}
