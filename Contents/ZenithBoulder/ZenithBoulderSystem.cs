using System.Collections.Generic;
using Terraria.WorldBuilding;
namespace MatterRecord.Contents.ZenithBoulder;

public class ZenithBoulderSystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks)
    {
        int ChestIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));
        if (ChestIndex != -1)
        {
            tasks.Insert(ChestIndex, new ZenithBoulderPass("ZenithBoulderSpawn", 100f));
        }
    }
}
