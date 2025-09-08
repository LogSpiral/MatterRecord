namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;

public interface ISequenceElement
{
    void Initialize();

    void Update();

    bool IsCompleted { get; }
}