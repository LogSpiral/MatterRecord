namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithBoulderEx : ModItem
{
    public override bool IsLoadingEnabled(Mod mod) => false;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<ZenithBoulderExTile>());

        Item.width = Item.height = 32;
        Item.value = Item.sellPrice(0, 0, 5);
        Item.rare = ItemRarityID.Purple;
        Item.createTile = ModContent.TileType<ZenithBoulderExTile>();

        base.SetDefaults();
    }

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;

        base.SetStaticDefaults();
    }

    public override void AddRecipes()
    {
        // CreateRecipe().AddIngredient(ItemID.LunarBar).AddIngredient(ItemID.Boulder).AddIngredient(ItemID.BouncyBoulder).AddIngredient(ItemID.RollingCactus).AddTile(TileID.HeavyWorkBench).DisableDecraft().Register();
        base.AddRecipes();
    }
}