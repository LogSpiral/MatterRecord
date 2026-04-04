using MatterRecord.Contents.Recorder;

namespace MatterRecord.Contents.LittlePrince;



public class LittlePrince : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.LittlePrince;
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.AbigailsFlower);
        base.AddRecipes();
    }
    //public override string Texture => $"Terraria/Images/Item_{ItemID.JungleRose}";
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 27;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        Item.accessory = true;
        base.SetDefaults();
    }

    public override void UpdateEquip(Player player)
    {
        player.GetModPlayer<LittlePrincePlayer>().EquippedRose = true;
        base.UpdateEquip(player);
    }
}