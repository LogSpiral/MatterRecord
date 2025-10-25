using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.Recorder;

public class RecorderPlayer : ModPlayer
{
    public double UniqueIDForRecorder;
    public override void LoadData(TagCompound tag)
    {
        tag.TryGet("UID",out UniqueIDForRecorder);
    }
    public override void SaveData(TagCompound tag)
    {
        if (UniqueIDForRecorder != 0)
            tag["UID"] = UniqueIDForRecorder;
    }
}
