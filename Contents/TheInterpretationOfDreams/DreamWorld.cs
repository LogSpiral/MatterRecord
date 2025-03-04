using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.IO;
using System.Reflection;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public class DreamWorld:ModSystem
{
    public static bool UsedZoologistDream;
    public static List<int> availableDyeId = [];
    public override void PostSetupContent()
    {
        availableDyeId.Clear();
        Dictionary<int,int> dict  = (Dictionary<int, int>)typeof(ArmorShaderDataSet).GetField("_shaderLookupDictionary",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GameShaders.Armor);
        foreach(var key in dict.Keys)
            availableDyeId.Add(key);
        base.PostSetupContent();
    }
    public override void LoadWorldData(TagCompound tag)
    {
        UsedZoologistDream = tag.GetBool(nameof(UsedZoologistDream));
        base.LoadWorldData(tag);
    }
    public override void SaveWorldData(TagCompound tag)
    {
        tag[nameof(UsedZoologistDream)] = UsedZoologistDream;
        base.SaveWorldData(tag);
    }
}
