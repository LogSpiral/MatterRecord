using MatterRecord.Contents.Recorder;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;

namespace MatterRecord.Contents.TheoryOfJustice;

[AutoloadEquip(EquipType.Back, EquipType.Front)]
public class TheoryOfJustice : RecordBookItem
{
    public override ItemRecords RecordType => ItemRecords.TheoryOfJustice;
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.width = 26;
        Item.height = 30;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        base.SetDefaults();
    }
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.MysteriousCape);
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        BestiaryUnlockProgressReport bestiaryProgressReport = Main.GetBestiaryProgressReport();
        float offsetEndurance = (1 - bestiaryProgressReport.CompletionPercent) * .25f;
        player.endurance += offsetEndurance;// * (Main.hardMode ? .5f : 1f);
        base.UpdateAccessory(player, hideVisual);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        BestiaryUnlockProgressReport bestiaryProgressReport = Main.GetBestiaryProgressReport();
        float offsetEndurance = (1 - bestiaryProgressReport.CompletionPercent) * .25f;
        tooltips.Add(new TooltipLine(Mod, "JusticeEndurance", this.GetLocalizedValue("Endurance") + $"{offsetEndurance * 100:0.00}%"));
        base.ModifyTooltips(tooltips);
    }
}