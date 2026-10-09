using System.Collections.Generic;
using Terraria.Localization;

namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodotPlayer : ModPlayer
{
    public bool EnAttendantGodotEquipped { get; set; }
    public IReadOnlySet<int> BannedNPCType => _bannedNPCType;
    private readonly HashSet<int> _bannedNPCType = [];
    private int _rebuildTimer;
    private const int REBUILD_TIME = 600;
    public override void ResetEffects()
    {
        EnAttendantGodotEquipped = false;
    }
    public void RebuildBannedList()
    {
        var dict = EnAttendantGodotSystem.Instance.BannerItemType2NPCType;
        HashSet<int> old = Main.netMode == NetmodeID.MultiplayerClient ? [.. _bannedNPCType] : null;
        _rebuildTimer = 0;
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
                _bannedNPCType.Remove(i);
        }
    }
    public override void PostUpdateEquips()
    {
        if (Player.whoAmI != Main.myPlayer) return;
        if (!EnAttendantGodotEquipped) return;
        _rebuildTimer++;
        if (_rebuildTimer < REBUILD_TIME) return;
        RebuildBannedList();
    }
    public void PrintBannedList()
    {
        if (BannedNPCType.Count > 0)
        {
            Main.NewText("");
            Main.NewText(Language.GetTextValue("Mods.MatterRecord.Items.EnAttendantGodot.BannedList"));
            Main.NewText("--------------------");
            foreach (var i in _bannedNPCType)
                Main.NewText($"{ContentSamples.NpcsByNetId[i].FullName}, ID: {i}");
            Main.NewText("--------------------");
            Main.NewText("");
        }
        else
        {
            Main.NewText(Language.GetTextValue("Mods.MatterRecord.Items.EnAttendantGodot.NoBanned"));
        }
        Main.NewText(Language.GetTextValue("Mods.MatterRecord.Items.EnAttendantGodot.ReEquipPlz"));
    }
}