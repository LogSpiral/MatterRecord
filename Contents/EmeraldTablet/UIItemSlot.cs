using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.UI;

namespace MatterRecord.Contents.EmeraldTablet;

public class UIItemSlot : UIElement
{
    private Item _item;
    private static Texture2D _backgroundTexture;
    private readonly float _scale;
    private readonly int _itemSlotContext;
    public Action<Item> OnItemChanged;

    public UIItemSlot(int context, float scale = 1f)
    {
        _item = new();
        _itemSlotContext = context;
        _scale = scale;
        Width.Set(52 * scale, 0);
        Height.Set(52 * scale, 0);
        _backgroundTexture = TextureAssets.InventoryBack9.Value;
    }

    public Item Item 
    {
        get => _item;
        set => _item = value;
    }

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            var item = _item;
            Item[] tempArray = [_item];
            ItemSlot.OverrideHover(tempArray, _itemSlotContext);
            ItemSlot.LeftClick(tempArray, _itemSlotContext);
            ItemSlot.RightClick(tempArray, _itemSlotContext);
            ItemSlot.MouseHover(tempArray, _itemSlotContext);
            if (item != _item)
                OnItemChanged?.Invoke(_item);
        }
        var dimension = GetDimensions();
        var s = Main.inventoryScale;
        Main.inventoryScale = 0.8f;
        Vector2 position = dimension.Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
        spriteBatch.Draw(_backgroundTexture, dimension.Center(), null, Color.White * .75f, 0, new Vector2(26), _scale, 0, 0);
        ItemSlot.Draw(spriteBatch, ref _item, 14, position);
        Main.inventoryScale = s;
    }
}