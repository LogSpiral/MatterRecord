using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace MatterRecord.Contents.Recorder;

public enum RecordHintState
{
    NotHinted,
    Hinted,
    Found,
    Unlocked
}
public class RecorderPlayer : ModPlayer
{
    public bool MetWithRecorder;

    private static Bits64 _hintedRecords;

    public static bool CheckNotHinted(ItemRecords records) => GetHintState(records) is RecordHintState.NotHinted;
    private static HashSet<ItemRecords> RecordsToHintInternal { get; } =
    [
        ItemRecords.AliceInWonderland,
        // ItemRecords.DonQuijoteDeLaMancha,
        ItemRecords.TheAdventureofSherlockHolmes,
        ItemRecords.LittlePrince,
        ItemRecords.TheOldManAndTheSea,
        ItemRecords.WarAndPeace,
        ItemRecords.TheInterpretationOfDreams,
        ItemRecords.TheoryOfFreedom,
        ItemRecords.TheTaleOfTheHeike,
        ItemRecords.CompendiumOfMateriaMedica
    ];
    public static IReadOnlySet<ItemRecords> RecordsToHint { get; } = RecordsToHintInternal;
    public static List<(string, ItemRecords)> GetNotHintedRecords()
    {
        List<(string, ItemRecords)> hints = [];
        foreach (var record in RecordsToHintInternal)
        {
            if (CheckNotHinted(record))
                hints.Add(($"{record}Hint", record));
        }
        return hints;
    }
    public static List<string> GetLockedKeys()
    {
        List<string> hints = [];
        foreach (var record in RecordsToHintInternal)
        {
            if (!RecorderSystem.CheckUnlock(record))
                hints.Add($"{record}Hint");
        }
        return hints;
    }

    public static Dictionary<string, RecordHintState> GetHintedKeysWithState()
    {
        Dictionary<string, RecordHintState> hints = [];

        foreach (var record in RecordsToHintInternal)
        {
            if (!CheckNotHinted(record))
                hints.Add($"{record}Hint", GetHintState(record));
        }

        return hints;
    }

    public static void SetHinted(ItemRecords record)
    {
        var state = GetHintState(record);
        if (state is RecordHintState.NotHinted)
            SetHintState(record, RecordHintState.Hinted);
    }

    public static RecordHintState GetHintState(ItemRecords records)
    {
        int index = (int)records;
        index *= 2;

        int result = (_hintedRecords[index] ? 1 : 0) + 2 * (_hintedRecords[index + 1] ? 1 : 0);

        return (RecordHintState)result;
    }

    public static void SetHintState(ItemRecords records, RecordHintState state)
    {
        int index = (int)records;
        index *= 2;

        int stateValue = (int)state;

        _hintedRecords[index] = stateValue % 2 == 1;

        stateValue /= 2;

        _hintedRecords[index + 1] = stateValue % 2 == 1;
    }



    public override void LoadData(TagCompound tag)
    {
        tag.TryGet("Met", out MetWithRecorder);
        if (tag.TryGet("HR", out ulong value))
            _hintedRecords = value;
        _hintedRecords = default;
    }
    public override void SaveData(TagCompound tag)
    {
        if (MetWithRecorder)
            tag["Met"] = true;
        tag["HR"] = (ulong)_hintedRecords;
    }
    public override void OnEnterWorld()
    {
        RecorderSystem.RequestSyncingRecordData.Get().Send();
    }
}
