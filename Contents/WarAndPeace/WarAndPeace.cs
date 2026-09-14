using MatterRecord.Contents.Rarities;
using MatterRecord.Contents.Recorder;
using System;

namespace MatterRecord.Contents.WarAndPeace;

public class WarAndPeace : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.WarAndPeace;
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ModContent.RarityType<ImmutableQuest>();
        Item.width = Item.height = 32;
        base.SetDefaults();
    }
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.LicenseCat);
    }
    public static bool IsPeace => ((int)(8 * Math.Sin(8 * DateTime.Now.Day)) + 8) % 2 == 1;

    /// <summary>
    /// 今天是否属于「和平」一侧，决定猫猫弹幕的形态（和平猫 / 战争猫）。
    /// <para>周日走假日分支，由 <see cref="IsPeace"/> 决定；其余日子偶数日和平、奇数日战争。</para>
    /// <para>把日期规则集中在这里，是为了让「施加 buff」与「维持弹幕」共用同一份判定，避免两处规则漂移。</para>
    /// </summary>
    public static bool IsPeaceDay
    {
        get
        {
            var dayOfWeek = DateTime.Now.DayOfWeek;
            if (dayOfWeek == DayOfWeek.Sunday)
                return IsPeace;
            return (int)dayOfWeek % 2 == 0;
        }
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!this.IsRecordUnlocked) return;
        var dayOfWeek = DateTime.Now.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Sunday)
            player.AddBuff(IsPeace ? ModContent.BuffType<Holiday_Peace>() : ModContent.BuffType<Holiday_War>(), 2);
        else if (IsPeaceDay)
        {
            player.endurance += .1f;
            player.AddBuff(ModContent.BuffType<Peace>(), 2);
        }
        else
        {
            player.AddBuff(ModContent.BuffType<War>(), 2);
            player.GetDamage(DamageClass.Generic) += .1f;

        }
        // 需求：饰品可见性设为不可见时不出现猫猫弹幕（buff 照常保留，不受可见性影响）
        player.GetModPlayer<WarAndPeacePlayer>().CatVisible = !hideVisual;
        base.UpdateAccessory(player, hideVisual);
    }

    /// <summary>
    /// 装备在时装（幻化）槽时调用：只点亮猫猫弹幕，不提供 buff。
    /// <para>时装栏为固定显示，因此这里不判断饰品可见性。</para>
    /// </summary>
    public override void UpdateVanity(Player player)
    {
        if (!this.IsRecordUnlocked) return;
        player.GetModPlayer<WarAndPeacePlayer>().CatVisible = true;
        base.UpdateVanity(player);
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => this.IsRecordUnlocked;
}