using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
namespace MatterRecord.Contents.TheOldManAndTheSea;

public class TheOldManAndTheSea : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheOldManAndTheSea;
    public override Color? GetAlpha(Color lightColor)
    {
        return null;
    }

    public override void SetStaticDefaults()
    {
        ItemID.Sets.CanFishInLava[Item.type] = true; // Allows the pole to fish in lava
    }

    private static Asset<Texture2D> itemTex;
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.ObsidianSwordfish);
    }
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.WoodFishingPole);
        Item.value = Item.sellPrice(0, 5);
        Item.rare = ItemRarityID.Yellow;
        Item.fishingPole = 75; // Sets the poles fishing power
        Item.shootSpeed = 12f; // Sets the speed in which the bobbers are launched. Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f.
        Item.shoot = ModContent.ProjectileType<TheOldManAndTheSeaBobber>(); // The bobber projectile. Note that this will be overridden by Fishing Bobber accessories if present, so don't assume the bobber spawned is the specified projectile. https://terraria.wiki.gg/wiki/Fishing_Bobbers
    }

    public override void HoldItem(Player player)
    {
        player.accFishingLine = true;
    }

    public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
    {
        lineOriginOffset = new Vector2(42, -44);
        lineColor = Color.Black;
    }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (RecorderSystem.CheckUnlock(ItemRecords.TheOldManAndTheSea))
        {
            itemTex ??= ModContent.Request<Texture2D>("MatterRecord/Contents/TheOldManAndTheSea/TheOldManAndTheSea_full");
            spriteBatch.Draw(itemTex.Value, position, null, drawColor, 0, origin, scale, 0, 0);
        }
        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (RecorderSystem.CheckUnlock(ItemRecords.TheOldManAndTheSea))
        {
            itemTex ??= ModContent.Request<Texture2D>("MatterRecord/Contents/TheOldManAndTheSea/TheOldManAndTheSea_full");
            spriteBatch.Draw(itemTex.Value, Item.position - Main.screenPosition, null, lightColor, rotation, itemTex.Size() * .5f, scale, 0, 0);
        }
        return false;
    }
}