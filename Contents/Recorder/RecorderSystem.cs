using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.Recorder;

public class RecorderSystem : ModSystem
{
    private Bits64 _itemLockRecords;
    public static void ClearRecord() => Instance?._itemLockRecords = default;
    public static bool CheckUnlock(ItemRecords record) => Instance._itemLockRecords[(int)record];
    public static bool CheckUnlock(IRecordBookItem recordBookItem) => CheckUnlock(recordBookItem.RecordType);
    public static void SetUnlock(ItemRecords record) => Instance?._itemLockRecords[(int)record] = true;
    public static void SetUnlock(IRecordBookItem recordBookItem) => SetUnlock(recordBookItem.RecordType);
    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.TryGet<ulong>("LR", out var records))
            _itemLockRecords = records;

        _itemLockRecords[(int)ItemRecords.Faust] = true;
        base.LoadWorldData(tag);
    }
    public override void SaveWorldData(TagCompound tag)
    {
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
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int GuideIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Guide"));

        if (GuideIndex != -1)
        {
            tasks.Insert(GuideIndex + 1, new RecorderSpawnPass());
        }
    }
}
public class RecorderSpawnPass : GenPass
{
    public RecorderSpawnPass() : base("Recorder", 1)
    {
    }

    public override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Set(1.0);

        progress.Message = Language.GetTextValue("Mods.MatterRecord.NPCs.Recorder.Spawn");

        int num297 = NPC.NewNPC(new EntitySource_WorldGen(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<Recorder>());
        Main.npc[num297].homeTileX = Main.spawnTileX;
        Main.npc[num297].homeTileY = Main.spawnTileY;
        Main.npc[num297].direction = 1;
        Main.npc[num297].homeless = true;
    }
}