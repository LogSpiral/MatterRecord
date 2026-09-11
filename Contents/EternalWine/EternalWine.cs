using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
namespace MatterRecord.Contents.EternalWine;

public class EternalWine : ModItem
{
    public override void Load()
    {
        On_Player.QuickBuff_UseItemForBuff += EternalWineQuickBuffEffect;
        IL_Player.QuickBuff_ShouldUseItem += EternalWineQuickBuffCheck;
        IL_Player.QuickHeal += EternalWineHealModify;
        On_Player.QuickHeal_GetItemToUse += GetEternalWineToHeal;
        On_Player.ApplyPotionDelay += WineBanDelay;
        base.Load();
    }

    private void EternalWineQuickBuffEffect(On_Player.orig_QuickBuff_UseItemForBuff orig, Player self, Item item, int btype)
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
            GetStageValues(out int healValue, out int buffTime);
            player.AddBuff(ModContent.BuffType<Eternal>(), buffTime);
            player.GetModPlayer<EternalWinePlayer>().SetLifeDebt(healValue, healValue);
            player.statLife += healValue;
            if (player.whoAmI == Main.myPlayer)
                player.HealEffect(healValue);
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                EternalWineSync.Get(player.whoAmI, healValue, healValue).Send(-1, player.whoAmI);

    private void EternalWineQuickBuffCheck(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        for (int i = 0; i < 3; i++)
            if (!cursor.TryGotoNext(i => i.MatchLdarg1()))
                return;
        cursor.Index++;
        cursor.RemoveRange(2);
        if (!cursor.Next.MatchBle(out var label)) return;
        cursor.Remove();
        cursor.EmitDelegate<Func<Item, bool>>(
            item =>
            {
                bool flag1 = item.buffTime <= 0;
                bool flag2 = item.type != ModContent.ItemType<EternalWine>();
                return flag1 && flag2;
            }
            );
        cursor.EmitBrtrue(label);
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

    /// <summary>
    /// 按当前世界进度取永生之酒的效果档位（未进困难 / 困难 / 击败月总，共三档）。
    /// </summary>
    /// <param name="healValue">借出的生命值，同时也是饮用时的回复量。</param>
    /// <param name="buffTime">「永生」增益的持续帧数（60 帧 = 1 秒），即无敌帧时长。</param>
    private static void GetStageValues(out int healValue, out int buffTime)
    {
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
    }

    /// <summary>
    /// 动态生成物品提示：把当前档位的「借贷生命」与「无敌秒数」填入首行占位符，
    /// 并按世界进度在末尾追加一条解锁提示（击败月总后不再追加）。
    /// </summary>
    /// <param name="tooltips">待显示的工具提示行集合。</param>
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // 原版会依据 Item.healLife 自动生成「恢复生命 xx」行，本物品改用自定义文案表达，故移除
        TooltipLine healLine = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "HealLife");
        if (healLine != null)
            tooltips.Remove(healLine);

        GetStageValues(out int healValue, out int buffTime);

        // hjson 的 Tooltip 被原版逐行拆成 Tooltip0/Tooltip1…，占位符只出现在首行，
        // 因此只格式化首行自身文本，避免破坏多行结构
        TooltipLine firstLine = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "Tooltip0");
        if (firstLine != null)
            firstLine.Text = string.Format(firstLine.Text, healValue, buffTime / 60f);

        // 末尾追加解锁提示：未进困难模式提示困难模式，进困难但未击败月总提示月总，击败月总后不再提示
        string unlockKey = NPC.downedMoonlord ? null : Main.hardMode ? "UnlockMoonLord" : "UnlockHardMode";
        if (unlockKey != null)
        {
            int lastTooltip = tooltips.FindLastIndex(x => x.Mod == "Terraria" && x.Name.StartsWith("Tooltip"));
            if (lastTooltip != -1)
                tooltips.Insert(lastTooltip + 1, new TooltipLine(Mod, "EternalWineUnlock", this.GetLocalizedValue(unlockKey)));
        }
    }

    public override bool CanUseItem(Player player)
    {
        return !player.HasBuff<LifeRegenStagnant>();
    }

    public override void GetHealLife(Player player, bool quickHeal, ref int healValue)
    {
        GetStageValues(out healValue, out int buffTime);
        if (quickHeal)
        {
            player.AddBuff(ModContent.BuffType<Eternal>(), buffTime);
            player.GetModPlayer<EternalWinePlayer>().SetLifeDebt(healValue, healValue);
            if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
            {
                EternalWineSync.Get(Main.myPlayer, healValue, healValue).Send(-1, Main.myPlayer);
            }
        }
    }
}