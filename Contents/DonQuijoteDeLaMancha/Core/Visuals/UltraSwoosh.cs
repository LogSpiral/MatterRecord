using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;

public class UltraSwoosh : MeleeVertexInfo
{
    #region 参数和属性

    private int counts = 45;

    public int Counts
    {
        get { return counts = Math.Clamp(counts, 2, 45); }
        set
        {
            int c = counts;
            counts = Math.Clamp(value, 2, 45);
            if (c != counts)
            {
                Array.Resize(ref _vertexInfos, 2 * counts);
                if (autoUpdate)
                {
                    Update();
                    timeLeft++;
                }
            }
        }
    }

    private CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[90];
    public override CustomVertexInfo[] VertexInfos => _vertexInfos;
    public (float from, float to) angleRange;

    #endregion 参数和属性

    #region 生成函数

    public static UltraSwoosh NewUltraSwoosh(string canvasName, int timeLeft, float scaler, Vector2 center, (float, float) angleRange)
    {
        var content = new UltraSwoosh();

        content.timeLeft = content.timeLeftMax = timeLeft;
        content.scaler = scaler;
        content.center = center;
        content.angleRange = angleRange;
        content.aniTexIndex = 3;
        content.baseTexIndex = 7;
        RenderCanvasSystem.AddRenderDrawingContent(canvasName, content);

        return content;
    }

    public static UltraSwoosh NewUltraSwooshOnDefaultCanvas(int timeLeft, float scaler, Vector2 center, (float, float) angleRange)
        => NewUltraSwoosh(string.Empty, timeLeft, scaler, center, angleRange);

    #endregion 生成函数

    #region 绘制和更新，主体

    public override void Draw(SpriteBatch spriteBatch)
    {
        Main.graphics.GraphicsDevice.Textures[0] = ModAsset.BaseTex_15.Value;
        Main.graphics.GraphicsDevice.Textures[1] = ModAsset.AniTex_3.Value;
        base.Draw(spriteBatch);
    }

    public override void Update()
    {
        if (!autoUpdate)
        {
            autoUpdate = true;
            return;
        }
        timeLeft--;
        for (int i = 0; i < Counts; i++)
        {
            var f = i / (Counts - 1f);
            var num = 1 - Factor;
            //float theta2 = (1.8375f * MathHelper.Lerp(num, 1f, f) - 1.125f) * MathHelper.Pi;
            var lerp = MathHelper.Lerp(num * .5f, 1, f);//num
            float theta2 = MathHelper.Lerp(angleRange.from, angleRange.to, lerp) * MathHelper.Pi;
            if (negativeDir) theta2 = MathHelper.TwoPi - theta2;
            Vector2 offsetVec = (theta2.ToRotationVector2() * new Vector2(1, 1 / xScaler)).RotatedBy(rotation) * scaler;//  * MathHelper.Lerp(2 - 1 / (MathF.Abs(angleRange.from - angleRange.to) + 1), 1, f)
            Vector2 adder = (offsetVec * 0.05f + rotation.ToRotationVector2() * 2f) * num;
            //adder = default;
            var realColor = Color.White;//color.Invoke(f);
            realColor.A = (byte)((1 - (float)Math.Cos(MathHelper.TwoPi * MathF.Sqrt(1 - f))) * 0.5f * 255);
            realColor.A = 255;
            VertexInfos[2 * i] = new CustomVertexInfo(center + offsetVec + adder, realColor, new Vector3(1 - f, 1, 1));
            VertexInfos[2 * i + 1] = new CustomVertexInfo(center + adder, realColor, new Vector3(0, 0, 1));
        }
    }
    #endregion 绘制和更新，主体
}