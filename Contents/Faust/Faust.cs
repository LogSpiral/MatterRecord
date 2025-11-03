using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.Localization;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace MatterRecord.Contents.Faust;

public class Faust : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.Faust;
    //旧版物品说明
    /*不行！不行！恶魔是利己主义者，
    对别人有益的事体，
			白白帮忙他决不干。
			你还是先说明条件！
			无条件的仆人会给家里带来危险*/

    //public override string Texture => $"Terraria/Images/Item_{ItemID.Book}";
    public override void SetDefaults()
    {
        Item.width = Item.height = 36;
        Item.value = Item.sellPrice(0, 1, 0, 0);
        Item.rare = ItemRarityID.Yellow;
        Item.useTime = Item.useAnimation = 60;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noUseGraphic = true;
        base.SetDefaults();
    }
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.Glass);
        base.AddRecipes();
    }
    public static bool Active;
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer) return null;
        var unit = Main.rand.NextVector2Unit();
        float factor = player.itemAnimation / 60f;
        var dust = Dust.NewDustPerfect(
            player.Center + unit * 64 * factor,
            DustID.DungeonSpirit,
            -unit * 4f + player.velocity,
            0,
            default,
            Main.rand.NextFloat(0.25f, 0.75f));
        dust.noGravity = true;
        if (player.itemAnimation == 1)
        {
            List<string> hints = [];

            if (!RecorderSystem.CheckUnlock(ItemRecords.AliceInWonderland))
                hints.Add("AliceInWonderlandHint");
            if (!RecorderSystem.CheckUnlock(ItemRecords.DonQuijoteDeLaMancha))
                hints.Add("DonQuijoteDeLaManchaHint");
            if (!RecorderSystem.CheckUnlock(ItemRecords.LittlePrince))
                hints.Add("LittlePrinceHint");
            if (!RecorderSystem.CheckUnlock(ItemRecords.TheOldManAndTheSea))
                hints.Add("TheOldManAndTheSeaHint");
            if (!RecorderSystem.CheckUnlock(ItemRecords.WarAndPeace))
                hints.Add("WarAndPeaceHint");
            if (!RecorderSystem.CheckUnlock(ItemRecords.TheInterpretationOfDreams))
                hints.Add("TheInterpretationOfDreamsHint");
            if (!RecorderSystem.CheckUnlock(ItemRecords.TheoryOfFreedom))
                hints.Add("TheoryOfFreedomHint");

            if (hints.Count > 0)
                CombatText.NewText(
                    player.Hitbox,
                    Color.MediumPurple,
                    this.GetLocalizedValue(Main.rand.Next(hints)));
            else
                CombatText.NewText(
                    player.Hitbox,
                    Color.MediumPurple,
                    this.GetLocalizedValue("NoMoreHints"));
            SoundEngine.PlaySound(SoundID.Item4);
            ParticleOrchestrator.Spawn_NightsEdge(new ParticleOrchestraSettings() { IndexOfPlayerWhoInvokedThis = (byte)Main.myPlayer, PositionInWorld = player.Center });
            for (int n = 0; n < 60; n++)
            {
                unit = Main.rand.NextVector2Unit() * new Vector2(2, 1);
                Dust.NewDustPerfect(
                    player.Center,
                    DustID.DungeonSpirit,
                    -unit * 8f + player.velocity,
                    0,
                    Color.White with { A = 0 },
                    Main.rand.NextFloat(0.5f, 1.5f))
                    .noGravity = true;
            }
        }
        return null;
    }
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (Active)
        {
            float factor = Main.GlobalTimeWrappedHourly % 1;
            spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), 0, origin, scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);
        }
        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (Active)
        {
            float factor = Main.GlobalTimeWrappedHourly % 1;
            spriteBatch.Draw(TextureAssets.Item[Type].Value, Item.Center - Main.screenPosition, null, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), rotation, new Vector2(18), scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);
        }
        base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }

    public override bool CanRightClick()
    {
        return true;
    }

    public override bool ConsumeItem(Player player)
    {
        return false;
    }

    public override void RightClick(Player player)
    {
        Active = !Active;
        SoundEngine.PlaySound(SoundID.Item4);
        base.RightClick(player);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var money = (long)Main.LocalPlayer.GetModPlayer<FaustPlayer>().ConsumedMoney;

        string content;

        if (money > 0)
            content = this.GetLocalizedValue("MoneySpent") + $"{Main.ValueToCoins(money)}";
        else
            content = this.GetLocalizedValue("WannaDeal");

        tooltips.Add(new(Mod, "ConsumedMoney", content)
        {
            OverrideColor = Color.Lerp(Color.Gray, Color.Red, 0.5f + 0.5f * MathF.Cos(Main.GlobalTimeWrappedHourly))
        });

        base.ModifyTooltips(tooltips);
    }
}

