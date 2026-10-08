using System;
using System.Collections.Generic;
using MatterRecord.Contents.TodaySword;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「万剑归宗」的保底剑池：即使背包与副背包里一把剑都没有，也能唤出这一套剑——毕竟这把武器就是天顶剑做的。
/// <para>根节点是天顶剑本体 + 它的十把合成材料，再沿配方树往下挖「材料里的剑」：
/// 永夜 → 光之灾厄 / 血之屠夫 / 村正 / 草刀 / 火之大剑，真永夜 → 永夜，真断钢 → 断钢……
/// 只沿「剑」继续往下挖，不会顺着矿石、锭之类的东西把整棵合成树都拖进来。</para>
/// </summary>
public class WanJianSwordPool : ModSystem
{
    /// <summary>保底剑池的根：天顶剑本体，加上它的十把合成材料（按配方顺序）。</summary>
    public static readonly int[] PoolRoots =
    {
        ItemID.Zenith,
        ItemID.CopperShortsword,
        ItemID.EnchantedSword,
        ItemID.BeeKeeper,
        ItemID.Starfury,
        ItemID.Seedler,
        ItemID.TheHorsemansBlade,
        ItemID.InfluxWaver,
        ItemID.StarWrath,
        ItemID.Meowmere,
        ItemID.TerraBlade,
    };

    /// <summary>保底剑池：背包里没有的剑会从这里补上。</summary>
    public static readonly List<int> BaseSwords = new();

    /// <summary>
    /// 在配方全部注册完之后建池子（PostSetupContent 时配方还没建好，
    /// tML 的 PostSetupRecipes 才是配方齐全的时机）。
    /// </summary>
    public override void PostSetupRecipes()
    {
        BuildBaseSwords();
    }

    /// <summary>这个物品类型是不是保底剑池的根（天顶剑本体或它的合成材料）。</summary>
    public static bool IsPoolRoot(int type)
    {
        return Array.IndexOf(PoolRoots, type) >= 0;
    }

    /// <summary>
    /// 挖配方树用的判定：只看「近战 + 挥砍式 + 有伤害 + 不是镐斧锤」，不管 <c>noMelee</c>。
    /// <para>真永夜 / 真断钢 / 断钢这类「挥砍但伤害由发射的弹幕结算」的剑 <c>noMelee</c> 是 true
    /// （原版数据：675/674/368 都是 useStyle=1、melee、noMelee=true），
    /// 若沿用今日推剑那条带 <c>noMelee == false</c> 的严格判定，会在泰拉之刃这里把整条线剪断，
    /// 池子里就只剩天顶剑加十把材料共 11 个根节点，永夜那条线的 9 把剑一把都进不来。</para>
    /// <para>回旋镖、悠悠球之类也落在这一档里，但它们不在天顶剑的配方树上，递归挖不到；
    /// 背包扫描那边仍然用严格判定，不会把回旋镖当剑召唤。</para>
    /// </summary>
    public static bool IsSwordLike(int type)
    {
        return type > ItemID.None &&
               ContentSamples.ItemsByType.TryGetValue(type, out Item item) &&
               item.damage > 0 &&
               item.DamageType == DamageClass.Melee &&
               item.useStyle == ItemUseStyleID.Swing &&
               item.pick == 0 &&
               item.axe == 0 &&
               item.hammer == 0;
    }

    /// <summary>把「产物 → 材料」索引建好，再从十把天顶剑材料往下递归出保底剑池。</summary>
    private static void BuildBaseSwords()
    {
        BaseSwords.Clear();

        // 同一把剑可能有多个配方（比如永夜有腐化 / 猩红两条），全部都要收
        Dictionary<int, List<int>> recipeIngredients = new();
        for (int i = 0; i < Recipe.numRecipes; i++)
        {
            Recipe recipe = Main.recipe[i];
            if (recipe?.createItem == null || recipe.createItem.IsAir)
                continue;

            if (!recipeIngredients.TryGetValue(recipe.createItem.type, out List<int> ingredients))
                recipeIngredients[recipe.createItem.type] = ingredients = new List<int>();

            for (int j = 0; j < recipe.requiredItem.Count; j++)
            {
                Item required = recipe.requiredItem[j];
                if (required == null || required.IsAir || ingredients.Contains(required.type))
                    continue;

                ingredients.Add(required.type);
            }
        }

        HashSet<int> visited = new();
        Queue<int> pending = new();
        foreach (int type in PoolRoots)
            pending.Enqueue(type);

        while (pending.Count > 0)
        {
            int type = pending.Dequeue();
            if (type <= ItemID.None || !visited.Add(type))
                continue;

            BaseSwords.Add(type);

            if (!recipeIngredients.TryGetValue(type, out List<int> ingredients))
                continue;

            foreach (int ingredient in ingredients)
            {
                if (IsSwordLike(ingredient))
                    pending.Enqueue(ingredient);
            }
        }
    }
}
