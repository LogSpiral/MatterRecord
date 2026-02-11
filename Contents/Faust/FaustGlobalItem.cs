using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.UI;

namespace MatterRecord.Contents.Faust;

public class FaustGlobalItem : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (!Faust.Active) return;

        TooltipLine dummy = null;
        foreach (var tip in tooltips)
        {
            if (tip.Name == "ItemName")
                dummy = tip;
        }
        tooltips.Clear();
        tooltips.Add(dummy);
        if (item.value == 0 || item.type == ModContent.ItemType<Faust>() || Main.ItemDropsDB.GetRulesForItemID(item.type).Any() || (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin))
            tooltips.Add(new TooltipLine(Mod, "CantBuyIt", Language.GetTextValue("Mods.MatterRecord.Items.Faust.CantButIt")) { OverrideColor = Color.Lerp(Color.Gray, Color.DarkGray, MathF.Cos(Main.GlobalTimeWrappedHourly) * .5f + .5f) });
        else
        {
            var text5 = "";
            var num16 = (long)item.GetStoreValue() * 4;
            var num12 = 0L;
            var num13 = 0L;
            var num14 = 0L;
            var num15 = 0L;
            if (num16 >= 1000000)
            {
                num12 = num16 / 1000000;
                num16 -= num12 * 1000000;
            }
            if (num16 >= 10000)
            {
                num13 = num16 / 10000;
                num16 -= num13 * 10000;
            }

            if (num16 >= 100)
            {
                num14 = num16 / 100;
                num16 -= num14 * 100;
            }

            if (num16 >= 1)
                num15 = num16;

            if (num12 > 0)
                text5 = text5 + num12 + " " + Lang.inter[15].Value + " ";

            if (num13 > 0)
                text5 = text5 + num13 + " " + Lang.inter[16].Value + " ";

            if (num14 > 0)
                text5 = text5 + num14 + " " + Lang.inter[17].Value + " ";

            if (num15 > 0)
                text5 = text5 + num15 + " " + Lang.inter[18].Value + " ";

            float num17 = (float)(int)Main.mouseTextColor / 255f;
            var color2 = Color.White;
            if (num12 > 0)
                color2 = new Color((byte)(220f * num17), (byte)(220f * num17), (byte)(198f * num17), Main.mouseTextColor);
            else if (num13 > 0)
                color2 = new Color((byte)(224f * num17), (byte)(201f * num17), (byte)(92f * num17), Main.mouseTextColor);
            else if (num14 > 0)
                color2 = new Color((byte)(181f * num17), (byte)(192f * num17), (byte)(193f * num17), Main.mouseTextColor);
            else if (num15 > 0)
                color2 = new Color((byte)(246f * num17), (byte)(138f * num17), (byte)(96f * num17), Main.mouseTextColor);

            tooltips.Add(new TooltipLine(Mod, "ThePrice", $"{Language.GetTextValue("Mods.MatterRecord.Items.Faust.BuyPrice")}{text5}") { OverrideColor = color2 });
        }
        base.ModifyTooltips(item, tooltips);
    }

    public override void Load()
    {
        On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
        MonoModHooks.Add(typeof(ItemLoader).GetMethod(nameof(ItemLoader.RightClick), BindingFlags.Static | BindingFlags.Public), FaustModifyRightClick);
        base.Load();
    }

    public static void FaustModifyRightClick(Action<Item, Player> orig, Item item, Player player)
    {
        if (!Faust.Active) goto Label;
        if (item.value == 0) goto Label;
        if (item.type == ModContent.ItemType<Faust>()) goto Label;
        if (Main.ItemDropsDB.GetRulesForItemID(item.type).Any()) goto Label;
        if (player.CanAfford((long)item.GetStoreValue() * 4))
        {
            if (Main.stackSplit > 1) return;
            int m = Main.superFastStack + 1;
            for (int n = 0; n < m; n++)
            {
                bool flag1 = Main.mouseItem.type == item.type && Main.mouseItem.stack < Main.mouseItem.maxStack;
                bool flag2 = Main.mouseItem.type == ItemID.None;
                if (!(flag1 || flag2)) return;

                player.BuyItem((long)item.GetStoreValue() * 4);
                player.GetModPlayer<FaustPlayer>().ConsumedMoney += (ulong)item.GetStoreValue() * 4UL;
                if (flag1)
                    Main.mouseItem.stack++;
                else if (flag2)
                {
                    Main.mouseItem = item.Clone();
                    Main.mouseItem.stack = 1;
                }
                if (n == 0)
                {
                    SoundEngine.PlaySound(SoundID.Coins);
                    ItemSlot.RefreshStackSplitCooldown();
                }
            }
            return;
        }
    Label:
        orig.Invoke(item, player);
    }

    private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item)
    {
        if (Faust.Active) return;
        orig.Invoke(item);
    }

    public override bool CanRightClick(Item item)
    {
        if (item.type == ItemID.None) return false;
        if (!Faust.Active) return false;
        if (item.value == 0) return false;
        if (item.type == ModContent.ItemType<UnloadedItem>()) return false;
        if (item.type == ModContent.ItemType<Faust>()) return false;

        if (!Main.LocalPlayer.CanAfford((long)item.GetStoreValue() * 4)) return false;
        if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin) return false;
        if (Faust.Active)
            return Main.mouseRightRelease = true;

        return false;
    }
}