public class FaustGBItem : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        //if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<Faust>()) return;
        if (!Faust.Active) return;

        TooltipLine dummy = null;
        foreach (var tip in tooltips)
        {
            if (tip.Name == "ItemName")
                dummy = tip;
            //Main.NewText(tip.Name);
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

            //BlockConsumeCache = true;
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

    //public override bool ConsumeItem(Item item, Player player)
    //{
    //    if (BlockConsumeCache)
    //    {
    //        BlockConsumeCache = false;
    //        return false;
    //    }
    //    return base.ConsumeItem(item, player);
    //}
    public static bool BlockConsumeCache;

    //public override void RightClick(Item item, Player player)
    //{
    //    if (!Faust.Active) return;
    //    if (item.value == 0) return;
    //    if (item.type == ModContent.ItemType<Faust>()) return;
    //    if (Main.ItemDropsDB.GetRulesForItemID(item.type).Any()) return;
    //    if (player.CanAfford((long)item.GetStoreValue() * 4))
    //    {
    //        if (Main.mouseItem.type == ItemID.None)
    //        {
    //            player.BuyItem((long)item.GetStoreValue() * 4);
    //            Main.mouseItem = item.Clone();
    //            Main.mouseItem.stack = 1;
    //            SoundEngine.PlaySound(SoundID.CoinPickup);
    //        }
    //        else if (Main.mouseItem.type == item.type && Main.mouseItem.stack < Main.mouseItem.maxStack)
    //        {
    //            player.BuyItem((long)item.GetStoreValue() * 4);
    //            Main.mouseItem.stack++;
    //            SoundEngine.PlaySound(SoundID.CoinPickup);
    //        }
    //        BlockConsumeCache = true;
    //    }
    //    base.RightClick(item, player);
    //}
}

public class FaustPlayer : ModPlayer
{
    public ulong ConsumedMoney;

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(ConsumedMoney), ConsumedMoney);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        ConsumedMoney = tag.Get<ulong>(nameof(ConsumedMoney));
        base.LoadData(tag);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)PacketType.FaustSync);
        packet.Write((byte)Player.whoAmI);
        packet.Write(ConsumedMoney);
        packet.Send(toWho, fromWho);
    }

    // Called in ExampleMod.Networking.cs
    public void ReceivePlayerSync(BinaryReader reader)
    {
        ConsumedMoney = reader.ReadUInt64();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        FaustPlayer clone = (FaustPlayer)targetCopy;
        clone.ConsumedMoney = ConsumedMoney;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        FaustPlayer clone = (FaustPlayer)clientPlayer;

        if (ConsumedMoney != clone.ConsumedMoney)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void UpdateEquips()
    {
        //Player.statLifeMax2 -= (int)(ConsumedMoney / 500000);
        //if (Player.statLifeMax2 < 100) Player.statLifeMax2 = 100;
        base.UpdateEquips();
    }

    public override void ModifyLuck(ref float luck)
    {
        luck -= ConsumedMoney / 1000000 * 0.01f;

        base.ModifyLuck(ref luck);
    }
}