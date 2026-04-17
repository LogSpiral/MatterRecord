using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
namespace MatterRecord.Contents.TheOldManAndTheSea;

public class TheOldManAndTheSea : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheOldManAndTheSea;
    public override void SetStaticDefaults()
    {
        ItemID.Sets.CanFishInLava[Item.type] = true;
    }

    private static Asset<Texture2D> itemTex;
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.ObsidianSwordfish);
    }
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.WoodFishingPole);
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        Item.fishingPole = 75;
        Item.shootSpeed = 12f;
        Item.shoot = ModContent.ProjectileType<TheOldManAndTheSeaBobber>();
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
        Item.fishingPole = this.IsRecordUnlocked ? 75 : 0;
        if (!base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale)) return false;
        if (RecorderSystem.CheckUnlock(ItemRecords.TheOldManAndTheSea))
        {
            itemTex ??= ModContent.Request<Texture2D>("MatterRecord/Contents/TheOldManAndTheSea/TheOldManAndTheSea_full");
            spriteBatch.Draw(itemTex.Value, position, null, drawColor, 0, origin, scale, 0, 0);
        }
        return false;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (!base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI)) return false;
        if (RecorderSystem.CheckUnlock(ItemRecords.TheOldManAndTheSea))
        {
            itemTex ??= ModContent.Request<Texture2D>("MatterRecord/Contents/TheOldManAndTheSea/TheOldManAndTheSea_full");
            spriteBatch.Draw(itemTex.Value, Item.position - Main.screenPosition, null, lightColor, rotation, itemTex.Size() * .5f, scale, 0, 0);
        }
        return false;
    }
}