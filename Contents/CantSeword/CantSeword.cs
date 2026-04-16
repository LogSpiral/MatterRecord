using Microsoft.Xna.Framework;
namespace MatterRecord.Contents.CantSeword;

public class CantSeword : ModItem
{
    public override void SetDefaults()
    {
        Item.DamageType = DamageClass.Melee;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 18;
        Item.width = Item.height = 80;
        Item.damage = 15;
        Item.rare = ItemRarityID.Blue;
        Item.value = Item.sellPrice(0, 0, 1, 0);
        Item.shoot = ModContent.ProjectileType<CantSewordProj>();
        Item.shootSpeed = 16;
        Item.noMelee = true;
        Item.channel = true;
        base.SetDefaults();
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;

    public override void Load()
    {
        On_PopupText.NewText_PopupTextContext_Item_Vector2_int_bool_bool += CantSewordModify;
        base.Load();
    }

    private int CantSewordModify(On_PopupText.orig_NewText_PopupTextContext_Item_Vector2_int_bool_bool orig, PopupTextContext context, Item newItem, Vector2 position, int stack, bool noStack, bool longText)
    {
        if (newItem?.ModItem is CantSeword)
            return -1;
        return orig.Invoke(context, newItem, position, stack, noStack, longText);
    }
}