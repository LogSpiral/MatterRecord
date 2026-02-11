namespace MatterRecord.Contents.EtherChest;
public class EtherChest : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<EtherChest_Tile>());
        Item.width = 26;
        Item.height = 22;
        Item.value = 500;
        Item.rare = ItemRarityID.Green;
    }
}