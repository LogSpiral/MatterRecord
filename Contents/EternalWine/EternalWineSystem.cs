using System.Collections.Generic;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.EternalWine;

public class EternalWineSystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int ChestIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Buried Chests"));

        if (ChestIndex != -1)
        {
            tasks.Insert(ChestIndex + 1, new EternalWinePass("EternalWineSpawn", 100f));
        }
        base.ModifyWorldGenTasks(tasks, ref totalWeight);
    }
}