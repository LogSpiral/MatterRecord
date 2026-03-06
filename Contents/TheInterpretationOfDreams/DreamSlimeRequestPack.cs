using NetSimplified;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public class DreamSlimeRequestPack : NetModule
{
    public static DreamSlimeRequestPack Get() => NetModuleLoader.Get<DreamSlimeRequestPack>();
    public override void Receive()
    {
        if (!Main.dedServ || !DreamWorld.UsedZoologistDream) return;
        DreamSlimeUnlockPack.Get().Send(Sender);
    }
}
