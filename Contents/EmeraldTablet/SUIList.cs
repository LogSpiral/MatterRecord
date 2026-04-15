using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.UI;
namespace MatterRecord.Contents.EmeraldTablet;

public class SUIList : UIElement
{
    /// <summary>
    /// 容器，装着所有子UI
    /// </summary>
    public UIElement Container { get; private set; }


    /// <summary>
    /// 蒙版，提供显示的那部分
    /// </summary>
    public UIElement Mask { get; private set; }


    private List<UIElement> Contents { get; } = [];

    public float ListPadding { get; set; }

    private float TargetScrollWheelValue;
    private float CurrentScrollWheelValue;
    private float _InnerTotalHeight;

    public SUIList()
    {
        OverflowHidden = true;
        PaddingBottom = PaddingLeft = PaddingRight = PaddingTop = 4;
        Mask = new()
        {
            Width = new(-8, 1),
            Height = new(0, 1),
            OverflowHidden = true
        };
        Append(Mask);

        Container = new()
        {
            Width = new(114514, 1),
            Height = new(114514, 0),
            MinHeight = new(114514, 0)
        };
        Mask.Append(Container);
    }
    public void Add(UIElement element)
    {
        Contents.Add(element);
        UpdateContainer();
    }
    public void Clear()
    {
        Contents.Clear();
        UpdateContainer();
    }

    private void UpdateContainer()
    {
        Container.RemoveAllChildren();
        float top = 0;
        foreach (var element in Contents)
        {
            Container.Append(element);
            element.Recalculate();
            element.Top = new(top, 0);
            var dimension = element.GetOuterDimensions();
            top += element.Height.Pixels + ListPadding;
        }
        _InnerTotalHeight = top;
        Recalculate();
    }

    public override void ScrollWheel(UIScrollWheelEvent evt)
    {
        var rest = _InnerTotalHeight - GetInnerDimensions().Height;
        if (rest < 0) return;
        TargetScrollWheelValue -= evt.ScrollWheelValue;

        base.ScrollWheel(evt);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        var prev = CurrentScrollWheelValue;
        var rest = _InnerTotalHeight - GetInnerDimensions().Height;
        if (IsMouseHovering)
            PlayerInput.LockVanillaMouseScroll("SUIList");
        if (rest < 0) return;
        if (TargetScrollWheelValue < 0)
            TargetScrollWheelValue = MathHelper.Lerp(TargetScrollWheelValue, 0, 0.5f);
        if (TargetScrollWheelValue > rest)
            TargetScrollWheelValue = MathHelper.Lerp(TargetScrollWheelValue, rest, 0.5f);
        CurrentScrollWheelValue = MathHelper.Lerp(CurrentScrollWheelValue, TargetScrollWheelValue, 0.1f);
        if (Math.Abs(prev - CurrentScrollWheelValue) > 0.2f)
        {
            Container.Top = new(-CurrentScrollWheelValue, 0);
            Recalculate();
        }

    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        base.DrawSelf(spriteBatch);

        var dimension = GetDimensions();
        var rect = dimension.ToRectangle();
        float fullHeight = rect.Height - 8;
        var rest = _InnerTotalHeight - GetInnerDimensions().Height;

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(rect.X + rect.Width - 8, rect.Y + 4, 4, (int)fullHeight), Color.Black * .5f);

        var factor = GetInnerDimensions().Height / _InnerTotalHeight;
        if (factor > 1) factor = 1;

        float sliderHeight = (rect.Height - 8) * factor;
        float sliderTop = rect.Y + 4 + (fullHeight - sliderHeight) * (CurrentScrollWheelValue / (float)rest);
        var sliderDest = new Rectangle(rect.X + rect.Width - 8, (int)sliderTop, 4, (int)sliderHeight);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, sliderDest, Color.White * .75f);


        if (sliderDest.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeft && !_draggingSlider)
        {
            _draggingSlider = true;
            _startPoint = Main.MouseScreen.Y - sliderDest.Y;
        }
        if (Main.mouseLeftRelease && _draggingSlider)
            _draggingSlider = false;
        if (_draggingSlider)
        {
            float p = Main.MouseScreen.Y - _startPoint - (rect.Y + 4);
            if (fullHeight != sliderHeight) 
            {
                TargetScrollWheelValue = rest * MathHelper.Clamp(p / (fullHeight - sliderHeight), 0, 1);
                CurrentScrollWheelValue = MathHelper.Lerp(CurrentScrollWheelValue, TargetScrollWheelValue, 0.3f);
            }
        }
    }

    private bool _draggingSlider;
    private float _startPoint;
}
