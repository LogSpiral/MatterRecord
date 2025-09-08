using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Arguments;

public class ConditionArg(Condition condition) : IGroupArgument
{
    public Condition Condition { get; } = condition;
}