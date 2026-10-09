using MatterRecord.Contents.Rarities;
using MatterRecord.Contents.Recorder;
using Terraria.Localization;

namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodot : ModItem, IRecordBookItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ModContent.RarityType<ImmutableQuest>();
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
            Main.NewText(Language.GetTextValue("Mods.MatterRecord.Items.EnAttendantGodot.BannedListRefreshed"));
            mplr.PrintBannedList();
        }
        return null;
    }
    public override string Texture => $"Terraria/Images/Item_{ItemID.TopHat}";

    public ItemRecords RecordType => ItemRecords.EnAttendantGodot;
}
