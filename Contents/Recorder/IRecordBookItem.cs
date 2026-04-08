namespace MatterRecord.Contents.Recorder;

public interface IRecordBookItem
{
    ItemRecords RecordType { get; }
}
public static class RecordBookItemExtension
{
    extension(IRecordBookItem recordBookItem) 
    {
        public bool IsRecordUnlocked => RecorderSystem.CheckUnlock(recordBookItem);
    }
}