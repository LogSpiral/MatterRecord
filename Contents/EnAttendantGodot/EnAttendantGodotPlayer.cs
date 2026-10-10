using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodotPlayer : ModPlayer
{
    public bool EnAttendantGodotEquipped { get; set; }
    public IReadOnlySet<int> BannedNPCType => _bannedNPCType;
    private readonly HashSet<int> _bannedNPCType = [];

    /// <summary>上一帧是否装备着本饰品：用来抓「刚戴上」这一下，戴上就立刻重建一次。</summary>
    private bool _wasEquipped;

    /// <summary>上一次刷新时鼠标停着的旗帜物品 type；-1 表示当时没停在旗帜上。</summary>
    private int _lastHoveredBannerItem = -1;

    /// <summary>距离上次重建过了多少帧，用于 10 秒一次的兜底重建。</summary>
    private int _fallbackTimer;

    /// <summary>兜底重建间隔：600 帧 = 10 秒。</summary>
    private const int FALLBACK_REBUILD_TIME = 600;

    public override void ResetEffects()
    {
        EnAttendantGodotEquipped = false;
    }
    public void RebuildBannedList()
    {
        var dict = EnAttendantGodotSystem.Instance.BannerItemType2NPCType;
        HashSet<int> old = Main.netMode == NetmodeID.MultiplayerClient ? [.. _bannedNPCType] : null;
        _fallbackTimer = 0; // 任何一次重建都把兜底窗口重新计时
        _bannedNPCType.Clear();

        foreach (var item in Player.inventory)
        {
            if (!item.favorited) continue;
            if (!dict.TryGetValue(item.type, out int npc)) continue;
            _bannedNPCType.Add(npc);
        }
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            List<int> delta = [];
            foreach (var i in old)
                if (!_bannedNPCType.Contains(i))
                    delta.Add(-i);
            foreach (var i in _bannedNPCType)
                if (!old.Contains(i))
                    delta.Add(i);
            if (delta.Count > 0)
                EnAttendantGodotPacket.Get(Player.whoAmI, [.. delta]).Send();
        }
    }
    public void IncrementalUpdateBannedList(int[] array)
    {
        foreach (var i in array)
        {
            if (i > 0)
                _bannedNPCType.Add(i);
            else if (i < 0)
                _bannedNPCType.Remove(-i); // 负数代表「取消禁用」，存的是正的 NPC type，要取回来再删
        }
    }

    /// <summary>
    /// 列出背包里收藏的敌怪旗帜（按背包格顺序，同一种旗帜只出一次）。
    /// <para>
    /// 只读展示用：即时扫背包，不碰 <see cref="_bannedNPCType"/>、不发同步包，
    /// 所以没装备饰品时玩家自己也能看到自己收藏了哪些旗帜。
    /// </para>
    /// </summary>
    public List<int> GetFavoritedBannerItems()
    {
        var dict = EnAttendantGodotSystem.Instance.BannerItemType2NPCType;
        List<int> result = [];
        HashSet<int> seen = [];
        foreach (var item in Player.inventory)
        {
            if (item.type <= 0 || !item.favorited) continue;
            if (!dict.ContainsKey(item.type)) continue;
            if (seen.Add(item.type))
                result.Add(item.type);
        }
        return result;
    }

    /// <summary>
    /// 禁用表的重建时机。原来是每 600 帧（10 秒）扫一遍，收藏完旗帜要等一会儿才生效，
    /// 现在改成事件驱动，只在玩家真的动了收藏的时候重建：
    /// <list type="number">
    /// <item><b>刚戴上饰品</b>：进场/换装瞬间按当前背包重建（多人下这一下也把全量增量发给服务器）。</item>
    /// <item><b>鼠标移到另一面敌怪旗帜上</b>：停在同一个物品上不会重复扫。</item>
    /// <item><b>按住 Alt</b>（收藏键，Alt + 左键收藏/取消收藏）：按住期间持续重扫。</item>
    /// <item><b>10 秒兜底</b>：上面三条都没覆盖到的改动（别的 mod 动收藏、不经过鼠标的 UI 等）最多晚 10 秒补上。</item>
    /// </list>
    /// 第三条必须是「按住期间每帧」而不是「刚按下时一次」：背包格子的点击是绘制阶段处理的，
    /// 按下 Alt 那一帧收藏还没生效，只抓按下会永远差一拍。
    /// </summary>
    public override void PostUpdateEquips()
    {
        if (Player.whoAmI != Main.myPlayer) return;

        if (!EnAttendantGodotEquipped)
        {
            // 没装备就不维护禁用表；边沿状态一起清掉，重新戴上时能立刻重建
            _wasEquipped = false;
            _lastHoveredBannerItem = -1;
            _fallbackTimer = 0;
            return;
        }

        bool rebuild = false;

        if (!_wasEquipped)
        {
            _wasEquipped = true;
            rebuild = true;
        }

        int hovered = GetHoveredBannerItem();
        if (hovered != _lastHoveredBannerItem)
        {
            _lastHoveredBannerItem = hovered;
            if (hovered > 0)
                rebuild = true;
        }

        if (Main.keyState.IsKeyDown(Keys.LeftAlt) || Main.keyState.IsKeyDown(Keys.RightAlt))
            rebuild = true;

        // 兜底：计数器在 RebuildBannedList 里清零，所以这是「距上次重建满 10 秒」
        if (++_fallbackTimer >= FALLBACK_REBUILD_TIME)
            rebuild = true;

        if (rebuild)
            RebuildBannedList();
    }

    /// <summary>鼠标底下如果是敌怪旗帜，返回它的物品 type；否则返回 -1。</summary>
    private static int GetHoveredBannerItem()
    {
        int type = Main.HoverItem.type;
        if (type <= 0) return -1;
        return EnAttendantGodotSystem.Instance.BannerItemType2NPCType.ContainsKey(type) ? type : -1;
    }
}