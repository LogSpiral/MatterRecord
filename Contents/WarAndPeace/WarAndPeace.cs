using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.WarAndPeace;

public class WarAndPeace : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 1);
        Item.width = Item.height = 32;
        base.SetDefaults();
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var dayOfWeek = DateTime.Now.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Sunday)
            player.AddBuff(ModContent.BuffType<Holiday>(), 2);
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
}
public class WarAndPeaceGlobalItem : GlobalItem 
{
    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.type == ItemID.LicenseCat)
            player.QuickSpawnItem(item.GetSource_Misc("CatLicense"), ModContent.ItemType<WarAndPeace>());

        base.OnConsumeItem(item, player);
    }
}
public class WarAndPeacePlayer : ModPlayer
{
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Player.HasBuff<Peace>())
            modifiers.FinalDamage.Flat -= 5;

    }
}
public class War : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
    }
}
public class Peace : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
    }
}
public class Holiday : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
    }
}
