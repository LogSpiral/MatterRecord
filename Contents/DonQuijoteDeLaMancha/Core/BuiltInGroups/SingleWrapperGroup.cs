using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Arguments;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Base;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups;

public class SingleWrapperGroup() : SingleGroup<NoneArg>
{
    public override Wrapper GetWrapper() => Data.Wrapper;

    public SingleWrapperGroup(Wrapper wrapper) : this()
    {
        Data = new WrapperArgPair<NoneArg>
        {
            Wrapper = wrapper,
            Argument = NoneArg.Instance
        };
    }
}