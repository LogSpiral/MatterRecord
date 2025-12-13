using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.Recorder;

public class RecorderPlayer : ModPlayer
{
    public bool MetWithRecorder;
    public override void LoadData(TagCompound tag)
    {
        tag.TryGet("Met",out MetWithRecorder);
    }
    public override void SaveData(TagCompound tag)
    {
        if (MetWithRecorder)
            tag["Met"] = true;
    }
}
