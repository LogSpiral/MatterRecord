using MatterRecord.Contents.AliceInWonderland;
using MatterRecord.Contents.Recorder;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    internal static bool Active;
    private static int _updateTimer;

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
            List<string> hints = RecorderPlayer.GetNotHintedKeys();

            if (hints.Count > 0)
            {
                string key = Main.rand.Next(hints);
                RecorderPlayer.SetHintedViaKey(key);
                CombatText.NewText(
                    player.Hitbox,
                    Color.MediumPurple,
                    this.GetLocalizedValue(key));
            }
            else
            {
                hints = RecorderPlayer.GetLockedKeys();
                if (hints.Count == 0)
                {
                    CombatText.NewText(
                        player.Hitbox,
                        Color.MediumPurple,
                        this.GetLocalizedValue("NoMoreHints")
                        );
                }
                else
                {
                    hints.Add("NoMoreHints");
                    hints.Add("NoMoreHints");
                    hints.Add("NoMoreHints");
                    CombatText.NewText(
                        player.Hitbox,
                        Color.MediumPurple,
                        this.GetLocalizedValue(Main.rand.Next(hints))
                        );
                }
            }


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
        if (!this.IsRecordUnlocked) return;
        if (_updateTimer <= 0)
        {
            _updateTimer = 30;

            ItemRecords[] records =
                [
                    ItemRecords.AliceInWonderland,
                    ItemRecords.DonQuijoteDeLaMancha,
                    ItemRecords.LittlePrince,
                    ItemRecords.TheOldManAndTheSea,
                    ItemRecords.WarAndPeace,
                    ItemRecords.TheInterpretationOfDreams,
                    ItemRecords.TheoryOfFreedom
                ];

            var player = Main.LocalPlayer;
            Item[][] inventories =
            [
                player.inventory,
                    player.armor,
                    player.dye,
                    player.miscEquips,
                    player.miscDyes,
                    [player.trashItem],
                    player.bank.item,
                    player.bank2.item,
                    player.bank3.item,
                    player.bank4.item
            ];
            foreach (var inventory in inventories)
                foreach (var item in inventory)
                {
                    if (item.ModItem is IRecordBookItem book && records.Contains(book.RecordType))
                        RecorderPlayer.SetHintState(book.RecordType, RecordHintState.Found);
                }

            foreach (var record in records)
            {
                if (RecorderSystem.CheckUnlock(record))
                    RecorderPlayer.SetHintState(record, RecordHintState.Unlocked);
            }
        }
        _updateTimer--;


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
    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (!this.IsRecordUnlocked) return true;
        List<TooltipLine> extraLines = [];
        Dictionary<string, RecordHintState> hints = RecorderPlayer.GetHintedKeysWithState();
        if (hints.Count == 0)
            return true;
        extraLines.Add(new TooltipLine(Mod, "RecordHintList", this.GetLocalizedValue("HintList")));
        int counter = 0;
        foreach (var (hint, state) in hints)
        {
            var hintText = this.GetLocalizedValue(hint);
            var localized = this.GetLocalization(state switch
            {
                RecordHintState.Hinted => "HintNotFound",
                RecordHintState.Found => "HintFound",
                RecordHintState.Unlocked => "HintUnlocked",
                _ => "HintNotFound"
            });
            hintText = localized.Format([hintText]);
            var line = new TooltipLine(Mod, "RecordHint" + counter, hintText)
            {
                OverrideColor = state switch
                {
                    RecordHintState.Hinted => Color.Gray,
                    RecordHintState.Found => Color.MediumPurple,
                    RecordHintState.Unlocked => Color.Yellow,
                    _ => Color.Transparent
                }
            };
            extraLines.Add(line);
            counter++;
        }

        MiscMethods.DrawTagTooltips(lines, extraLines, x, y);
        return true;
    }
}


