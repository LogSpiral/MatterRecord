using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Arguments;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;
using System.Collections.Generic;
namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Base;

public abstract class MultiGroup<T> : IGroup where T : class, IGroupArgument
{
    public List<WrapperArgPair<T>> DataList { get; private set; } = [];

    public abstract Wrapper GetWrapper();
}