namespace MatterRecord.Contents.InternalDocument;

using Microsoft.Xna.Framework;

public class InternalDocument : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 38;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.autoReuse = true;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 6;                     // 基础伤害改为 6
        Item.knockBack = 5f;
        Item.crit = 0;

        Item.value =100;
        Item.rare = ItemRarityID.White;
        Item.UseSound = SoundID.Item1;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        player.itemLocation.X -= 1f;
        player.itemLocation.Y += 6f;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        // 获取当前已启用的模组总数
        int enabledModCount = ModLoader.Mods.Length;

        // 每个模组提供 +5% 伤害（乘法）
        float multiplier = 1f + 0.05f * enabledModCount;
        damage *= multiplier;
    }

    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Wood, 10);
        recipe.AddTile(TileID.WorkBenches);
        recipe.Register();
    }
}