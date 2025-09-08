using MatterRecord.Contents.DonQuijoteDeLaMancha.Core;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Arguments;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Base;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups;

public class ConditionalMultiGroup : MultiGroup<ConditionArg>
{
    public override Wrapper GetWrapper()
    {
        Wrapper result = null;
        foreach (var pair in DataList)
        {
            var (wrapper, argument) = pair.Deconstruct();
            if (argument.Condition.IsMet() && wrapper != null)
            {
                result = wrapper;
                break;
            }
        }
        return result;
    }
}