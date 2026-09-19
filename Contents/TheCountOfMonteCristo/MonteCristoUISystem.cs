using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheCountOfMonteCristo
{
    public class MonteCristoUISystem : ModSystem
    {
        private static Texture2D _candleFrame1;   // Candle.png       第一帧
        private static Texture2D _candleFrame2;   // Candlelight2.png 第二帧
        private static Texture2D _lightTexture;   // Candlelight.png  10×10 火光

        /// <summary>蜡烛帧切换速度：每多少帧切一次（数值越小闪得越快）。</summary>
        private const int CandleFrameInterval = 8;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _candleFrame1 = ModContent.Request<Texture2D>("MatterRecord/Contents/TheCountOfMonteCristo/Candle", AssetRequestMode.ImmediateLoad).Value;
                _candleFrame2 = ModContent.Request<Texture2D>("MatterRecord/Contents/TheCountOfMonteCristo/Candle2", AssetRequestMode.ImmediateLoad).Value;
                _lightTexture = ModContent.Request<Texture2D>("MatterRecord/Contents/TheCountOfMonteCristo/Candlelight", AssetRequestMode.ImmediateLoad).Value;
            }
        }

        public override void Unload()
        {
            _candleFrame1 = null;
            _candleFrame2 = null;
            _lightTexture = null;
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            Player localPlayer = Main.LocalPlayer;
            if (localPlayer == null || !localPlayer.active) return;

            var modPlayer = localPlayer.GetModPlayer<MonteCristoPlayer>();
            if (modPlayer == null || !modPlayer.ShouldShowProgressBar) return;
            if (_candleFrame1 == null || _candleFrame2 == null || _lightTexture == null) return;

            int frames = modPlayer.CurrentDelayedFrames;
            float seconds = frames / 60f;
            float progress = MathHelper.Clamp(seconds / 10f, 0f, 1f);

            Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);

            // ---------- 绘制蜡烛 ----------
            // 两帧贴图尺寸均为 24 × 46，左上角为原点。
            // 火焰中心位于贴图局部坐标 (12, 18)：x=12 水平居中，y=18 从顶部往下 18 像素。
            // 用火焰中心作为锚点贴合鼠标：贴图左上角 = 鼠标位置 - 火焰中心偏移 * 缩放。
            float candleScale = 3.5f;
            Vector2 flameCenterInTexture = new Vector2(12f, 13f);
            Vector2 candleDrawPos = mousePos - flameCenterInTexture * candleScale;

            // 火焰中心初始落在鼠标位置
            Vector2 flameCenter = mousePos;

            // ---------- 摇曳（火焰轻微晃动） ----------
            float t = Main.GlobalTimeWrappedHourly;
            float swayX = (float)Math.Sin(t * 5.3f) * 1.2f + (float)Math.Sin(t * 11.7f) * 0.6f;
            float swayY = (float)Math.Sin(t * 4.1f) * 0.8f + (float)Math.Sin(t * 9.3f) * 0.4f;
            flameCenter += new Vector2(swayX, swayY);

            // ---------- 脉动（呼吸感） ----------
            float pulse = 1f + (float)Math.Sin(t * 6.5f) * 0.08f + (float)Math.Sin(t * 13f) * 0.02f;

            // ---------- 蜡烛两帧动画 ----------
            // 用游戏更新计数按固定间隔切换两帧，模拟烛火/蜡烛的闪动。
            // 若想让帧率跟随蓄力变化（越接近 10 秒闪得越快），把 CandleFrameInterval
            // 换成动态计算即可，这里保持恒定。
            bool useSecondFrame = (Main.GameUpdateCount / CandleFrameInterval) % 2 == 1;
            Texture2D candleFrame = useSecondFrame ? _candleFrame2 : _candleFrame1;

            spriteBatch.Draw(candleFrame, candleDrawPos, null, Color.White, 0f, Vector2.Zero, candleScale, SpriteEffects.None, 0f);

            // ---------- 绘制光芒（Additive 混合） ----------
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.Additive,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.UIScaleMatrix);

            // 火光统一缩放倍率：大于 1 放大，小于 1 缩小。
            const float LightScaleMultiplier = 4.5f;

            // 基础尺寸随蓄力 0→10 秒线性增长，并叠加脉动
            float baseScale = MathHelper.Lerp(1.0f, 2.0f, progress) * pulse * LightScaleMultiplier;

            // 分四层绘制，由外到内、由暗到亮
            DrawLight(spriteBatch, flameCenter, baseScale * 2.2f, new Color(255, 140, 40) * 0.10f); // 最外层
            DrawLight(spriteBatch, flameCenter, baseScale * 1.5f, new Color(255, 170, 70) * 0.18f); // 中层
            DrawLight(spriteBatch, flameCenter, baseScale * 1.0f, new Color(255, 210, 120) * 0.35f); // 内层
            DrawLight(spriteBatch, flameCenter, baseScale * 0.55f, new Color(255, 245, 200) * 0.60f); // 核心

            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.LinearClamp,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.UIScaleMatrix);
        }

        /// <summary>
        /// 以纹理中心为锚点绘制一层光芒（火光贴图 10 × 10，原点自动为 (5, 5)）。
        /// </summary>
        private static void DrawLight(SpriteBatch sb, Vector2 position, float scale, Color color)
        {
            Vector2 origin = new Vector2(_lightTexture.Width / 2f, _lightTexture.Height / 2f);
            sb.Draw(_lightTexture, position, null, color, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }
}