using MatterRecord.Contents.Recorder;

namespace MatterRecord.Contents.EmeraldTablet;

public class EmeraldTablet : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.EmeraldTablet;

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.AlchemyTable);
    }
}