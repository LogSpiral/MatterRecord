using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

/// <summary>
/// 「获取道具类」梦境物品的公共基类：左键使用后直接发放梦中之物。
/// <para>这些梦原先采用「背包内右键使用」，现已统一为手持左键使用，并改为消耗品（用一次少一个）。</para>
/// </summary>
/// <param name="index">NPC 头像贴图索引，用于在云朵贴图上叠加对应 NPC 的头像。</param>
/// <param name="condition">合成条件（对应 NPC 是否可入住等）。</param>
/// <param name="type">固定发放的物品类型（与 <paramref name="itemGetter"/> 二选一）。</param>
/// <param name="stack">固定发放的数量，仅在未提供 <paramref name="itemGetter"/> 时生效。</param>
/// <param name="itemGetter">动态发放的物品集合（每项为「物品类型 + 数量」），优先于 <paramref name="type"/>。</param>
public abstract class BagLikeDreams(int index, Func<bool> condition, int type, int stack = 1, Func<IEnumerable<(int, int)>> itemGetter = null) : ActionLikeDreams(index, condition)
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.maxStack = 9999;
        // 由背包右键改为手持左键后，统一按消耗品处理
        Item.consumable = true;
    }

    /// <summary>
    /// 判断当前是否满足使用条件（如世界进度、是否击败特定 Boss）。
    /// </summary>
    /// <param name="player">使用该物品的玩家。</param>
    /// <returns>满足条件返回 true；返回 false 时左键仅弹提示，既不发放奖励也不消耗物品。</returns>
    public virtual bool CanUse(Player player) => true;

    /// <summary>
    /// 左键使用：发放梦中之物。条件不满足时只提示、不消耗。
    /// </summary>
    /// <param name="player">使用该物品的玩家。</param>
    public override void UseAction(Player player)
    {
        if (!CanUse(player))
        {
            // 复用各子类已有的 Unlock 文案作为拦截提示，避免新增本地化条目
            Main.NewText(this.GetLocalizedValue("Unlock"), Color.Lerp(Color.Gray, Color.DarkGray, Main.mouseTextColor / 255f));
            return;
        }

        var collection = itemGetter?.Invoke();
        if (collection != null)
            foreach (var pair in collection)
                player.QuickSpawnItem(new EntitySource_Gift(Item), pair.Item1, pair.Item2);
        else
            player.QuickSpawnItem(new EntitySource_Gift(Item), type, stack);

        // 消耗写法与 WizardDream / ZoologiseDream 保持一致
        Item.stack--;
        if (Item.stack <= 0)
            Item.TurnToAir();
    }
}

public class GuideDream() : BagLikeDreams(1, () => true, 0, 0, () => [(ItemID.WarriorEmblem, 1), (ItemID.SummonerEmblem, 1), (ItemID.SorcererEmblem, 1), (ItemID.RangerEmblem, 1)])
{
    public override bool CanUse(Player player) => Main.hardMode;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!Main.hardMode)
            tooltips.Add(new TooltipLine(Mod, "Unlock", this.GetLocalizedValue("Unlock")) { Color = Color.Lerp(Color.Gray, Color.DarkGray, Main.mouseTextColor / 255f) });
        base.ModifyTooltips(tooltips);
    }

    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.GuideVoodooDoll);
}

public class TavernkeepDream() : BagLikeDreams(24, () => NPC.savedBartender, 0, 0, () => [(ItemID.DefenderMedal, NPC.downedGolemBoss ? 12 : NPC.downedMechBossAny ? 8 : 4)])
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.DD2ElderCrystal);
}

public class ClothierDream() : BagLikeDreams(7, () => NPC.downedBoss3, ItemID.GoldenKey)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.BlackThread);
}

public class StylistDream() : BagLikeDreams(20, () => NPC.savedStylist, ModContent.ItemType<CosmosScissors>())
{
    public override bool CanUse(Player player) => NPC.downedPlantBoss;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!NPC.downedPlantBoss)
            tooltips.Add(new TooltipLine(Mod, "Unlock", this.GetLocalizedValue("Unlock")) { Color = Color.Lerp(Color.Gray, Color.DarkGray, Main.mouseTextColor / 255f) });
        base.ModifyTooltips(tooltips);
    }

    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.HairDyeRemover);
}

public class DyeTraderDream() : BagLikeDreams(14, NPC.SpawnAllowed_DyeTrader, 0, 0, () => [(Main.rand.Next(DreamWorld.availableDyeId), 3)])
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.SilverDye);
}
