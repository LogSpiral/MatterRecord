using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace MatterRecord.Contents.Recorder;

public class RecorderSystem : ModSystem
{
    private Bits64 _itemLockRecords;
    public HashSet<double> RecorderTalkedPlayers { get; private set; } = [];

    public static bool CheckUnlock(ItemRecords record) => Instance._itemLockRecords[(int)record];
    public static bool CheckUnlock(IRecordBookItem recordBookItem) => CheckUnlock(recordBookItem.RecordType);
    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.TryGet<List<double>>("UIDList", out var list))
            RecorderTalkedPlayers = [.. list];
        else
            RecorderTalkedPlayers.Clear();

        if (tag.TryGet<ulong>("LR", out var records))
            _itemLockRecords = records;

        _itemLockRecords[(int)ItemRecords.Faust] = true;
        base.LoadWorldData(tag);
    }
    public override void SaveWorldData(TagCompound tag)
    {
        if (RecorderTalkedPlayers.Count > 0)
            tag.Add("UIDList", RecorderTalkedPlayers.ToList());

        tag.Add("LR", (ulong)_itemLockRecords);
        base.SaveWorldData(tag);
    }

    public static RecorderSystem Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }


    public static bool ShouldSpawnRecordItem<T>() where T : ModItem
    {
        int recordType = ModContent.ItemType<T>();
        if (Main.dedServ)
        {
            foreach (var player in Main.player)
            {
                if (!player.active) continue;
                Item[][] inventories =
                [
                    player.inventory,
                    player.armor,
                    player.dye,
                    player.miscEquips,
                    player.miscDyes,
                    [player.trashItem],
                    player.bank.item,
                    player.bank2.item,
                    player.bank3.item,
                    player.bank4.item
                ];
                foreach (var inventory in inventories)
                    foreach (var item in inventory)
                    {
                        if (item.type == recordType)
                            return false;
                    }
            }
        }
        else
        {
            var player = Main.LocalPlayer;
            Item[][] inventories =
            [
                player.inventory,
                player.armor,
                player.dye,
                player.miscEquips,
                player.miscDyes,
                [player.trashItem],
                player.bank.item,
                player.bank2.item,
                player.bank3.item,
                player.bank4.item
            ];
            foreach (var inventory in inventories)
                foreach (var item in inventory)
                {
                    if (item.type == recordType)
                        return false;
                }
        }
        return true;
    }
}
