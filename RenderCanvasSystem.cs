using MatterRecord;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MatterRecord;

public class RenderCanvasSystem : ModSystem
{
    #region 常量

    public const string DEFAULTCANVASNAME = "Default Canvas";

    #endregion 常量

    #region 字段/属性

    public static RenderingCanvas Canvas { get; } = new();

    public static Dictionary<Type, RenderDrawingContent> RenderDrawingContentInstance { get; } = [];
    #endregion 字段/属性

    #region 公开函数

    public static void AddRenderDrawingContent(string canvasName, VertexDrawInfo content)
    {
        if (Main.dedServ) return;
        Canvas.Add(content);
    }

    #endregion 公开函数

    #region 重写函数

    private static void UpdateCanvases()
    {
        Canvas.Update();
    }

    public override void PostUpdateEverything()
    {
        if (!Main.dedServ)
            UpdateCanvases();
    }

    public override void Load()
    {
        // 服务器端大黑框自然用不到这些
        if (Main.dedServ) return;
        // 挂起矩阵更新，下一次要用的时候就会先计算一下然后缓存着
        Main.OnPostDraw += delegate
        {
            _pendingUpdateViewMatrix = true;
            _pendingUpdateUIMatrix = true;
        };

        On_Main.DrawProjectiles += DrawCanvasHook;


        // Filters.Scene.OnPostDraw += () => { _pendingUpdateViewMatrix = true; }; // 这个仅在色彩或白光模式下有

        // 注册默认画布
        // 默认画布上不会有任何渲染特效
        // RegisterCanvasFactory(DEFAULTCANVASNAME, () => new RenderingCanvas());
        base.Load();
    }

    private static void DrawCanvasHook(On_Main.orig_DrawProjectiles orig, Main self)
    {
        orig?.Invoke(self);
        DrawCanvases(Main.spriteBatch, Main.graphics.GraphicsDevice);
    }

    #endregion 重写函数

    #region Matrix

    // 已替换为field关键字
    // static Matrix _uTransformCache;

    // static Matrix _uTransformUILayerCache;

    private static bool _pendingUpdateViewMatrix;

    private static bool _pendingUpdateUIMatrix;

    /// <summary>
    /// 控制是否计算ui层绘制矩阵
    /// </summary>
    private static bool uiDrawing;

    /// <summary>
    /// spbdraw那边的矩阵
    /// </summary>
    public static Matrix TransformationMatrix
    {
        get
        {
            if (Main.gameMenu)
            {
                return Matrix.CreateScale(Main.instance.Window.ClientBounds.Width / (float)Main.screenWidth, Main.instance.Window.ClientBounds.Height / (float)Main.screenHeight, 1);
            }
            else if (uiDrawing)
            {
                return Matrix.Identity;
            }
            return Main.GameViewMatrix?.TransformationMatrix ?? Matrix.Identity;
        }
    }

    private static Matrix Projection => Main.gameMenu ? Matrix.CreateOrthographicOffCenter(0, Main.instance.Window.ClientBounds.Width, Main.instance.Window.ClientBounds.Height, 0, 0, 1) : Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);//Main.screenWidth  Main.screenHeight
    private static Matrix Model => Matrix.CreateTranslation(uiDrawing ? default : new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));

    /// <summary>
    /// 丢给顶点坐标变换的矩阵
    /// <br>先右乘<see cref="Model"/>将世界坐标转屏幕坐标</br>
    /// <br>再右乘<see cref="TransformationMatrix"/>进行画面缩放等</br>
    /// <br>最后右乘<see cref="Projection"/>将坐标压缩至[0,1]</br>
    /// </summary>
    public static Matrix uTransform
    {
        get
        {
            if (_pendingUpdateViewMatrix)
            {
                uiDrawing = false;
                field = Model * TransformationMatrix * Projection;
                _pendingUpdateViewMatrix = false;
            }
            return field;
        }
    }

    /// <summary>
    /// UI层变换矩阵
    /// </summary>
    public static Matrix uTransformUILayer
    {
        get
        {
            if (_pendingUpdateUIMatrix)
            {
                uiDrawing = true;
                field = Model * TransformationMatrix * Projection;
                _pendingUpdateUIMatrix = false;
            }
            return field;
        }
    }

    #endregion Matrix

    private static void DrawCanvases(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        if (Canvas.RenderDrawingContents.Count > 0)
            Canvas.DrawContents(spriteBatch, graphicsDevice);
    }
}