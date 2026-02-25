using MatterRecord.Contents.Recorder;
using System;

namespace MatterRecord.Contents.WarAndPeace;

public class WarAndPeace : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.WarAndPeace;
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 1);
        Item.width = Item.height = 32;
        base.SetDefaults();
    }
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.LicenseCat);
    }
    public static bool IsPeace => ((int)(8 * Math.Sin(8 * DateTime.Now.Day)) + 8) % 2 == 1;

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!this.IsRecordUnlocked) return;
        var dayOfWeek = DateTime.Now.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Sunday)
            player.AddBuff(IsPeace ? ModContent.BuffType<Holiday_Peace>() : ModContent.BuffType<Holiday_War>(), 2);
        else if ((int)dayOfWeek % 2 == 0)
        {
            player.endurance += .1f;
            player.AddBuff(ModContent.BuffType<Peace>(), 2);
        }
        else
        {
            player.AddBuff(ModContent.BuffType<War>(), 2);
            player.GetDamage(DamageClass.Generic) += .1f;
            player.GetDamage(DamageClass.Generic).Flat += 5f;
        }
        base.UpdateAccessory(player, hideVisual);
    }
    public override bool CanEquipAccessory(Player player, int slot, bool modded) => this.IsRecordUnlocked;
}