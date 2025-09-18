using System.Linq;
using static Terraria.Recipe;

namespace MatterRecord.Contents.EmeraldTablet;

public class EmeraldTablet : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.value = Item.sellPrice(0, 1);
        Item.rare = ItemRarityID.Green;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.AlchemyTable);
    }
}

public class EmeraldTabltAlchemy : ModSystem
{
    public static IngredientQuantityCallback EmeraldTabltAlchemyMethod = (Recipe recipe, int type, ref int amount, bool isDecrafting) =>
    {
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