using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;
using Terraria.Localization;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public abstract class BasicDream : ModItem
{
    protected BasicDream(Asset<Texture2D> headTexture, Func<bool> recipeCondition)
    {
        string str = Language.GetTextValue($"Mods.{nameof(MatterRecord)}.Items.{GetType().Name}.RecipeCondition");
        RecipeCondition = recipeCondition == null ? null : new(str, recipeCondition);

        HeadTexture = headTexture;
    }
    Condition RecipeCondition { get; init; }
    Asset<Texture2D> HeadTexture { get; init; }
    public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        var texture = Item.stack == 0 ? TextureAssets.Item[ItemID.RainCloud].Value : TextureAssets.Item[Type].Value;
        for (int n = 0; n < 3; n++)
            spriteBatch.Draw(texture, position + (MathHelper.TwoPi / 3 * n + Main.GlobalTimeWrappedHourly).ToRotationVector2() * 4, frame, drawColor * .33f, 0, origin, scale, 0, 0);

        for (int n = 0; n < 3; n++)
            spriteBatch.Draw(texture, position + (MathHelper.TwoPi / 3 * n - 2 * Main.GlobalTimeWrappedHourly).ToRotationVector2() * 4, frame, drawColor with { A = 0 } * .167f, 0, origin, scale * .75f, 0, 0);

        if (HeadTexture != null)
            spriteBatch.Draw(HeadTexture.Value, position + new Vector2(2), null, drawColor * .5f, 0, default, scale, 0, 0);

        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        spriteBatch.Draw(TextureAssets.Item[Type].Value, Item.Center - Main.screenPosition, null, lightColor, rotation, new Vector2(8), scale, 0, 0);

        if (HeadTexture != null)
            spriteBatch.Draw(HeadTexture.Value, Item.Center - Main.screenPosition + new Vector2(2), null, lightColor * .5f, rotation, new Vector2(-2), scale, 0, 0);

        return false;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Green;
        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        var recipe = CreateRecipe();
        recipe.AddIngredient<BrokenDream>(3);
        if (RecipeCondition != null)
            recipe.AddCondition(RecipeCondition);
        ExtraIngredient(recipe);
        recipe.Register();
    }

    public virtual void ExtraIngredient(Recipe recipe) { }
}
