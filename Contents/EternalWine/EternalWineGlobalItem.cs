using System.Linq;
using Terraria.GameContent.ItemDropRules;

namespace MatterRecord.Contents.EternalWine;

public class EternalWineGlobalItem : GlobalItem
{
    public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
    {
        if (item.type != ItemID.ObsidianLockbox)
            return;

        OneFromRulesRule dropRule = null;
        foreach (var rule in itemLoot.Get())
        {
            if (rule is OneFromRulesRule oneFromRulesRule)
            {
                dropRule = oneFromRulesRule;
                break;
            }
        }
        if (dropRule != null)
        {
            itemLoot.Remove(dropRule);
            var list = dropRule.options.ToList();
            list.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<EternalWine>()));
            itemLoot.Add(new OneFromRulesRule(1, [.. list]));
        }
    }
}
