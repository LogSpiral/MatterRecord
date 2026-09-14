namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodot : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ItemRarityID.Master;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<EnAttendantGodotPlayer>().EnAttendantGodotEquipped = true;
    }
    public override bool AltFunctionUse(Player player) => true;
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer || player.itemAnimation != player.itemAnimationMax) return null;
        if (!player.TryGetModPlayer<EnAttendantGodotPlayer>(out var mplr)) return null;
        if (player.altFunctionUse == 2)
        {
            mplr.PrintBannedList();
        }
        else
        {
            mplr.RebuildBannedList();
            Main.NewText("已经刷新NPC禁用表");
            mplr.PrintBannedList();
        }
        return null;
    }
    public override string Texture => $"Terraria/Images/Item_{ItemID.TopHat}";
}
