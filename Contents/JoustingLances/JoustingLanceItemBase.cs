namespace MatterRecord.Contents.JoustingLances;

public abstract class JoustingLanceItemBase<T>(int damage, int value, int rarity, int barType) : ModItem where T : ModProjectile
{
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.JoustingLance);
        Item.damage = damage;
        Item.value = value;
        Item.rare = rarity;
        Item.shoot = ModContent.ProjectileType<T>();
        base.SetDefaults();
    }
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(barType, 15)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
