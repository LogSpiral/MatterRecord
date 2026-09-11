using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace MatterRecord.Contents.TheOldManAndTheSea
{
    public class TheOldManAndTheSeaSystem : ModSystem
    {
        private Asset<Texture2D> _iconTexture;

        public override void Load()
        {
            _iconTexture = ModContent.Request<Texture2D>("MatterRecord/Contents/TheOldManAndTheSea/FishEnergy");
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Inventory");
            if (index != -1)
            {
                layers.Insert(index + 1, new LegacyGameInterfaceLayer(
                    "MatterRecord: TheOldManAndTheSeaEnergyUI",
                    DrawEnergyUI,
                    InterfaceScaleType.UI
                ));
            }
        }

        private bool DrawEnergyUI()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active) return true;

            var mp = player.GetModPlayer<TheOldManAndTheSeaPlayer>();
            if (mp == null) return true;

            // 只有手持老人与海鱼竿时才显示
            if (player.HeldItem == null || player.HeldItem.type != ModContent.ItemType<TheOldManAndTheSea>())
                return true;

            // 能量为0且未激活时半透明显示
            float alpha = mp.Energy > 0 ? 1f : 0.3f;
            if (mp.Energy == 0 && !mp.IsActivated) alpha = 0.3f;

            SpriteBatch sb = Main.spriteBatch;

            // ---- 位置计算：屏幕中央偏下 ----
            int iconSize = 32;
            int barWidth = 180;
            int barHeight = 18;
            int spacing = 8; // 图标与能量条间距
            int totalWidth = iconSize + spacing + barWidth; // 整体宽度

            // 水平居中，垂直从屏幕中心向下偏移 120 像素（可调整）
            Vector2 basePos = new Vector2(
                (Main.screenWidth - totalWidth) / 2,
                Main.screenHeight / 2 + 50
            );

            // 获取抖动偏移（激活时生效）
            Vector2 shake = mp.GetUIShakeOffset();
            Vector2 drawPos = basePos + shake;

            // ---- 绘制图标 ----
            Rectangle iconRect = new Rectangle(0, 0, iconSize, iconSize);
            Color iconColor = Color.White * alpha;
            sb.Draw(_iconTexture.Value, drawPos, iconRect, iconColor);

            // ---- 绘制能量条 ----
            Vector2 barPos = drawPos + new Vector2(iconSize + spacing, (iconSize - barHeight) / 2);
            Rectangle barBgRect = new Rectangle((int)barPos.X, (int)barPos.Y, barWidth, barHeight);

            // 背景
            sb.Draw(TextureAssets.MagicPixel.Value, barBgRect, new Rectangle(0, 0, 1, 1), Color.DarkGray * alpha);
            // 边框
            sb.Draw(TextureAssets.MagicPixel.Value, barBgRect, null, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            // 填充
            float fillRatio = mp.Energy / (float)TheOldManAndTheSeaPlayer.MaxEnergy;
            int fillWidth = (int)(barWidth * fillRatio);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle((int)barPos.X + 2, (int)barPos.Y + 2, fillWidth - 4, barHeight - 4);
                Color fillColor = mp.IsActivated ? Color.LimeGreen : Color.CornflowerBlue;
                fillColor *= alpha;
                sb.Draw(TextureAssets.MagicPixel.Value, fillRect, new Rectangle(0, 0, 1, 1), fillColor);
            }

            // ---- 显示能量数值 ----
            string text = $"{mp.Energy}/{TheOldManAndTheSeaPlayer.MaxEnergy}";
            Vector2 textPos = barPos + new Vector2(barWidth / 2, barHeight / 2) - FontAssets.MouseText.Value.MeasureString(text) / 2 + new Vector2(0, 3);
            sb.DrawString(FontAssets.MouseText.Value, text, textPos + new Vector2(1, 1), Color.Black * alpha);
            sb.DrawString(FontAssets.MouseText.Value, text, textPos, Color.White * alpha);

            return true;
        }
    }
}