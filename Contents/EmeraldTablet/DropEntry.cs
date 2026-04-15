using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace MatterRecord.Contents.EmeraldTablet;

public class DropEntry : UIPanel
{
    private readonly DropInfo _drop;
    private readonly Rectangle _frame;
    private readonly string _itemName;
    private const int IconSize = 32;
    private readonly int _factor;
    private readonly int? _bannersPerItem;
    private readonly int? _itemsPerBanner;
    private readonly Action<DropInfo, int, int> _onExchange;
    private bool _canExchange;
    private readonly bool _forceUnavailable;
    private readonly bool _isBagMode;
    private readonly double _bagTotalRate;
    private Item _dummyItem;

    public DropEntry(DropInfo drop, bool isBagMode, int factor, bool forceUnavailable, double bagTotalRate, Action<DropInfo, int, int> onExchange)
    {
        _drop = drop;
        _isBagMode = isBagMode;
        _factor = factor;
        _forceUnavailable = forceUnavailable;
        _bagTotalRate = bagTotalRate;
        _onExchange = onExchange;
        Width.Set(0, 1);
        Height.Set(36, 0);
        BackgroundColor = new Color(40, 50, 70) * .5f;
        BorderColor = Color.Transparent;
        SetPadding(4);

        _dummyItem = ContentSamples.ItemsByType[drop.ItemID];
        _itemName = Lang.GetItemNameValue(drop.ItemID);

        if (!_forceUnavailable)
        {
            if (_isBagMode)
            {
                int maxStack = ContentSamples.ItemsByType[drop.ItemID].maxStack;
                if (maxStack > 1)
                {
                    int max = drop.StackMax;
                    if (max <= 0) max = 1;
                    _itemsPerBanner = max;
                }
                else
                {
                    double avg = (drop.StackMin + drop.StackMax) / 2.0;
                    double E = _bagTotalRate * drop.DropRate * avg;
                    if (E >= 1.0)
                        _itemsPerBanner = 1;
                    else if (E > 0.0)
                        _bannersPerItem = (int)Math.Ceiling(1.0 / E);
                }
            }
            else
            {
                double avg = (drop.StackMin + drop.StackMax) / 2.0;
                double E = _factor * drop.DropRate * avg;
                int maxStack = ContentSamples.ItemsByType[drop.ItemID].maxStack;
                if (maxStack == 1)
                {
                    if (E >= 1.0)
                        _itemsPerBanner = 1;
                    else if (E > 0.0)
                        _bannersPerItem = (int)Math.Ceiling(1.0 / E);
                }
                else
                {
                    if (E >= 1.0)
                        _itemsPerBanner = (int)Math.Floor(E);
                    else if (E > 0.0)
                        _bannersPerItem = (int)Math.Ceiling(1.0 / E);
                }
            }
        }
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        if (_canExchange)
        {
            int requiredBanners, giveStack;
            if (_bannersPerItem.HasValue)
            {
                requiredBanners = _bannersPerItem.Value;
                giveStack = 1;
            }
            else if (_itemsPerBanner.HasValue)
            {
                requiredBanners = 1;
                giveStack = _itemsPerBanner.Value;
            }
            else
                return;
            _onExchange?.Invoke(_drop, requiredBanners, giveStack);
        }
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (_forceUnavailable)
        {
            _canExchange = false;
            return;
        }

        var bannerItem = EmeraldUI.Instance?.CurrentBannerItem;
        if (_bannersPerItem.HasValue)
            _canExchange = bannerItem != null && !bannerItem.IsAir && bannerItem.stack >= _bannersPerItem.Value;
        else if (_itemsPerBanner.HasValue)
            _canExchange = bannerItem != null && !bannerItem.IsAir && bannerItem.stack >= 1;
        else
            _canExchange = false;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (_canExchange)
            BackgroundColor = new Color(60, 80, 100) * .75f;
        else
            BackgroundColor = new Color(40, 50, 70) * .75f;

        base.DrawSelf(spriteBatch);

        var dimensions = GetDimensions();
        var panelPos = dimensions.Position();
        var hitbox = dimensions.ToRectangle();
        float panelRight = panelPos.X + dimensions.Width - 4;

        float scale = Math.Min(IconSize / (float)_frame.Width, IconSize / (float)_frame.Height);
        int drawWidth = (int)(_frame.Width * scale);
        int drawHeight = (int)(_frame.Height * scale);
        float iconX = panelPos.X + 4;
        float iconY = panelPos.Y + (Height.Pixels - drawHeight) / 2f;
        var destRect = new Rectangle((int)iconX, (int)iconY, drawWidth, drawHeight);
        // spriteBatch.Draw(_cachedTexture, destRect, _frame, Color.White);
        var s = Main.inventoryScale;
        Main.inventoryScale = 0.8f;
        ItemSlot.Draw(spriteBatch, ref _dummyItem, 14, panelPos + new Vector2(dimensions.Height * .5f - 26 * 0.8f) + new Vector2(4, 0));
        Main.inventoryScale = s;
        var font = FontAssets.MouseText.Value;
        float textScale = 0.85f;
        string rateText = _forceUnavailable ? "0.00%" : $"{_drop.DropRate:P2}";

        string unit = _forceUnavailable ? "" : (_isBagMode ? EmeraldUI.BagUnit.Value : EmeraldUI.BannerUnit.Value);
        string rightText = "";
        if (!_forceUnavailable)
        {
            if (_bannersPerItem.HasValue)
            {
                rightText = string.Format(EmeraldUI.FormatBannersPerItem.Value, _bannersPerItem.Value, unit);
            }
            else if (_itemsPerBanner.HasValue)
            {
                rightText = string.Format(EmeraldUI.FormatItemsPerBanner.Value, _itemsPerBanner.Value, unit);
            }
        }
        string availableText = _drop.IsAvailable ? "✓" : "✗";
        if (_forceUnavailable) availableText = "✗";

        float textHeight = font.MeasureString("A").Y * textScale;
        float rightTextY = panelPos.Y + (Height.Pixels - textHeight) / 2f + 6f;

        Vector2 nameSize = font.MeasureString(_itemName) * textScale;
        Vector2 rateSize = font.MeasureString(rateText) * textScale;
        Vector2 rightSize = font.MeasureString(rightText) * textScale;
        Vector2 availSize = font.MeasureString(availableText) * textScale;

        float rightGroupWidth = rightSize.X + 4 + availSize.X;
        float rightGroupStartX = panelRight - rightGroupWidth - 4;

        float totalHeight = nameSize.Y + rateSize.Y + 4;
        float textBlockStartY = panelPos.Y + (Height.Pixels - totalHeight) / 2f;
        float leftX = iconX + IconSize + 8;

        spriteBatch.DrawString(font, _itemName, new Vector2(leftX, textBlockStartY + 10f), Color.White, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
        spriteBatch.DrawString(font, rateText, new Vector2(leftX, textBlockStartY + nameSize.Y + 2), Color.LightGoldenrodYellow, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);

        if (!string.IsNullOrEmpty(rightText))
        {
            spriteBatch.DrawString(font, rightText, new Vector2(rightGroupStartX, rightTextY), Color.Aquamarine, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
            spriteBatch.DrawString(font, availableText, new Vector2(rightGroupStartX + rightSize.X + 4, rightTextY),
                _drop.IsAvailable && !_forceUnavailable ? Color.LightGreen : Color.OrangeRed, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
        }
        else
        {
            spriteBatch.DrawString(font, availableText, new Vector2(rightGroupStartX, rightTextY),
                _drop.IsAvailable && !_forceUnavailable ? Color.LightGreen : Color.OrangeRed, 0, Vector2.Zero, textScale, SpriteEffects.None, 0);
        }

        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            Item hoverItem = new Item();
            hoverItem.SetDefaults(_drop.ItemID);
            hoverItem.stack = 1;
            Main.HoverItem = hoverItem;
            Main.hoverItemName = "1";
            Main.mouseText = true;
        }
    }
}