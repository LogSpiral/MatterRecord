using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;

public class UltraStab : MeleeVertexInfo
{
    private const bool usePSShaderTransform = true;

    #region 参数和属性

    private readonly CustomVertexInfo[] _vertexInfos = new CustomVertexInfo[usePSShaderTransform ? 4 : 90];
    public override CustomVertexInfo[] VertexInfos => _vertexInfos;

    #endregion 参数和属性

    #region 生成函数

    public static UltraStab NewUltraStab(string canvasName, int timeLeft, float scaler, Vector2 center)
    {
        var content = new UltraStab();
        content.timeLeft = content.timeLeftMax = timeLeft;
        content.scaler = scaler;
        content.center = center;
        content.aniTexIndex = 9;
        content.baseTexIndex = 0;
        RenderCanvasSystem.AddRenderDrawingContent(canvasName, content);

        return content;
    }

    public static UltraStab NewUltraStabOnDefaultCanvas(int timeLeft, float scaler, Vector2 center)
        => NewUltraStab(RenderCanvasSystem.DEFAULTCANVASNAME, timeLeft, scaler, center);

    #endregion 生成函数

    #region 绘制和更新，主体

    public override void PreDraw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        base.PreDraw(spriteBatch, graphicsDevice);
        ModAsset.ShaderSwooshEffectUL.Value.Parameters["stab"].SetValue(usePSShaderTransform);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Main.graphics.GraphicsDevice.Textures[0] = ModAsset.BaseTex_0.Value;
        Main.graphics.GraphicsDevice.Textures[1] = ModAsset.AniTex_9.Value;
        base.Draw(spriteBatch);
    }

    public override void Update()
    {
        if (!autoUpdate)
        {
            autoUpdate = true;
            return;
        }
        var realColor = Color.White;
        //Vector2 offsetVec = 20f * new Vector2(8, 3 / xScaler) * scaler;
        Vector2 offsetVec = new Vector2(1, 3 / xScaler / 8) * scaler;

        if (negativeDir) offsetVec.Y *= -1;
        VertexInfos[0] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 1, 1f));
        VertexInfos[2] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 1, 1f));
        offsetVec.Y *= -1;
        VertexInfos[1] = new CustomVertexInfo(center + offsetVec.RotatedBy(rotation), realColor, new Vector3(0, 0, 1f));
        VertexInfos[3] = new CustomVertexInfo(center + (offsetVec with { X = 0 }).RotatedBy(rotation), realColor, new Vector3(1, 0, 1f));

        for (int n = 0; n < 4; n++)
            VertexInfos[n].Position += rotation.ToRotationVector2();
        timeLeft--;
    }

    #endregion 绘制和更新，主体
}