using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;

namespace MatterRecord.Contents.EmeraldTablet
{
    public class UIItemSlot : UIElement
    {
        private Item _item;
        private Texture2D _backgroundTexture;
        private float _scale;
        public Action<Item> OnItemChanged;

        public UIItemSlot(int context, float scale = 1f)
        {
            _item = new Item();
            _scale = scale;
            Width.Set(52 * scale, 0);
            Height.Set(52 * scale, 0);
            _backgroundTexture = TextureAssets.InventoryBack9.Value;
        }

        public Item Item => _item;

        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dimensions = GetDimensions();
            var pos = dimensions.Position();

            spriteBatch.Draw(_backgroundTexture, pos, null, Color.White, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);

            if (!_item.IsAir)
            {
                Main.instance.LoadItem(_item.type);
                var itemTexture = TextureAssets.Item[_item.type].Value;

                Rectangle frame;
                if (Main.itemAnimations[_item.type] != null)
                    frame = Main.itemAnimations[_item.type].GetFrame(itemTexture);
                else
                    frame = itemTexture.Bounds;

                float slotSize = 52 * _scale;
                float maxDrawSize = slotSize - 8;
                float scale = Math.Min(maxDrawSize / frame.Width, maxDrawSize / frame.Height);
                int drawWidth = (int)(frame.Width * scale);
                int drawHeight = (int)(frame.Height * scale);

                float iconX = pos.X + (slotSize - drawWidth) / 2f;
                float iconY = pos.Y + (slotSize - drawHeight) / 2f;
                var destRect = new Rectangle((int)iconX, (int)iconY, drawWidth, drawHeight);

                spriteBatch.Draw(itemTexture, destRect, frame, Color.White);

                if (_item.stack > 1)
                {
                    string stackText = _item.stack.ToString();
                    var font = FontAssets.ItemStack.Value;
                    Vector2 textSize = font.MeasureString(stackText);
                    Vector2 textPos = pos + new Vector2(10f * _scale, slotSize - textSize.Y - -10f * _scale);
                    spriteBatch.DrawString(font, stackText, textPos, Color.White, 0, Vector2.Zero, _scale, SpriteEffects.None, 0);
                }
            }

            if (IsMouseHovering)
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.HoverItem = _item.Clone();
                Main.mouseText = true;

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Item mouseItem = Main.mouseItem;
                    bool changed = false;

                    if (mouseItem.IsAir && !_item.IsAir)
                    {
                        Main.mouseItem = _item.Clone();
                        _item.TurnToAir();
                        changed = true;
                    }
                    else if (!mouseItem.IsAir && _item.IsAir)
                    {
                        _item = mouseItem.Clone();
                        Main.mouseItem.TurnToAir();
                        changed = true;
                    }
                    else if (!mouseItem.IsAir && !_item.IsAir && mouseItem.type == _item.type && _item.stack < _item.maxStack)
                    {
                        int space = _item.maxStack - _item.stack;
                        int transfer = Math.Min(space, mouseItem.stack);
                        _item.stack += transfer;
                        mouseItem.stack -= transfer;
                        if (mouseItem.stack <= 0)
                            mouseItem.TurnToAir();
                        changed = true;
                    }
                    else if (!mouseItem.IsAir && !_item.IsAir)
                    {
                        Item temp = _item.Clone();
                        _item = mouseItem.Clone();
                        Main.mouseItem = temp;
                        changed = true;
                    }

                    if (changed)
                        OnItemChanged?.Invoke(_item);
                }
            }
        }
    }
}