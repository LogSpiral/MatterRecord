using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;
using System.Collections.Generic;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups.Arguments;

public class NoneArg : IGroupArgument
{
    public bool IsHidden => true;
    public static NoneArg Instance { get; } = new();
    public void LoadAttributes(Dictionary<string, string> attributes)
    {
    }

    public void SetDefault()
    {
    }

    public void WriteAttributes(Dictionary<string, string> attributes)
    {
    }
    public IGroupArgument Clone() => this;

    public override string ToString() => "None";
}