using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes
{
    public static class SearchCore
    {
        // 配置参数
        private const int MAX_RESULTS = 1500;
        private const float MARKER_SCALE_BASE = 0.9f;
        private const bool ENABLE_VEIN_SCALING = true;
        private const float VEIN_SCALE_MIN = 0.85f;
        private const float VEIN_SCALE_MAX = 1.25f;
        private const int VEIN_SCALE_MIN_TILES = 4;
        private const int VEIN_SCALE_MAX_TILES = 40;
        private const int REFRESH_INTERVAL = 120; // 2秒

        private static Asset<Texture2D> _markerTexture;
        private static string _currentQuery = "";
        private static List<SearchResult> _resultEntries = new List<SearchResult>();
        private static string _pendingQuery = "";
        private static int _pendingSearchDelay = 0;

        // 用户输入、精确匹配标志、刷新计时器
        public static string UserInputText = "";
        private static bool _exactMatch = false;
        private static int _refreshTimer = 0;

        // 公开属性
        public static string CurrentQuery => _currentQuery;
        public static IReadOnlyList<Point> Results => _resultEntries.Select(r => r.Position).ToList().AsReadOnly();
        public static bool HasActiveSearch => _resultEntries.Count > 0 && !string.IsNullOrWhiteSpace(_currentQuery);

        // 八方向邻居偏移
        private static readonly Point[] _neighborOffsets = {
            new Point(-1,-1), new Point(0,-1), new Point(1,-1),
            new Point(-1,0),  new Point(1,0),
            new Point(-1,1),  new Point(0,1), new Point(1,1)
        };

        public static void Load()
        {
            if (!Main.dedServ)
            {
                try
                {
                    _markerTexture = ModContent.Request<Texture2D>("MatterRecord/Contents/TheAdventureofSherlockHolmes/MapMarker");
                }
                catch
                {
                    _markerTexture = null;
                }
            }
        }

        public static void Unload()
        {
            _markerTexture = null;
            ClearSearch();
        }

        public static void QueueSearch(string query)
        {
            UserInputText = query?.Trim() ?? "";
            _currentQuery = UserInputText;
            _pendingQuery = _currentQuery;
            _pendingSearchDelay = 12;
            _exactMatch = false; // 包含匹配
            if (string.IsNullOrWhiteSpace(_currentQuery))
                ClearSearch();

            var player = Main.LocalPlayer?.GetModPlayer<LocatorPlayer>();
            if (player != null)
                player.ClearSavedState();
        }

        public static void ForceSearchByExactName(string exactTileName)
        {
            _exactMatch = true;
            RunSearchNow(exactTileName, exactMatch: true);
            var player = Main.LocalPlayer?.GetModPlayer<LocatorPlayer>();
            if (player != null)
                player.ClearSavedState();
        }

        public static void Update()
        {
            // 清除已破坏的图格标记
            CleanInvalidMarkers();

            // 执行延迟搜索（如果有）
            if (_pendingSearchDelay > 0)
            {
                _pendingSearchDelay--;
                if (_pendingSearchDelay == 0)
                    RunSearchNow(_pendingQuery, _exactMatch);
            }

            // 只要查询非空就持续刷新（即使无标记）
            if (!string.IsNullOrWhiteSpace(_currentQuery) && _pendingSearchDelay == 0)
            {
                _refreshTimer++;
                if (_refreshTimer >= REFRESH_INTERVAL)
                {
                    _refreshTimer = 0;
                    RunSearchNow(_currentQuery, _exactMatch);
                }
            }
            else
            {
                _refreshTimer = 0;
            }
        }

        private static void CleanInvalidMarkers()
        {
            if (_resultEntries.Count == 0) return;
            _resultEntries.RemoveAll(entry =>
            {
                Point p = entry.Position;
                if (!WorldGen.InWorld(p.X, p.Y, 0)) return true;
                Tile tile = Main.tile[p.X, p.Y];
                return tile == null || !tile.HasTile;
            });
        }

        private static void RunSearchNow(string query, bool exactMatch)
        {
            _resultEntries.Clear();
            _currentQuery = query?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(_currentQuery)) return;

            string normalizedQuery = NormalizeSearchText(_currentQuery);
            if (string.IsNullOrWhiteSpace(normalizedQuery)) return;

            // 搜索普通图格（排除箱子）
            FindTileClusters(normalizedQuery, excludeChests: true, exactMatch);
            if (_resultEntries.Count >= MAX_RESULTS) return;

            // 搜索箱子实例
            SearchChests(normalizedQuery, exactMatch);
        }

        private static void FindTileClusters(string query, bool excludeChests, bool exactMatch)
        {
            var visited = new HashSet<int>();
            for (int i = 10; i < Main.maxTilesX - 10; i++)
            {
                for (int j = 10; j < Main.maxTilesY - 10; j++)
                {
                    if (!IsRevealedOnMap(i, j)) continue;
                    string mapName = GetMapDisplayName(i, j);
                    if (string.IsNullOrWhiteSpace(mapName)) continue;

                    bool match = exactMatch ? mapName.Equals(query, StringComparison.OrdinalIgnoreCase)
                                            : mapName.Contains(query, StringComparison.OrdinalIgnoreCase);
                    if (!match) continue;

                    var tile = Main.tile[i, j];
                    if (tile == null || !tile.HasTile) continue;
                    if (excludeChests && IsChestTile(tile)) continue;

                    int packed = PackPoint(i, j);
                    if (visited.Contains(packed)) continue;

                    var clusterTiles = new List<Point>();
                    var queue = new Queue<Point>();
                    queue.Enqueue(new Point(i, j));
                    visited.Add(packed);
                    ushort clusterTileType = tile.TileType;

                    while (queue.Count > 0)
                    {
                        var p = queue.Dequeue();
                        clusterTiles.Add(p);
                        foreach (var off in _neighborOffsets)
                        {
                            int x = p.X + off.X, y = p.Y + off.Y;
                            if (!WorldGen.InWorld(x, y, 0)) continue;
                            int pck = PackPoint(x, y);
                            if (visited.Contains(pck)) continue;
                            var neighborTile = Main.tile[x, y];
                            if (neighborTile == null || !neighborTile.HasTile) continue;
                            if (neighborTile.TileType != clusterTileType) continue;
                            if (!IsRevealedOnMap(x, y)) continue;
                            string neighborName = GetMapDisplayName(x, y);
                            if (string.IsNullOrWhiteSpace(neighborName)) continue;

                            bool neighborMatch = exactMatch ? neighborName.Equals(query, StringComparison.OrdinalIgnoreCase)
                                                            : neighborName.Contains(query, StringComparison.OrdinalIgnoreCase);
                            if (!neighborMatch) continue;

                            if (excludeChests && IsChestTile(neighborTile)) continue;

                            visited.Add(pck);
                            queue.Enqueue(new Point(x, y));
                        }
                    }

                    Point rep = GetRepresentativePoint(clusterTiles);
                    _resultEntries.Add(new SearchResult(rep, clusterTiles.Count));
                    if (_resultEntries.Count >= MAX_RESULTS) return;
                }
            }
        }

        private static void SearchChests(string query, bool exactMatch)
        {
            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null) continue;
                Point point = new Point(chest.x, chest.y);
                if (!WorldGen.InWorld(point.X, point.Y, 10)) continue;
                if (!IsRevealedOnMap(point.X, point.Y)) continue;

                var candidates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                string mapName = GetMapDisplayName(point.X, point.Y);
                if (!string.IsNullOrWhiteSpace(mapName))
                    candidates.Add(mapName);

                if (!string.IsNullOrWhiteSpace(chest.name))
                    candidates.Add(chest.name);

                Tile tile = Main.tile[point.X, point.Y];
                if (tile != null && tile.HasTile)
                {
                    string defaultName = GetChestDefaultContainerName(tile);
                    if (!string.IsNullOrWhiteSpace(defaultName))
                        candidates.Add(defaultName);

                    string placedItemName = GetChestPlacedItemName(tile);
                    if (!string.IsNullOrWhiteSpace(placedItemName))
                        candidates.Add(placedItemName);
                }

                foreach (string name in candidates)
                {
                    bool match = exactMatch ? name.Equals(query, StringComparison.OrdinalIgnoreCase)
                                            : name.Contains(query, StringComparison.OrdinalIgnoreCase);
                    if (match)
                    {
                        _resultEntries.Add(new SearchResult(point, 1));
                        if (_resultEntries.Count >= MAX_RESULTS) return;
                        break;
                    }
                }
            }
        }

        public static HashSet<string> GetAllChestNames()
        {
            HashSet<string> names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null) continue;
                Point point = new Point(chest.x, chest.y);
                if (!WorldGen.InWorld(point.X, point.Y, 10)) continue;
                if (!IsRevealedOnMap(point.X, point.Y)) continue;

                string mapName = GetMapDisplayName(point.X, point.Y);
                if (!string.IsNullOrWhiteSpace(mapName))
                    names.Add(mapName);

                if (!string.IsNullOrWhiteSpace(chest.name))
                    names.Add(chest.name);

                Tile tile = Main.tile[point.X, point.Y];
                if (tile != null && tile.HasTile)
                {
                    string defaultName = GetChestDefaultContainerName(tile);
                    if (!string.IsNullOrWhiteSpace(defaultName))
                        names.Add(defaultName);

                    string placedItemName = GetChestPlacedItemName(tile);
                    if (!string.IsNullOrWhiteSpace(placedItemName))
                        names.Add(placedItemName);
                }
            }
            return names;
        }

        public static List<string> GetMatchingTileNames(string query, int maxResults = 20)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<string>();
            string normalizedQuery = NormalizeSearchText(query);
            if (string.IsNullOrWhiteSpace(normalizedQuery)) return new List<string>();

            HashSet<string> uniqueNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 从普通图格收集
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    if (!IsRevealedOnMap(i, j)) continue;
                    string mapName = GetMapDisplayName(i, j);
                    if (string.IsNullOrWhiteSpace(mapName)) continue;
                    if (mapName.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        uniqueNames.Add(mapName);
                        if (uniqueNames.Count >= maxResults) goto done;
                    }
                }
            }

            // 从箱子收集
            for (int i = 0; i < Main.maxChests; i++)
            {
                Chest chest = Main.chest[i];
                if (chest == null) continue;
                Point point = new Point(chest.x, chest.y);
                if (!WorldGen.InWorld(point.X, point.Y, 10)) continue;
                if (!IsRevealedOnMap(point.X, point.Y)) continue;

                string mapName = GetMapDisplayName(point.X, point.Y);
                if (!string.IsNullOrWhiteSpace(mapName) && mapName.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    uniqueNames.Add(mapName);
                if (!string.IsNullOrWhiteSpace(chest.name) && chest.name.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                    uniqueNames.Add(chest.name);
                Tile tile = Main.tile[point.X, point.Y];
                if (tile != null && tile.HasTile)
                {
                    string defaultName = GetChestDefaultContainerName(tile);
                    if (!string.IsNullOrWhiteSpace(defaultName) && defaultName.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                        uniqueNames.Add(defaultName);
                    string placedItemName = GetChestPlacedItemName(tile);
                    if (!string.IsNullOrWhiteSpace(placedItemName) && placedItemName.IndexOf(normalizedQuery, StringComparison.OrdinalIgnoreCase) >= 0)
                        uniqueNames.Add(placedItemName);
                }
                if (uniqueNames.Count >= maxResults) break;
            }

            done:
            var list = uniqueNames.ToList();
            list.Sort((a, b) => a.Length.CompareTo(b.Length));
            return list;
        }

        private static bool IsChestTile(Tile tile)
        {
            if (tile == null || !tile.HasTile) return false;
            ushort type = tile.TileType;
            if (type == 21 || type == 467) return true;
            return !string.IsNullOrWhiteSpace(GetChestDefaultContainerName(tile));
        }

        private static string GetChestDefaultContainerName(Tile tile)
        {
            var modTile = TileLoader.GetTile(tile.TileType);
            if (modTile == null) return "";
            try
            {
                return modTile.DefaultContainerName(tile.TileFrameX, tile.TileFrameY)?.Value ?? "";
            }
            catch { return ""; }
        }

        private static string GetChestPlacedItemName(Tile tile)
        {
            int style = GetChestStyle(tile);
            if (style < 0) return "";
            try
            {
                int itemType = TileLoader.GetItemDropFromTypeAndStyle(tile.TileType, style);
                return itemType > 0 ? Lang.GetItemNameValue(itemType) : "";
            }
            catch { return ""; }
        }

        private static int GetChestStyle(Tile tile)
        {
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            if (frameX < 0 || frameY < 0) return -1;
            if (Chest.IsLocked(0, 0, tile))
                frameX -= 36;
            return frameX / 36;
        }

        private static string GetMapDisplayName(int x, int y)
        {
            try
            {
                ushort type = Main.Map[x, y].Type;
                return Lang.GetMapObjectName(type);
            }
            catch
            {
                return "";
            }
        }

        private static bool IsRevealedOnMap(int x, int y)
        {
            try { return Main.Map[x, y].Light > 0; }
            catch { return false; }
        }

        private static string NormalizeSearchText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";
            return text.Trim().ToLowerInvariant();
        }

        private static Point GetRepresentativePoint(List<Point> tiles)
        {
            if (tiles.Count == 0) return Point.Zero;
            float cx = (float)tiles.Average(p => p.X);
            float cy = (float)tiles.Average(p => p.Y);
            var center = new Vector2(cx, cy);
            return tiles.OrderBy(p => Vector2.DistanceSquared(p.ToVector2(), center)).First();
        }

        private static int PackPoint(int x, int y) => (x << 16) ^ y;

        public static void ClearSearch()
        {
            _resultEntries.Clear();
            _currentQuery = "";
            _pendingQuery = "";
            _pendingSearchDelay = 0;
            UserInputText = "";
            _exactMatch = false;
            _refreshTimer = 0;
        }

        public static void DrawResults(SpriteBatch sb)
        {
            if (!HasActiveSearch || _markerTexture?.Value == null) return;

            float pulse = 0.97f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.06f;
            foreach (var res in _resultEntries)
            {
                var pos = res.Position;
                if (WorldGen.InWorld(pos.X, pos.Y, 0))
                {
                    Vector2 screen = FullscreenMapTileToScreen(pos.X + 0.5f, pos.Y + 0.5f);
                    DrawCustomMarker(sb, screen, pulse, res.ClusterSize);
                }
            }
        }

        private static void DrawCustomMarker(SpriteBatch sb, Vector2 screen, float pulse, int clusterSize)
        {
            var tex = _markerTexture.Value;
            float scale = MARKER_SCALE_BASE;
            if (Main.mapFullscreenScale < 0.8f) scale = 0.8f;
            else if (Main.mapFullscreenScale > 1.3f) scale = 1f;
            if (ENABLE_VEIN_SCALING)
            {
                float veinScale = GetClusterScaleMultiplier(clusterSize);
                scale *= veinScale;
            }
            scale *= pulse;
            Vector2 origin = new Vector2(tex.Width, tex.Height) * 0.5f;
            sb.Draw(tex, screen + new Vector2(1, 1), null, Color.Black * 0.45f, 0f, origin, scale, SpriteEffects.None, 0f);
            sb.Draw(tex, screen, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        private static float GetClusterScaleMultiplier(int clusterSize)
        {
            if (!ENABLE_VEIN_SCALING) return 1f;
            if (clusterSize <= VEIN_SCALE_MIN_TILES) return VEIN_SCALE_MIN;
            if (clusterSize >= VEIN_SCALE_MAX_TILES) return VEIN_SCALE_MAX;
            float t = (clusterSize - VEIN_SCALE_MIN_TILES) / (float)(VEIN_SCALE_MAX_TILES - VEIN_SCALE_MIN_TILES);
            return MathHelper.Lerp(VEIN_SCALE_MIN, VEIN_SCALE_MAX, t);
        }

        private static Vector2 FullscreenMapTileToScreen(float tileX, float tileY)
        {
            Vector2 center = new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            Vector2 offset = new Vector2(tileX - Main.mapFullscreenPos.X, tileY - Main.mapFullscreenPos.Y);
            return center + offset * Main.mapFullscreenScale / Main.UIScale;
        }

        private struct SearchResult
        {
            public Point Position;
            public int ClusterSize;
            public SearchResult(Point pos, int size) { Position = pos; ClusterSize = size; }
        }
    }
}