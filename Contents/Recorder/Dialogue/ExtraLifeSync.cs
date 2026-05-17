using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.Recorder.Dialogue;

public class ExtraLifeSync : NetModule
{
    [AutoSync]
    private int _extraLifeValue;
    public static ExtraLifeSync Get(int value)
    {
        var packet = NetModuleLoader.Get<ExtraLifeSync>();
        packet._extraLifeValue = value;
        return packet;
    }
    public override void Receive()
    {
        if (Main.netMode is NetmodeID.MultiplayerClient) return;
        if (_extraLifeValue < RecorderSystem.RecorderExtraLifeData) 
        {
            ExtraLifeSyncFromServer.Get(false).Send(Sender);
            return;
        }
        RecorderSystem.RecorderExtraLifeData = _extraLifeValue;
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Recorder recorder)
                recorder.UpdateLifeFromExtraLife(RecorderSystem.RecorderExtraLifeData);
        }
        ExtraLifeSyncFromServer.Get(false).Send();
    }
}

public class ExtraLifeWorldDataIncreaseSync : NetModule
{
    public static ExtraLifeWorldDataIncreaseSync Get() => NetModuleLoader.Get<ExtraLifeWorldDataIncreaseSync>();
    public override void Receive()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        RecorderSystem.RecorderExtraLifeData += 20;
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Recorder recorder)
                recorder.UpdateLifeFromExtraLife(Main.dedServ ? RecorderSystem.RecorderExtraLifeData : Main.LocalPlayer.GetModPlayer<RecorderLocalPlayer>().LocalData.ExtraLife, true);
        }
        if (Main.dedServ)
            ExtraLifeSyncFromServer.Get(true).Send();
    }
}

[AutoSync]
public class ExtraLifeSyncFromServer : NetModule
{
    private int _maxExtraLife;
    private bool _increaseMode;
    public static ExtraLifeSyncFromServer Get(bool increaseMode)
    {
        var packet = NetModuleLoader.Get<ExtraLifeSyncFromServer>();
        packet._maxExtraLife = RecorderSystem.RecorderExtraLifeData;
        packet._increaseMode = increaseMode;
        return packet;
    }
    public override void Receive()
    {
        foreach (var npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is not Recorder recorder) continue;
            recorder.UpdateLifeFromExtraLife(_maxExtraLife, _increaseMode);
        }
    }
}