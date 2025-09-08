namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;

public interface ISequence
{
    int Count { get; }

    Wrapper GetWrapperAt(int index);
}