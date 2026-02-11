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

    public static List<string> GetNotHintedKeys()
    {
        List<string> hints = [];

        if (CheckNotHinted(ItemRecords.AliceInWonderland))
            hints.Add("AliceInWonderlandHint");
        if (CheckNotHinted(ItemRecords.DonQuijoteDeLaMancha))
            hints.Add("DonQuijoteDeLaManchaHint");
        if (CheckNotHinted(ItemRecords.LittlePrince))
            hints.Add("LittlePrinceHint");
        if (CheckNotHinted(ItemRecords.TheOldManAndTheSea))
            hints.Add("TheOldManAndTheSeaHint");
        if (CheckNotHinted(ItemRecords.WarAndPeace))
            hints.Add("WarAndPeaceHint");
        if (CheckNotHinted(ItemRecords.TheInterpretationOfDreams))
            hints.Add("TheInterpretationOfDreamsHint");
        if (CheckNotHinted(ItemRecords.TheoryOfFreedom))
            hints.Add("TheoryOfFreedomHint");

        return hints;
    }

    public static List<string> GetHintKeys() 
    {
        return
            [
            "AliceInWonderlandHint",
            "DonQuijoteDeLaManchaHint",
            "LittlePrinceHint",
            "TheOldManAndTheSeaHint",
            "WarAndPeaceHint",
            "TheInterpretationOfDreamsHint",
            "TheoryOfFreedomHint"
            ];
    }
    public static List<string> GetLockedKeys()
    {
        List<string> hints = [];

        if (!RecorderSystem.CheckUnlock(ItemRecords.AliceInWonderland))
            hints.Add("AliceInWonderlandHint");
        if (!RecorderSystem.CheckUnlock(ItemRecords.DonQuijoteDeLaMancha))
            hints.Add("DonQuijoteDeLaManchaHint");
        if (!RecorderSystem.CheckUnlock(ItemRecords.LittlePrince))
            hints.Add("LittlePrinceHint");
        if (!RecorderSystem.CheckUnlock(ItemRecords.TheOldManAndTheSea))
            hints.Add("TheOldManAndTheSeaHint");
        if (!RecorderSystem.CheckUnlock(ItemRecords.WarAndPeace))
            hints.Add("WarAndPeaceHint");
        if (!RecorderSystem.CheckUnlock(ItemRecords.TheInterpretationOfDreams))
            hints.Add("TheInterpretationOfDreamsHint");
        if (!RecorderSystem.CheckUnlock(ItemRecords.TheoryOfFreedom))
            hints.Add("TheoryOfFreedomHint");

        return hints;
    }

    public static Dictionary<string, RecordHintState> GetHintedKeysWithState()
    {
        Dictionary<string, RecordHintState> hints = [];

        if (!CheckNotHinted(ItemRecords.AliceInWonderland))
            hints.Add("AliceInWonderlandHint", GetHintState(ItemRecords.AliceInWonderland));

        if (!CheckNotHinted(ItemRecords.DonQuijoteDeLaMancha))
            hints.Add("DonQuijoteDeLaManchaHint", GetHintState(ItemRecords.DonQuijoteDeLaMancha));

        if (!CheckNotHinted(ItemRecords.LittlePrince))
            hints.Add("LittlePrinceHint", GetHintState(ItemRecords.LittlePrince));

        if (!CheckNotHinted(ItemRecords.TheOldManAndTheSea))
            hints.Add("TheOldManAndTheSeaHint", GetHintState(ItemRecords.TheOldManAndTheSea));

        if (!CheckNotHinted(ItemRecords.WarAndPeace))
            hints.Add("WarAndPeaceHint", GetHintState(ItemRecords.WarAndPeace));

        if (!CheckNotHinted(ItemRecords.TheInterpretationOfDreams))
            hints.Add("TheInterpretationOfDreamsHint", GetHintState(ItemRecords.TheInterpretationOfDreams));

        if (!CheckNotHinted(ItemRecords.TheoryOfFreedom))
            hints.Add("TheoryOfFreedomHint", GetHintState(ItemRecords.TheoryOfFreedom));

        return hints;
    }

    public static void SetHintedViaKey(string Key)
    {
        var record = Key switch
        {
            "AliceInWonderlandHint" => ItemRecords.AliceInWonderland,
            "DonQuijoteDeLaManchaHint" => ItemRecords.DonQuijoteDeLaMancha,
            "LittlePrinceHint" => ItemRecords.LittlePrince,
            "TheOldManAndTheSeaHint" => ItemRecords.TheOldManAndTheSea,
            "WarAndPeaceHint" => ItemRecords.WarAndPeace,
            "TheInterpretationOfDreamsHint" => ItemRecords.TheInterpretationOfDreams,
            "TheoryOfFreedomHint" or _ => ItemRecords.TheoryOfFreedom
        };
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
        //if (tag.TryGet("HR", out ulong value))
        //    _hintedRecords = value;
        _hintedRecords = default;
    }
    public override void SaveData(TagCompound tag)
    {
        if (MetWithRecorder)
            tag["Met"] = true;
        //tag["HR"] = (ulong)_hintedRecords;
    }
}
