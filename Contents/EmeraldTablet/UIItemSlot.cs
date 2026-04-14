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
        _item = new Item();
        _itemSlotContext = context;
        _scale = scale;
        Width.Set(52 * scale, 0);
        Height.Set(52 * scale, 0);
        _backgroundTexture = TextureAssets.InventoryBack9.Value;
    }

    public Item Item => _item;

    public override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (IsMouseHovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            var item = _item;
            ItemSlot.OverrideHover(ref _item, _itemSlotContext);
            ItemSlot.LeftClick(ref _item, _itemSlotContext);
            ItemSlot.RightClick(ref _item, _itemSlotContext);
            ItemSlot.MouseHover(ref _item, _itemSlotContext);
            if (item != _item)
                OnItemChanged?.Invoke(_item);
        }
        var dimension = GetDimensions();
        var s = Main.inventoryScale;
        Main.inventoryScale = 0.8f;
        Vector2 position = dimension.Center() + new Vector2(52f, 52f) * -0.5f * Main.inventoryScale;
        spriteBatch.Draw(_backgroundTexture, dimension.Center(), null, Color.White, 0, new Vector2(26), _scale, 0, 0);
        ItemSlot.Draw(spriteBatch, ref _item, 14, position);
        Main.inventoryScale = s;
    }
}