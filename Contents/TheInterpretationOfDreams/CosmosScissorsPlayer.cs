using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class CosmosScissorsPlayer : ModPlayer
    {
        // —— 可调参数 ——
        private const float Threshold = 0.10f;
        private const float LineOverScreen = 200f;
        private const float LineWidth = 1.5f;
        private static readonly Color FateLineColor = new Color(20, 5, 30) * 0.9f;

        // —— 波动参数 ——
        private const int WaveSegments = 14;
        private const float WaveAmplitude = 6f;
        private const float WaveSpeed = 2.4f;
        private const float WaveFrequency = 6f;

        // —— 缓存 ——
        private static readonly Dictionary<int, Color[]> TextureCache = new();
        private static readonly Dictionary<(int, int, int, int, int), int> TopRowCache = new();

        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            // ★ 只让本地玩家执行绘制
            if (Player.whoAmI != Main.myPlayer) return;

            Item held = Player.HeldItem;
            if (held == null || held.type != ModContent.ItemType<CosmosScissors>())
                return;

            DrawFateLines();
        }

        private void DrawFateLines()
        {
            Texture2D lineTex = TextureAssets.MagicPixel.Value;
            if (lineTex == null) return;

            float offscreenTopY = Main.screenPosition.Y - LineOverScreen;
            List<Vector2> points = new List<Vector2>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.life <= 0) continue;
                if (npc.dontTakeDamage) continue;
                if (npc.lifeMax <= 5) continue;

                float ratio = npc.life / (float)npc.lifeMax;
                if (ratio > Threshold) continue;

                points.Add(GetVisualTop(npc));
            }

            if (points.Count == 0) return;

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.PointWrap, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float time = Main.GlobalTimeWrappedHourly;

            foreach (Vector2 start in points)
            {
                Vector2 end = new Vector2(start.X, offscreenTopY);
                DrawWavyLine(lineTex, start, end, time);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                Main.DefaultSamplerState, DepthStencilState.None,
                RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private void DrawWavyLine(Texture2D tex, Vector2 start, Vector2 end, float time)
        {
            float phaseSeed = start.X * 0.04f;

            Vector2 prev = start;
            for (int i = 1; i <= WaveSegments; i++)
            {
                float t = i / (float)WaveSegments;
                Vector2 basePos = Vector2.Lerp(start, end, t);

                float envelope = (float)System.Math.Sin(t * System.Math.PI);
                float wobble = (float)System.Math.Sin(
                    time * WaveSpeed + t * WaveFrequency + phaseSeed
                ) * WaveAmplitude * envelope;

                Vector2 cur = new Vector2(basePos.X + wobble, basePos.Y);
                DrawLineSegment(Main.spriteBatch, tex, prev, cur, FateLineColor, LineWidth);
                prev = cur;
            }
        }

        private static Vector2 GetVisualTop(NPC npc)
        {
            Rectangle frame = npc.frame;
            if (frame.Width <= 0 || frame.Height <= 0)
                return npc.Top;

            Texture2D texture = TextureAssets.Npc[npc.type].Value;
            if (texture == null)
                return npc.Top;

            if (!TextureCache.TryGetValue(npc.type, out Color[] data) || data == null)
            {
                data = new Color[texture.Width * texture.Height];
                texture.GetData(data);
                TextureCache[npc.type] = data;
            }

            int texW = texture.Width;

            var cacheKey = (npc.type, frame.X, frame.Y, frame.Width, frame.Height);
            if (!TopRowCache.TryGetValue(cacheKey, out int topRow))
            {
                topRow = -1;
                for (int y = 0; y < frame.Height && topRow == -1; y++)
                {
                    int rowStart = (frame.Y + y) * texW + frame.X;
                    for (int x = 0; x < frame.Width; x++)
                    {
                        if (data[rowStart + x].A > 0)
                        {
                            topRow = y;
                            break;
                        }
                    }
                }
                TopRowCache[cacheKey] = topRow;
            }

            if (topRow < 0)
                return npc.Top;

            Vector2 frameTopLeftWorld = npc.Center
                + new Vector2(0f, npc.gfxOffY)
                - new Vector2(frame.Width / 2f, frame.Height / 2f);

            return new Vector2(
                frameTopLeftWorld.X + frame.Width / 2f,
                frameTopLeftWorld.Y + topRow
            );
        }

        // ---------- 顶点绘制细线 ----------
        private struct ColoredVertex : IVertexType
        {
            public Vector2 Position;
            public Color Color;
            public Vector2 TextureCoordinate;

            public ColoredVertex(Vector2 position, Color color, Vector2 texCoord)
            {
                Position = position;
                Color = color;
                TextureCoordinate = texCoord;
            }

            public VertexDeclaration VertexDeclaration => new VertexDeclaration(
                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
                new VertexElement(8, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0));
        }

        private void DrawLineSegment(SpriteBatch sb, Texture2D tex, Vector2 start, Vector2 end, Color color, float width)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 1f) return;

            Vector2 normal = new Vector2(-diff.Y, diff.X) / length;
            Vector2 half = normal * (width * 0.5f);

            Vector2[] corners = new Vector2[4];
            corners[0] = start - half;
            corners[1] = start + half;
            corners[2] = end - half;
            corners[3] = end + half;

            float texX = length / tex.Width;
            ColoredVertex[] verts = new ColoredVertex[6];
            verts[0] = new ColoredVertex(corners[0] - Main.screenPosition, color, new Vector2(0, 0));
            verts[1] = new ColoredVertex(corners[1] - Main.screenPosition, color, new Vector2(0, 1));
            verts[2] = new ColoredVertex(corners[2] - Main.screenPosition, color, new Vector2(texX, 0));
            verts[3] = new ColoredVertex(corners[1] - Main.screenPosition, color, new Vector2(0, 1));
            verts[4] = new ColoredVertex(corners[3] - Main.screenPosition, color, new Vector2(texX, 1));
            verts[5] = new ColoredVertex(corners[2] - Main.screenPosition, color, new Vector2(texX, 0));

            GraphicsDevice device = Main.graphics.GraphicsDevice;
            device.Textures[0] = tex;
            device.SamplerStates[0] = SamplerState.PointWrap;
            device.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 2);
        }
    }
}