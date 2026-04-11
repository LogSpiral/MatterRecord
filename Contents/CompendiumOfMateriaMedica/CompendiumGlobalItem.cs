using System.Collections.Generic;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

public class CompendiumGlobalItem : GlobalItem
{
    private static readonly HashSet<int> HerbIds =
    [
        ItemID.Blinkroot,
        ItemID.Daybloom,
        ItemID.Deathweed,
        ItemID.Fireblossom,
        ItemID.Moonglow,
        ItemID.Shiverthorn,
        ItemID.Waterleaf
    ];

    public override bool? UseItem(Item item, Player player)
    {
        var compPlayer = player.GetModPlayer<CompendiumPlayer>();
        // 如果玩家自己装备或同队有人装备，则触发药水效果
        if (compPlayer.ShouldGainHerbEffects())
        {
            if (item.consumable && item.buffType > 0 && item.buffTime > 0)
            {
                List<Recipe> recipes = GetRecipesForItem(item.type);
                bool hasHerb = false;
                foreach (var recipe in recipes)
                {
                    foreach (var requiredItem in recipe.requiredItem)
                    {
                        if (HerbIds.Contains(requiredItem.type))
                        {
                            compPlayer.ActivatePotionEffect(requiredItem.type);
                            hasHerb = true;
                        }
                    }
                }
                if (hasHerb)
                {
                    player.AddBuff(ModContent.BuffType<FragrantHerbs>(), 300 * 60);
                }
            }
        }
        return base.UseItem(item, player);
    }

    private static List<Recipe> GetRecipesForItem(int itemType)
    {
        var recipes = new List<Recipe>();
        for (int i = 0; i < Main.recipe.Length; i++)
        {
            if (Main.recipe[i].createItem.type == itemType)
                recipes.Add(Main.recipe[i]);
        }
        return recipes;
    }
}