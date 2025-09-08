using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Interfaces;
using System.Collections.Generic;
using System.Xml;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core;

public partial class Wrapper
{
    public ISequence Sequence { get; init; }

    public ISequenceElement Element { get; init; }

    public string RefSequenceFullName { get; set; }

    public bool Available => Element != null || Sequence != null;

    public Wrapper(ISequence sequence)
    {
        Sequence = sequence;
    }

    public Wrapper(ISequenceElement element)
    {
        Element = element;
    }
}