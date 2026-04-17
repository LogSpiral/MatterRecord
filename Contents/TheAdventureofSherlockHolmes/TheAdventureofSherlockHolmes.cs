using MatterRecord.Contents.Recorder;
namespace MatterRecord.Contents.TheAdventureofSherlockHolmes;

public class TheAdventureofSherlockHolmes : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheAdventureofSherlockHolmes;
    public override void SetDefaults()
    {
        Item.width = Item.height = 48;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.ShinePotion);
    }
}