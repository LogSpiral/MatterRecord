using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.IO;
using Terraria.WorldBuilding;
using MonoMod.Cil;
using System.Diagnostics;
using Terraria.DataStructures;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent;
using Microsoft.Xna.Framework;
using Terraria.UI.Chat;
using Terraria.ModLoader.IO;
using MatterRecord;
using Basic.Reference.Assemblies;
using Terraria.GameContent.ItemDropRules;
namespace MatterRecord.Contents.EternalWine
{
    public class EternalWine : ModItem
    {
        public override void Load()
        {
            IL_Player.QuickHeal += EternalWineHealModify;
            On_Player.QuickHeal_GetItemToUse += GetEternalWineToHeal;
            On_Player.ApplyPotionDelay += WineBanDelay;
            base.Load();
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
                if (item.stack <= 0 || item.type <= 0 || !item.potion && item.type != ModContent.ItemType<EternalWine>() || item.healLife <= 0)
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
            //Main.NewText("酒来!!");
            StackTrace stackTrace = new StackTrace();
            var str = stackTrace.GetFrame(4).GetMethod().Name;
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

            //TooltipLine line1 = tooltips.FirstOrDefault(x => x.Mod == "Terraria" && x.Name == "BuffTime");
            //tooltips.Remove(line1);
        }
        public override bool? UseItem(Player player)
        {
            return base.UseItem(player);
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
                player.GetModPlayer<EternalWinePlayer>().LifeDebt = healValue;
                player.GetModPlayer<EternalWinePlayer>().LifeDebtMax = healValue;
                if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                {
                    var packet = Mod.GetPacket();
                    packet.Write((byte)PacketType.EternalWinePlayerSync);
                    packet.Write((byte)player.whoAmI);
                    packet.Write(healValue);
                    packet.Write(healValue);
                    packet.Send(-1, player.whoAmI);
                }
            }

        }

    }
    public class EternalWinePlayer : ModPlayer
    {
        public float LifeDebt;
        public float LifeDebtMax;
        public override void SaveData(TagCompound tag)
        {
            tag["Debt"] = LifeDebt;
            tag["MaxDebt"] = LifeDebtMax;
            base.SaveData(tag);
        }
        public override void LoadData(TagCompound tag)
        {
            LifeDebt = tag.Get<float>("Debt");
            LifeDebtMax = tag.Get<float>("MaxDebt");
            base.LoadData(tag);
        }
        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            ModPacket packet = Mod.GetPacket();
            packet.Write((byte)PacketType.EternalWinePlayerSync);
            packet.Write((byte)Player.whoAmI);
            packet.Write(LifeDebt);
            packet.Write(LifeDebtMax);
            packet.Send(toWho, fromWho);
        }
        public override void Load()
        {
            IL_Player.UpdateLifeRegen += LifeDebtPaying;
            base.Load();
        }

        private void LifeDebtPaying(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            for (int n = 0; n < 3; n++)
                if (!cursor.TryGotoNext(i => i.MatchLdcI4(120)))
                    return;

            cursor.Index--;
            cursor.EmitDelegate<Action<Player>>(plr =>
            {
                var mplr = plr.GetModPlayer<EternalWinePlayer>();
                while (mplr.LifeDebt > 0 && plr.lifeRegenCount >= 120)
                {
                    plr.lifeRegenCount -= 120;
                    mplr.LifeDebt--;
                }
            });
            cursor.EmitLdarg0();
        }

        public override void Unload()
        {
            IL_Player.UpdateLifeRegen -= LifeDebtPaying;

            base.Unload();
        }
        public override void ResetEffects()
        {
            if (LifeDebt <= 0 && LifeDebtMax != -1)
            {
                LifeDebt = LifeDebtMax = -1;
                SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            }
            if (LifeDebt > 0)
                Player.AddBuff(ModContent.BuffType<LifeRegenStagnant>(), 2);
            base.ResetEffects();
        }
        public override void UpdateDead()
        {
            LifeDebt = LifeDebtMax = -1;
            base.UpdateDead();
        }
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (LifeDebtMax != -1 && Main.myPlayer == Player.whoAmI)
            {
                Vector2 cen = Player.Center + Player.gfxOffY * Vector2.UnitY - Main.screenPosition - new Vector2(16, 128);
                drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/EternalWine/LifeRegenStagnant_Recover").Value, cen, null, Color.White, 0, new Vector2(), 1f, 0));

                drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/EternalWine/LifeRegenStagnant").Value, cen, new Rectangle(0, 0, 32, (int)(32f * LifeDebt / LifeDebtMax)), Color.White, 0, new Vector2(), 1f, 0));
                string text = $"待偿还生命值{LifeDebt}/{LifeDebtMax}";
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text, cen + new Vector2(16, 48), Color.White, Color.Black, 0, FontAssets.MouseText.Value.MeasureString(text) * .5f, Vector2.One);
                //Main.spriteBatch.DrawString(FontAssets.MouseText.Value,, cen + Vector2.UnitY * 48, Color.White);
            }
            base.ModifyDrawInfo(ref drawInfo);
        }
    }
    public class EternalWineSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int ChestIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Buried Chests"));

            if (ChestIndex != -1)
            {
                tasks.Insert(ChestIndex + 1, new EternalWinePass("EternalWineSpawn", 100f));

            }
            base.ModifyWorldGenTasks(tasks, ref totalWeight);
        }
    }
    public class EternalWinePass : GenPass
    {
        public EternalWinePass(string str, float value) : base(str, value)
        {

        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "永生之酒";
            foreach (var chest in Main.chest)
            {
                if (chest != null)
                {
                    if (GenVars.hellChestItem.Contains(chest.item[0].type))
                    {
                        if (WorldGen.genRand.NextBool(10))
                            chest.item[0].SetDefaults(ModContent.ItemType<EternalWine>());
                    }
                }
            }

        }
    }
    public class Eternal : ModBuff
    {
        public override void Update(Player player, ref int buffIndex)
        {
            player.immune = true;
            player.immuneTime = 2;
            base.Update(player, ref buffIndex);
        }

    }
    public class LifeRegenStagnant : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            base.SetStaticDefaults();
        }
    }
    public class EternalWineGlobalItem : GlobalItem 
    {
        public override void ModifyItemLoot(Item item, ItemLoot itemLoot)
        {
            if (item.type == ItemID.ObsidianLockbox) 
            {
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
            base.ModifyItemLoot(item, itemLoot);
        }
    }
}
