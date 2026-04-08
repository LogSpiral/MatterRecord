namespace MatterRecord.Contents.Recorder;

public abstract partial class RecordBookItem : ModItem, IRecordBookItem
{
    public abstract ItemRecords RecordType { get; }
    public override void SetStaticDefaults()
    {
        RecorderSystem.Instance.RecordToItemType[RecordType] = Type;
    }
}
