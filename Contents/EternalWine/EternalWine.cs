using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
namespace MatterRecord.Contents.EternalWine;

public class EternalWine : ModItem
{
    public override void Load()
    {
        IL_Player.QuickBuff += EternalWineQuickBuff;
        IL_Player.QuickHeal += EternalWineHealModify;
        On_Player.QuickHeal_GetItemToUse += GetEternalWineToHeal;
        On_Player.ApplyPotionDelay += WineBanDelay;
        base.Load();
    }

    private void EternalWineQuickBuff(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        for (int i = 0; i < 3; i++)
            if (!cursor.TryGotoNext(i => i.MatchLdloc(5)))
                return;

        int currentIndex = cursor.Index;

        if (!cursor.TryGotoNext(i => i.MatchLdloc(4)))
            return;

        ILLabel curLabel = cursor.MarkLabel();

        cursor.Index = currentIndex;
        cursor.Index++;
        for (int n = 0; n < 3; n++)
            cursor.Remove();
        cursor.EmitDelegate<Func<Item, bool>>(
            item =>
            {
                bool flag1 = item.buffTime <= 0;
                bool flag2 = item.type != ModContent.ItemType<EternalWine>();
                return flag1 && flag2;
            }
            );
        cursor.EmitBrtrue(curLabel);

        for (int i = 0; i < 3; i++)//8
            if (!cursor.TryGotoNext(i => i.MatchLdloc(5)))
                return;
        cursor.EmitLdloc(5);
        cursor.EmitLdarg0();
        cursor.EmitDelegate<Action<Item, Player>>((item, player) =>
        {
            if (item.type != ModContent.ItemType<EternalWine>())
                return;
            int buffTime;
            int healValue;
            if (NPC.downedMoonlord)
            {
                healValue = 175;
                buffTime = 90;
            }
            else if (Main.hardMode)
            {
                healValue = 125;
                buffTime = 60;
            }
            else
            {
                healValue = 75;
                buffTime = 30;
            }
            player.AddBuff(ModContent.BuffType<Eternal>(), buffTime);
            player.GetModPlayer<EternalWinePlayer>().SetLifeDebt(healValue, healValue);
            player.statLife += healValue;
            if (player.whoAmI == Main.myPlayer)
                player.HealEffect(healValue);
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                EternalWineSync.Get(player.whoAmI, healValue, healValue).Send(-1, player.whoAmI);

        });
    }

    private static void WineBanDelay(On_Player.orig_ApplyPotionDelay orig, Player self, Item sItem)
    {
        if (sItem.type != ModContent.ItemType<EternalWine>())
            orig(self, sItem);
    }

    private static Item GetEternalWineToHeal(On_Player.orig_QuickHeal_GetItemToUse orig, Player self)
    {
        int num = self.statLifeMax2 - self.statLife;
        Item result = null;
        int num2 = -self.statLifeMax2;
        int num3 = 58;
        if (self.useVoidBag())
            num3 = 98;
        Item resultEternal = null;
        for (int i = 0; i < num3; i++)
        {
            Item item = i >= 58 ? self.bank4.item[i - 58] : self.inventory[i];
            if (item.stack <= 0 || item.type <= ItemID.None || !item.potion && item.type != ModContent.ItemType<EternalWine>() || item.healLife <= 0)
                continue;

            if (!CombinedHooks.CanUseItem(self, item))
                continue;
            if (item.type == ModContent.ItemType<EternalWine>())
            {
                resultEternal = item;
                continue;
            }

            int num4 = self.GetHealLife(item, true) - num;
            if (item.type == ItemID.RestorationPotion && num4 < 0)
            {
                num4 += 30;
                if (num4 > 0)
                    num4 = 0;
            }

            if (num2 < 0)
            {
                if (num4 > num2)
                {
                    result = item;
                    num2 = num4;
                }
            }
            else if (num4 < num2 && num4 >= 0)
            {
                result = item;
                num2 = num4;
            }
        }
        if (self.potionDelay > 0 || result == null)
            result = resultEternal;
        return result;
    }

    private static void EternalWineHealModify(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchRet()))
            return;
        cursor.Index++;
        ILLabel label = cursor.MarkLabel();
        cursor.Index -= 4;
        for (int n = 0; n < 3; n++)
            cursor.Remove();
        cursor.EmitDelegate<Func<Player, bool>>(plr =>
        {
            int num3 = 58;
            if (plr.useVoidBag())
                num3 = 98;
            for (int i = 0; i < num3; i++)
            {
                Item item = i >= 58 ? plr.bank4.item[i - 58] : plr.inventory[i];
                if (item.type == ModContent.ItemType<EternalWine>())
                {
                    return true;
                }
            }
            return plr.potionDelay <= 0;
        });
        cursor.EmitBrtrue(label);
    }

    public override void Unload()
    {
        IL_Player.QuickHeal -= EternalWineHealModify;
        On_Player.QuickHeal_GetItemToUse -= GetEternalWineToHeal;
        On_Player.ApplyPotionDelay -= WineBanDelay;
        base.Unload();
    }

    public override void SetDefaults()
    {
        Item.ResearchUnlockCount = 1;
        Item.width = 20;
        Item.height = 26;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useAnimation = 17;
        Item.useTime = 17;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item3;
        Item.maxStack = 1;
        Item.consumable = false;
        Item.rare = ItemRarityID.Orange;
        //Item.buffTime = 30;
        //Item.buffType = ModContent.BuffType<Eternal>();
        Item.value = Item.buyPrice(gold: 1);
        Item.healLife = 100;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        TooltipLine line = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "HealLife");
        tooltips.Remove(line);
    }
    public override bool CanUseItem(Player player)
    {
        return !player.HasBuff<LifeRegenStagnant>();
    }

    public override void GetHealLife(Player player, bool quickHeal, ref int healValue)
    {
        int buffTime;
        if (NPC.downedMoonlord)
        {
            healValue = 175;
            buffTime = 90;
        }
        else if (Main.hardMode)
        {
            healValue = 125;
            buffTime = 60;
        }
        else
        {
            healValue = 75;
            buffTime = 30;
        }
        if (quickHeal)
        {
            player.AddBuff(ModContent.BuffType<Eternal>(), buffTime);
            player.GetModPlayer<EternalWinePlayer>().SetLifeDebt(healValue, healValue);
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
            {
                EternalWineSync.Get(Main.myPlayer, healValue, healValue).Send(-1,Main.myPlayer);
            }
        }
    }
}