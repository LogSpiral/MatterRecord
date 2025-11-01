namespace MatterRecord.Contents.InternalDocument;

public class InternalDocument : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 40;
        Item.useStyle = 1;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.autoReuse = true;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 1;
        Item.knockBack = 4f;
        Item.crit = 0;

        Item.value = Item.sellPrice(copper: 1);
        Item.rare = 0;
        Item.UseSound = SoundID.Item1;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        // 修正：使用正确的方法获取启用的模组数量
        int enabledModCount = ModLoader.Mods.Length;

        // 每个模组提供+1伤害
        damage.Base = enabledModCount - 2;
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Wood, 10);
        recipe.AddTile(TileID.WorkBenches);
        recipe.Register();
    }
}