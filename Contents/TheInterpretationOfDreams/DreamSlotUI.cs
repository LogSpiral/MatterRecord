using MatterRecord.Contents.TheInterpretationOfDreams.TaijiNoYume;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

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
        vec /= Main.UIScale;
        instance.mainPanel.Left = new((int)vec.X, 0);
        instance.mainPanel.Top = new((int)vec.Y, 0);
        instance.mainPanel.Height = new(540, 0);
        instance.Height = new(600, 0);
        instance.Recalculate();
    }
    public static void Close()
    {
        Visible = false;
        SoundEngine.PlaySound(SoundID.MenuClose);
    }
    DraggablePanel mainPanel;
    public override void OnInitialize()
    {
        instance = this;
        mainPanel = new DraggablePanel()
        {
            Width = new(174, 0),
            Height = new(540, 0)
        };
        mainPanel.OnUpdate += elem =>
        {
            if (mainPanel.IsMouseHovering)
                Main.LocalPlayer.mouseInterface = true;
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
public class DraggablePanel : UIPanel
{
    public bool Dragging = false;
    public Vector2 Offset;
    public override void LeftMouseDown(UIMouseEvent evt)
    {
        if (evt.Target == this)
        {
            Dragging = true;
            var dimension = GetDimensions();
            Offset = new Vector2(evt.MousePosition.X - Left.Pixels, evt.MousePosition.Y - Top.Pixels);

        }
        base.LeftMouseDown(evt);
    }
    public override void LeftMouseUp(UIMouseEvent evt)
    {
        Dragging = false;
        base.LeftMouseUp(evt);
    }
    public override void Update(GameTime gameTime)
    {
        if (Dragging)
        {
            Left.Set(Main.mouseX - Offset.X, 0f);
            Top.Set(Main.mouseY - Offset.Y, 0f);
            Recalculate();
        }
        base.Update(gameTime);
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
        {
            int targetType = index switch
            {
                0 => ModContent.ItemType<BrokenDream>(),
                1 => ModContent.ItemType<WizardDream>(),
                2 => ModContent.ItemType<ZoologiseDream>(),
                3 => ModContent.ItemType<GolferDream>(),
                4 => ModContent.ItemType<PirateDream>(),
                5 => ModContent.ItemType<GuideDream>(),
                6 => ModContent.ItemType<TavernkeepDream>(),
                7 => ModContent.ItemType<ClothierDream>(),
                8 => ModContent.ItemType<StylistDream>(),
                9 => ModContent.ItemType<DyeTraderDream>(),
                10 => ModContent.ItemType<CybrogDream>(),
                11 or _ => ModContent.ItemType<TaijiNoYume.TaijiNoYume>()
            };
            if (Main.mouseItem.type != targetType)
                return;

            if (BindItem.stack < Main.mouseItem.maxStack)
            {
                int origStack = BindItem.stack;
                BindItem.SetDefaults(targetType);
                int delta = Math.Min(Main.mouseItem.stack, Main.mouseItem.maxStack - origStack);

                BindItem.stack = origStack + delta;

                Main.mouseItem.stack -= delta;
                if (Main.mouseItem.stack <= 0)
                    Main.mouseItem = new Item();
            }

            return;

        }
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
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        var position = GetDimensions().Center();

        int type = index switch
        {
            0 => ModContent.ItemType<BrokenDream>(),
            1 => ModContent.ItemType<WizardDream>(),
            2 => ModContent.ItemType<ZoologiseDream>(),
            3 => ModContent.ItemType<GolferDream>(),
            4 => ModContent.ItemType<PirateDream>(),
            5 => ModContent.ItemType<GuideDream>(),
            6 => ModContent.ItemType<TavernkeepDream>(),
            7 => ModContent.ItemType<ClothierDream>(),
            8 => ModContent.ItemType<PartyGirlDream>(),
            9 => ModContent.ItemType<DyeTraderDream>(),
            10 => ModContent.ItemType<CybrogDream>(),
            11 or _ => ModContent.ItemType<TaijiNoYume.TaijiNoYume>()
        };
        var iconItem = new Item(type) { stack = BindItem.stack };

        ItemSlot.DrawItemIcon(iconItem, 0, spriteBatch, position, 1f, 40, Color.White);
        if (BindItem.stack > 1)
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, BindItem.stack.ToString(), position, Color.White, 0, default, Vector2.One * .75f);
        if (IsMouseHovering)
        {
            string content = iconItem.HoverName;
            var m = iconItem.ToolTip.Lines;
            for (int n = 0; n < m; n++)
                content += "\n" + iconItem.ToolTip.GetLine(n);
            UICommon.TooltipMouseText(content);
        }
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

        SoundEngine.PlaySound(SoundID.MenuTick);
        base.LeftClick(evt);
    }
    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);
        var mplr = Main.LocalPlayer.GetModPlayer<DreamPlayer>();
        var position = GetDimensions().Center();
        int type = 0;
        if (DreamWorld.DreamTypeByState.TryGetValue(targetState, out var t))
            type = t;
        var iconItem = new Item(type) { stack = mplr.CheckUnlock(targetState) ? 1 : 0};

        ItemSlot.DrawItemIcon(iconItem, 0, spriteBatch, position, 1f, 40, Color.White);

        if (mplr.CheckActive(targetState))
        {
            spriteBatch.Draw(TextureAssets.Extra[98].Value, position, null, Color.White with { A = 0 } * .75f, Main.GlobalTimeWrappedHourly, new Vector2(36), new Vector2(1, .25f), 0, 0);
            spriteBatch.Draw(TextureAssets.Extra[98].Value, position, null, Color.White with { A = 0 } * .25f, Main.GlobalTimeWrappedHourly * -2f, new Vector2(36), new Vector2(2, .5f), 0, 0);
        }

        if (IsMouseHovering)
        {
            string content = iconItem.HoverName;
            var m = iconItem.ToolTip.Lines;
            for (int n = 0; n < m; n++)
                content += "\n" + iconItem.ToolTip.GetLine(n);

            content += "\n" + Language.GetTextValue($"Mods.MatterRecord.Items.TheInterpretationOfDreams.{(mplr.CheckUnlock(targetState) ? "Hint" : "Locked")}");

            UICommon.TooltipMouseText(content);
        }
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
