using MatterRecord.Contents.Recorder;
using System.Linq;
using static Terraria.Recipe;

namespace MatterRecord.Contents.EmeraldTablet;

public class EmeraldTablet : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.EmeraldTablet;

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