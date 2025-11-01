using System.Collections.Generic;
using Terraria.Localization;

namespace MatterRecord.Contents.HallowBucket;

public class HallowBucketModify : GlobalItem
{
    public override string IsArmorSet(Item head, Item body, Item legs)
    {
        if (head.type is ItemID.EmptyBucket && body.type is ItemID.HallowedPlateMail or ItemID.AncientHallowedPlateMail && legs.type is ItemID.HallowedGreaves or ItemID.AncientHallowedGreaves)
            return "HallowBucket";

        return "";
    }
    public override void UpdateArmorSet(Player player, string set)
    {
        if (set == "HallowBucket")
        {
            player.maxMinions += 2;
            player.setBonus = Language.GetTextValue("ArmorSetBonus.HallowedSummoner");
            player.onHitDodge = true;
        }
    }
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.type == ItemID.EmptyBucket)
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i] is { Mod: "Terraria", Name: "Defense" })
                {
                    tooltips.Insert(i + 1, new TooltipLine(Mod, "HallowHint", Language.GetTextValue("Mods.MatterRecord.Items.HallowBucket.SetHint")));
                    break;
                }
            }
        }
    }
}
