using System;
using System.Linq;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.EternalWine;

public class EternalWinePass(string str, float value) : GenPass(str, value)
{
    public override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Message = Language.GetTextValue("Mods.MatterRecord.Items.EternalWine.DisplayName");
        foreach (var chest in Main.chest)
        {
            if (chest != null)
            {
                if (GenVars.hellChestItem.Contains(chest.item[0].type))
                {
                    if (WorldGen.genRand.NextBool(10))
                        chest.item[0].SetDefaults(ModContent.ItemType<EternalWine>());
                }
            }
        }
    }
}
