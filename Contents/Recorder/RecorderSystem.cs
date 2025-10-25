using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.Recorder;

public class RecorderSystem : ModSystem
{
    public HashSet<double> RecorderTalkedPlayers { get; private set; } = [];
    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.TryGet<List<double>>("UIDList", out var list))
            RecorderTalkedPlayers = [.. list];
        else
            RecorderTalkedPlayers.Clear();
        base.LoadWorldData(tag);
    }
    public override void SaveWorldData(TagCompound tag)
    {
        if (RecorderTalkedPlayers.Count > 0)
            tag.Add("UIDList", RecorderTalkedPlayers.ToList());
        base.SaveWorldData(tag);
    }

    public static RecorderSystem Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }
}
