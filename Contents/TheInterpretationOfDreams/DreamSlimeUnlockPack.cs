using NetSimplified;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public class DreamSlimeUnlockPack : NetModule
{
    public static DreamSlimeUnlockPack Get() => NetModuleLoader.Get<DreamSlimeUnlockPack>();
    public override void Receive()
    {
        DreamWorld.UsedZoologistDream = true;
        if (Main.dedServ)
            Send(-1, Sender);
    }
}
