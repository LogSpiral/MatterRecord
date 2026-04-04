namespace MatterRecord.Contents.ZenithBoulder
{
    public class ZenithBoulder : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ZenithBoulderTile>());

            Item.width = Item.height = 32;
            Item.value = Item.sellPrice(0, 0, 5);
            Item.rare = ItemRarityID.Purple;
            Item.createTile = ModContent.TileType<ZenithBoulderTile>();

            base.SetDefaults();
        }

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;

            base.SetStaticDefaults();
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.LunarBar).AddIngredient(ItemID.Boulder).AddIngredient(ItemID.BouncyBoulder).AddIngredient(ItemID.RollingCactus).AddTile(TileID.HeavyWorkBench).DisableDecraft().Register();
            base.AddRecipes();
        }
    }








}