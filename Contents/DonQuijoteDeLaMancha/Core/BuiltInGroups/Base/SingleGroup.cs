using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Arguments;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Base;

public abstract class SingleGroup<T> : IGroup where T : class, IGroupArgument, new()
{
    public Type ArgType => typeof(T);

    public WrapperArgPair<T> Data { get; set; }

    public IReadOnlyList<IWrapperArgPair<IGroupArgument>> Contents => [Data];

    public bool ReadSingleWrapper => true;

    private static readonly Dictionary<string, string> _attributeDict = [];

    public abstract Wrapper GetWrapper();

    private Mod Mod { get; set; }

}