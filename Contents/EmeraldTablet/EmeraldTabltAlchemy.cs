using MatterRecord.Contents.Recorder;
using System.Linq;

namespace MatterRecord.Contents.EmeraldTablet;

public class EmeraldTabltAlchemy : ModSystem
{
    private static Recipe.IngredientQuantityCallback EmeraldTabltAlchemyMethod { get; } = (recipe, type, ref amount, isDecrafting) =>
    {
        if (!RecorderSystem.CheckUnlock(ItemRecords.EmeraldTablet)) return;
        bool flag = false;
        foreach (var item in Main.LocalPlayer.inventory.Union(Main.LocalPlayer.bank.item).Union(Main.LocalPlayer.bank.item).Union(Main.LocalPlayer.bank2.item).Union(Main.LocalPlayer.bank3.item).Union(Main.LocalPlayer.bank4.item))
        {
            if (item.type == ModContent.ItemType<EmeraldTablet>())
            {
                flag = true;
                break;
            }
        }
        if (!flag) return;
        int amountUsed = 0;

        for (int i = 0; i < amount; i++)
        {
            if (Main.rand.NextBool(2))
                amountUsed++;
        }
        amount = amountUsed;
    };

    public override void PostAddRecipes()
    {
        foreach (var recipe in Main.recipe)
        {
            if (recipe.requiredTile.Contains(TileID.Bottles))
                recipe.AddConsumeIngredientCallback(EmeraldTabltAlchemyMethod);
        }
        base.PostAddRecipes();
    }
}
