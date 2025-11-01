using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.Recorder;

public interface IRecordBookItem
{
    ItemRecords RecordType { get; }
}
