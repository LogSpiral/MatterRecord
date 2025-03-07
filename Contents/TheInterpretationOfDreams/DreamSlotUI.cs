using LogSpiralLibrary.ForFun.ScreenTransformUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class DreamSlotUI : UIState
    {
        public static bool Visible;
        public static DreamSlotUI instance;
        public static void Open()
        {
            Visible = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
            instance.RemoveAllChildren();
            instance.OnInitialize();


            instance.mainPanel.Width = new(174, 0);

            var vec = Main.MouseScreen;
            vec -= Main.ScreenSize.ToVector2() * .5f;
            vec *= Main.GameViewMatrix.Zoom.X;
            vec += Main.ScreenSize.ToVector2() * .5f;

            instance.mainPanel.Left = new((int)vec.X + 80, 0);
            instance.mainPanel.Top = new((int)vec.Y - 270, 0);
            instance.mainPanel.Height = new(540, 0);
            instance.Height = new(600, 0);
            instance.Recalculate();
        }
        public static void Close()
        {
            Visible = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
        UIPanel mainPanel;
        public override void OnInitialize()
        {
            instance = this;
            mainPanel = new UIPanel()
            {
                Width = new(174, 0),
                Height = new(540, 0)
            };
            Append(mainPanel);

            for (int n = 0; n < 12; n++)
            {
                int i = n % 3;
                int j = n / 3;
                DreamItemSlot slot = new()
                {
                    Width = new(40, 0),
                    Height = new(40, 0),
                    Left = new(5 + 50 * i, 0),
                    Top = new(10 + 50 * j, 0),
                    index = n,
                };
                mainPanel.Append(slot);
            }
            mainPanel.Append(new UIHorizontalSeparator
            {
                Width = StyleDimension.FromPercent(1f),
                Color = Color.Lerp(Color.White, new Color(63, 65, 151, 255), 0.85f) * 0.9f,
                Top = new(210, 0)
            });
            for (int n = 0; n < 18; n++)
            {
                int i = n % 3;
                int j = n / 3;
                DreamPowerSlot slot = new()
                {
                    Width = new(40, 0),
                    Height = new(40, 0),
                    Left = new(5 + 50 * i, 0),
                    Top = new(220 + 50 * j, 0),
                    targetState = (DreamState)(1 << n)
                };
                mainPanel.Append(slot);
            }
            base.OnInitialize();
        }
    }
    public class DreamItemSlot : UIPanel
    {
        public int index;
        public Item BindItem => _item ??= Main.LocalPlayer.GetModPlayer<DreamPlayer>().dreamItemSlots[index];
        Item _item;
        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            if (Main.mouseItem.type != ItemID.None)
                return;
            if (BindItem.type == ItemID.None) return;
            Main.mouseItem = BindItem.Clone();
            BindItem.TurnToAir();
        }
        public override void RightClick(UIMouseEvent evt)
        {
            base.RightClick(evt);
            if (BindItem.type == ItemID.None) return;
            if (Main.mouseItem.type == BindItem.type && Main.mouseItem.stack != Main.mouseItem.maxStack)
            {
                Main.mouseItem.stack++;
                goto Label;
            }
            else if (Main.mouseItem.type == ItemID.None)
            {
                Main.mouseItem = BindItem.Clone();
                Main.mouseItem.stack = 1;
                goto Label;
            }
            return;
        Label:
            BindItem.stack--;
            if (BindItem.stack <= 0)
                BindItem.TurnToAir();
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);
            var position = GetDimensions().Center();
            if (BindItem.type != ItemID.None)
            {
                ItemSlot.DrawItemIcon(BindItem, 0, spriteBatch, position, 1f, 40, Color.White);
                if (BindItem.stack > 1)
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, BindItem.stack.ToString(), position, Color.White, 0, default, Vector2.One * .75f);
                if (IsMouseHovering)
                    UICommon.TooltipMouseText(BindItem.HoverName);
            }
            else
                ItemSlot.DrawItemIcon(new Item(ItemID.RainCloud), 0, spriteBatch, position, 1f, 40, Color.White);

            if (index == 0) return;

            spriteBatch.Draw(TextureAssets.NpcHead[index switch
            {
                1 => 10,
                2 => 26,
                3 => 25,
                4 => 19,
                5 => 1,
                6 => 24,
                7 => 7,
                8 => 20,
                9 => 14,
                10 => 16,
                11 or _ => 0
            }].Value, position, Color.White * .5f);
        }
    }
    public class DreamPowerSlot : UIPanel
    {
        public DreamState targetState;
        public override void LeftClick(UIMouseEvent evt)
        {
            var mplr = Main.LocalPlayer.GetModPlayer<DreamPlayer>();
            if (!mplr.CheckUnlock(targetState)) return;
            mplr.ActiveState ^= targetState;
            base.LeftClick(evt);
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var mplr = Main.LocalPlayer.GetModPlayer<DreamPlayer>();
            var position = GetDimensions().Center();
            if (mplr.CheckUnlock(targetState))
                ItemSlot.DrawItemIcon(new Item(ItemID.Cloud), 0, spriteBatch, position, 1f, 40, Color.White);
            else
                ItemSlot.DrawItemIcon(new Item(ItemID.RainCloud), 0, spriteBatch, position, 1f, 40, Color.White);

            if (mplr.CheckActive(targetState))
            {
                spriteBatch.Draw(TextureAssets.Extra[98].Value, position, null, Color.White with { A = 0 } * .75f, Main.GlobalTimeWrappedHourly, new Vector2(36), new Vector2(1, .25f), 0, 0);
                spriteBatch.Draw(TextureAssets.Extra[98].Value, position, null, Color.White with { A = 0 } * .25f, Main.GlobalTimeWrappedHourly * -2f, new Vector2(36), new Vector2(2, .5f), 0, 0);
            }

            if (targetState == DreamState.SkeletonMerchant)
                spriteBatch.Draw(ModContent.Request<Texture2D>("MatterRecord/Contents/TheInterpretationOfDreams/SkeletonMerchantHead").Value, position, Color.White * .5f);
            else
                spriteBatch.Draw(TextureAssets.NpcHead[targetState switch
                {
                    DreamState.Princess => 45,
                    DreamState.SantaClaus => 11,
                    DreamState.GoblinTinkerer => 9,
                    DreamState.Merchant => 2,
                    DreamState.Mechanic => 8,
                    DreamState.Demolitionist => 4,
                    DreamState.TaxCollector => 23,
                    DreamState.Steampunker => 13,
                    DreamState.Dryad => 5,
                    DreamState.WitchDoctor => 18,
                    DreamState.Painter => 17,
                    DreamState.Truffle => 12,
                    DreamState.PartyGirl => 15,
                    DreamState.ArmsDealer => 6,
                    DreamState.Angler => 22,
                    DreamState.Nurse => 3,
                    DreamState.TravelingMerchant => 21,
                    _ => 0
                }].Value, position, Color.White * .5f);


            if (IsMouseHovering)
                UICommon.TooltipMouseText(Language.GetTextValue($"Mods.MatterRecord.Items.TheInterpretationOfDreams.{targetState}") + "\n" + Language.GetTextValue($"Mods.MatterRecord.Items.TheInterpretationOfDreams.{(mplr.CheckUnlock(targetState) ? "Hint" : "Unlock")}"));
            base.DrawSelf(spriteBatch);
        }
    }
    public class DreamUISystem : ModSystem
    {
        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                instance = this;
                dreamSlotUI = new DreamSlotUI();
                userInterface = new UserInterface();
                dreamSlotUI.Activate();
                userInterface.SetState(dreamSlotUI);
            }
            base.Load();
        }
        public static DreamUISystem instance;
        public DreamSlotUI dreamSlotUI;
        public UserInterface userInterface;
        public override void UpdateUI(GameTime gameTime)
        {
            if (DreamSlotUI.Visible)
            {
                userInterface?.Update(gameTime);
            }
            base.UpdateUI(gameTime);
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (MouseTextIndex != -1)
            {
                layers.Insert(MouseTextIndex, new LegacyGameInterfaceLayer(
                   "MatterRecord:TheInterpretationOfDreams",
                   delegate
                   {
                       if (DreamSlotUI.Visible)
                           dreamSlotUI.Draw(Main.spriteBatch);
                       return true;
                   },
                   InterfaceScaleType.UI)
               );
            }
            base.ModifyInterfaceLayers(layers);
        }

    }
}
