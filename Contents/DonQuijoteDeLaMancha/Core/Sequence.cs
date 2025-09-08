using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;
using System.Collections.Generic;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core;

public partial class Sequence(params List<IGroup> groups) : ISequence
{
    public Sequence() : this([])
    {
    }

    public List<IGroup> Groups { get; set; } = groups;

    int ISequence.Count => Groups.Count;

    Wrapper ISequence.GetWrapperAt(int index) => Groups[index].GetWrapper();
}