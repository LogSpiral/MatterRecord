using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.UI;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFilesSystem : ModSystem
{
    #region Properties

    public static float Progress { get; set; }

    public static float BarValue { get; set; }

    public static float AmmoValue { get; set; }

    #endregion Properties

    #region Assets

    public static Asset<Texture2D> ElementPanelBorder => ModAsset.Element_Panel_Right_Border;
    public static Asset<Texture2D> ElementPanelContent => ModAsset.Element_Panel_Right_Content;
    public static Asset<Texture2D> ElementPanelMiddleBorder => ModAsset.Element_Panel_Middle_Border;
    public static Asset<Texture2D> ElementPanelMiddleGround => ModAsset.Element_Panel_Middle_Ground;
    public static Asset<Texture2D> ElementPanelFill => ModAsset.Element_Fill;
    public static Asset<Texture2D> ElementPanelEnd => ModAsset.Panel_Left;

    public static Asset<Texture2D> ElementPanelMiddleBorderLong => ModAsset.Element_Panel_Middle_Border_Long;
    public static Asset<Texture2D> ElementPanelMiddleGroundLong => ModAsset.Element_Panel_Middle_Ground_Long;
    public static Asset<Texture2D> ElementPanelFillLong => ModAsset.Element_Fill_Long;

    #endregion Assets

    #region Drawing

    public static void DrawChargeBar_Internal(CalculatedStyle destination, float elementBarProgress, float elementBarValue)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        Vector2 topLeft = destination.Position();
        topLeft.X += 44;
        var flag = true;
        Color color = Color.Green;
        float factor1 = MathHelper.SmoothStep(0, 1, 2 * elementBarProgress);
        float factor2 = MathHelper.SmoothStep(0, 1, elementBarProgress * 2 - 1);

        #region Bar

        var offsetUnit = new Vector2(0, 12 * factor2);
        var offset = new Vector2(-30, 40);
        if (flag)
            for (int n = 0; n < 20; n++)
                spriteBatch.Draw(ElementPanelMiddleGround.Value, topLeft + offset + offsetUnit * n + new Vector2(24, -12), new Rectangle(0, 0, (int)(12f * factor2 + 1), 24), color * factor1 * factor1, MathHelper.PiOver2, default, 1f, 0, 0);
        int count = (int)Math.Ceiling(elementBarValue * 20);//向上取整
        float extraValue = elementBarValue * 20 + 1 - count;
        if (flag)
            for (int n = 0; n < count; n++)
            {
                var _color = Color.Lerp(color, Color.White, 0.25f - 0.25f * MathF.Cos(elementBarValue * Main.GlobalTimeWrappedHourly * 8));
                spriteBatch.Draw(ElementPanelFill.Value, topLeft + offset + new Vector2(18, -12) + offsetUnit * n, new Rectangle(0, 0, (int)((n == count - 1 ? extraValue : 1) * 12f * factor2 + 1), 12), _color * factor1, MathHelper.PiOver2, default, 1f, 0, 0);
            }
        for (int n = 0; n < 20; n++)
            spriteBatch.Draw(ElementPanelMiddleBorder.Value, topLeft + offset + offsetUnit * n + new Vector2(24, -12), new Rectangle(0, 0, (int)(12f * factor2 + 1), 24), Color.White * factor1 * factor1, MathHelper.PiOver2, default, 1f, 0, 0);

        #endregion Bar

        #region icon

        spriteBatch.Draw(ElementPanelEnd.Value, topLeft + offset + offsetUnit * 20 - new Vector2(0, 6), null, Color.White * factor1, -MathHelper.PiOver2, default, 1f, 0, 0);

        if (flag)
        {
            spriteBatch.Draw(ElementPanelContent.Value, topLeft, null, color * factor1, MathHelper.PiOver2, default, 1f, SpriteEffects.FlipHorizontally, 0);
            for (int n = 0; n < 3; n++)
                spriteBatch.Draw(ElementPanelContent.Value, topLeft + Main.rand.NextVector2Unit() * elementBarValue * 4, null, Color.White with { A = 0 } * .25f * factor1 * elementBarValue, MathHelper.PiOver2, default, 1f, SpriteEffects.FlipHorizontally, 0);
        }
        spriteBatch.Draw(ElementPanelBorder.Value, topLeft, null, Color.White * factor1, MathHelper.PiOver2, default, 1f, SpriteEffects.FlipHorizontally, 0);

        #endregion icon

        if (destination.ToRectangle().Contains(Main.MouseScreen.ToPoint()))
            Main.instance.MouseText($"{(int)(elementBarValue * 200)}");
    }

    public static void DrawAmmoBar_Internal(CalculatedStyle destination, float elementBarProgress, float elementBarValue)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;

        Vector2 topLeft = destination.Position();
        topLeft.X += 44;
        var flag = true;
        Color color = Color.Green;
        float factor1 = MathHelper.SmoothStep(0, 1, 2 * elementBarProgress);
        float factor2 = MathHelper.SmoothStep(0, 1, elementBarProgress * 2 - 1);

        #region Bar

        var offsetUnit = new Vector2(0, 40 * factor2);
        var offset = new Vector2(-30, 40);
        if (flag)
            for (int n = 0; n < 6; n++)
                spriteBatch.Draw(ElementPanelMiddleGroundLong.Value, topLeft + offset + offsetUnit * n + new Vector2(24, -12), new Rectangle(0, 0, (int)(40f * factor2 + 1), 24), color * factor1 * factor1, MathHelper.PiOver2, default, 1f, 0, 0);
        int count = (int)Math.Ceiling(elementBarValue * 6);//向上取整
        float extraValue = elementBarValue * 6 + 1 - count;
        if (flag)
            for (int n = 0; n < count; n++)
            {
                var _color = Color.Lerp(color, Color.White, 0.25f - 0.25f * MathF.Cos(elementBarValue * Main.GlobalTimeWrappedHourly * 8));
                spriteBatch.Draw(ElementPanelFillLong.Value, topLeft + offset + new Vector2(18, -12) + offsetUnit * n, new Rectangle(0, 0, (int)((n == count - 1 ? extraValue : 1) * 40f * factor2 + 1), 12), _color * factor1, MathHelper.PiOver2, default, 1f, 0, 0);
            }
        for (int n = 0; n < 6; n++)
            spriteBatch.Draw(ElementPanelMiddleBorderLong.Value, topLeft + offset + offsetUnit * n + new Vector2(24, -12), new Rectangle(0, 0, (int)(40f * factor2 + 1), 24), Color.White * factor1 * factor1, MathHelper.PiOver2, default, 1f, 0, 0);

        #endregion Bar

        #region icon

        spriteBatch.Draw(ElementPanelEnd.Value, topLeft + offset + offsetUnit * 6 - new Vector2(0, 6), null, Color.White * factor1, -MathHelper.PiOver2, default, 1f, 0, 0);

        if (flag)
        {
            spriteBatch.Draw(ElementPanelContent.Value, topLeft, null, color * factor1, MathHelper.PiOver2, default, 1f, SpriteEffects.FlipHorizontally, 0);
            for (int n = 0; n < 3; n++)
                spriteBatch.Draw(ElementPanelContent.Value, topLeft + Main.rand.NextVector2Unit() * elementBarValue * 4, null, Color.White with { A = 0 } * .25f * factor1 * elementBarValue, MathHelper.PiOver2, default, 1f, SpriteEffects.FlipHorizontally, 0);
        }
        spriteBatch.Draw(ElementPanelBorder.Value, topLeft, null, Color.White * factor1, MathHelper.PiOver2, default, 1f, SpriteEffects.FlipHorizontally, 0);

        #endregion icon

        // if (destination.ToRectangle().Contains(Main.MouseScreen.ToPoint()))
        //     Main.instance.MouseText($"{(int)(6 * (elementBarValue + 0.01f))}/6");
    }

    public static bool DrawChargeBar()
    {
        if (Progress <= 0) return true;

        DrawChargeBar_Internal(new(Main.screenWidth - 360 + OffsetValue.X, 80 + OffsetValue.Y, 40, 280), Progress, BarValue);
        DrawAmmoBar_Internal(new(Main.screenWidth - 400 + OffsetValue.X, 80 + OffsetValue.Y, 40, 280), Progress, AmmoValue);

        var dimension = new Rectangle(Main.screenWidth - 400 + (int)OffsetValue.X, 80 + (int)OffsetValue.Y, 80, 280);

        bool flag = dimension.Contains(Main.MouseScreen.ToPoint());
        if (flag)
            Main.LocalPlayer.mouseInterface = true;
        if (!IsDragging && Main.mouseLeft && flag)
        {
            IsDragging = true;
            PendingOffsetValue = OffsetValue;
            PendingOffsetValue = Main.MouseScreen - PendingOffsetValue;
        }
        if (IsDragging)
        {
            if (Main.mouseLeft && flag)
            {
                OffsetValue = Main.MouseScreen - PendingOffsetValue;
            }
            else
            {
                IsDragging = false;
                SetOffsetValue(Main.MouseScreen - PendingOffsetValue);
            }
        }
        return true;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int inventoryIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
        if (inventoryIndex != -1)
            layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer($"{nameof(MatterRecord)}: EnergyUI", DrawChargeBar, InterfaceScaleType.UI));

        base.ModifyInterfaceLayers(layers);
    }

    #endregion Drawing

    #region Update

    public override void PostUpdatePlayers()
    {
        Player player = Main.LocalPlayer;

        float progressTarget = 0;
        if (player.HeldItem.ModItem is LordOfTheFlies)
        {
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            var target = mplr.ChargingEnergy / 120f;
            var result = MathHelper.Lerp(BarValue, target, 0.1f);
            if (result < 0.0001f) result = 0;
            if (result > 0.9999f) result = 1;
            BarValue = result;
            target = mplr.StoredAmmoCount / 6f;
            result = MathHelper.Lerp(AmmoValue, target, 0.1f);
            if (result < 0.0001f) result = 0;
            if (result > 0.9999f) result = 1;
            AmmoValue = result;

            progressTarget = 1;
        }

        Progress = MathHelper.Lerp(Progress, progressTarget, 0.1f);
        if (Progress < 0.01f) Progress = 0;
        if (Progress > 0.99f) Progress = 1;

        base.PostUpdatePlayers();
    }

    #endregion Update

    private static Vector2 PendingOffsetValue { get; set; }
    private static Vector2 OffsetValue { get; set; }

    private static string MainPath { get; set; }

    private static bool IsDragging { get; set; }

    public static void SetOffsetValue(Vector2 newOffset)
    {
        OffsetValue = newOffset;
        File.WriteAllText(MainPath, $"{OffsetValue.X},{OffsetValue.Y}");
    }

    public override void Load()
    {
        MainPath = Path.Combine(Main.SavePath, "Mods", nameof(MatterRecord));
        Directory.CreateDirectory(MainPath);
        MainPath = Path.Combine(MainPath, "LordOfTieFilesUIOffset.txt");
        if (File.Exists(MainPath))
        {
            var line = File.ReadAllLines(MainPath);
            if (line.Length > 0)
            {
                var content = line[0];
                var datas = content.Split(",");
                if (float.TryParse(datas[0], out float x) && float.TryParse(datas[1], out float y))
                    OffsetValue = new(x, y);
            }
        }
        base.Load();
    }
}