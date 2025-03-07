using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.Bestiary;

namespace MatterRecord.Contents.TheoryofJustice
{
    [AutoloadEquip(EquipType.Back, EquipType.Front)]
    public class TheoryofJustice : ModItem
    {
        public override void SetDefaults()
        {
            Item.accessory = true;
            Item.width = 26;
            Item.height = 30;
            Item.value = Item.buyPrice(0, 50);
            Item.rare = ItemRarityID.Orange;
            base.SetDefaults();
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            BestiaryUnlockProgressReport bestiaryProgressReport = Main.GetBestiaryProgressReport();
            float offsetEndurance = (1 - bestiaryProgressReport.CompletionPercent) * .3f;
            player.endurance += offsetEndurance;// * (Main.hardMode ? .5f : 1f);
            base.UpdateAccessory(player, hideVisual);
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            BestiaryUnlockProgressReport bestiaryProgressReport = Main.GetBestiaryProgressReport();
            float offsetEndurance = (1 - bestiaryProgressReport.CompletionPercent) * .3f;
            tooltips.Add(new TooltipLine(Mod, "JusticeEndurance", $"现在提供减伤{offsetEndurance * 100:0.00}%"));
            base.ModifyTooltips(tooltips);
        }
    }
}
