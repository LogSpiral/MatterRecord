using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.EnAttendantGodot;

[AutoSync]
public class EnAttendantGodotPacket : NetModule
{
    private byte _whoAmI;
    private int[] _deltaNPCType;
    public static EnAttendantGodotPacket Get(int whoAmI, int[] delta)
    {
        var result = NetModuleLoader.Get<EnAttendantGodotPacket>();
        result._deltaNPCType = delta;
        result._whoAmI = (byte)whoAmI;
        return result;
    }
    public override void Receive()
    {
        if (!Main.player[_whoAmI].TryGetModPlayer<EnAttendantGodotPlayer>(out var mplr)) return;
        mplr.IncrementalUpdateBannedList(_deltaNPCType);
    }
}
