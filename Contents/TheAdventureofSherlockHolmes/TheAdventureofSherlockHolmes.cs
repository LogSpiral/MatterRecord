using MatterRecord.Contents.Recorder;
namespace MatterRecord.Contents.TheAdventureofSherlockHolmes;
public class TheAdventureofSherlockHolmes : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheAdventureofSherlockHolmes;
    public override void SetDefaults()
    {
        Item.width = Item.height = 48;
        Item.value = Item.sellPrice(0, 1, 0, 0);
        Item.rare = ItemRarityID.Green;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.ShinePotion);
    }
}