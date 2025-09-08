using MatterRecord;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatterRecord;

public sealed class RenderingCanvas // 应该没有继承的必要所以就直接sealed了
{
    #region 常量

    private const int TIMELEFT = 180;

    #endregion 常量

    #region 字段/属性

    private int _timeLeft = TIMELEFT;

    // private IRenderEffect[][] _renderEffects = [];

    private readonly Dictionary<Type, HashSet<VertexDrawInfo>> _renderDrawingContents = [];

    public bool IsUILayer { get; init; }

    /// <summary>
    /// 如果什么东西都没有了就会开始倒计时
    /// <br>持续三秒钟没有任何东西就会从<see cref="RenderCanvasSystem._renderingCanvases">中清除掉</br>
    /// <br>目前和这个字典是耦合着的关系，不知道有没有更好的组织方式</br>
    /// </summary>
    public bool ShouldExist
    {
        get
        {
            if (_timeLeft > 0)
                return true;
            if (_renderDrawingContents.Count > 0)
            {
                _timeLeft = TIMELEFT;
                return true;
            }
            return false;
        }
    }

    //private HashSet<HashSet<IRenderEffect>> ActiveRenderEffects
    //{
    //    get
    //    {
    //        HashSet<HashSet<IRenderEffect>> result = [];
    //        foreach (var pipe in _renderEffects)
    //        {
    //            var list = new HashSet<IRenderEffect>();
    //            foreach (var rInfo in pipe)
    //                if (rInfo.Active)
    //                    list.Add(rInfo);
    //            if (list.Count != 0)
    //                result.Add(list);
    //        }
    //        return result;
    //    }
    //}

    public IReadOnlyDictionary<Type, HashSet<VertexDrawInfo>> RenderDrawingContents => _renderDrawingContents;

    #endregion 字段/属性

    #region 构造函数

    public RenderingCanvas()
    { }

    #endregion 构造函数

    /// <summary>
    /// 遍历各类型绘制对象并更新那些需要更新的
    /// 找出那些需要移除的绘制对象
    /// 如果每种绘制对象都已经空了，画布会开始倒计时
    /// 持续三秒以上就可以判定画布可以临时移除了
    /// </summary>
    public void Update()
    {
        if (RenderDrawingContents.Count == 0)
            _timeLeft--;
        HashSet<Type> pendingRemoveSets = [];
        foreach (var pair in RenderDrawingContents)
        {
            var sets = pair.Value;
            if (sets.Count == 0)
                goto label;
            HashSet<VertexDrawInfo> pendingRemoveContents = [];
            foreach (var content in sets)
            {
                content.Update();
                if (!content.Active)
                    pendingRemoveContents.Add(content);
            }
            sets.ExceptWith(pendingRemoveContents);
            if (sets.Count == 0)
                goto label;
            continue;
        label:
            pendingRemoveSets.Add(pair.Key);
        }
        foreach (var pendings in pendingRemoveSets)
            _renderDrawingContents.Remove(pendings);
    }


    /// <summary>
    /// 将绘制内容添加到画布上，并根据类型自动分类
    /// 同时刷新画布的清除倒计时
    /// </summary>
    /// <param name="content"></param>
    public void Add(VertexDrawInfo content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var type = content.GetType();
        if (!_renderDrawingContents.TryGetValue(content.GetType(), out var set) || set is null)
            set = _renderDrawingContents[type] = [];
        set.Add(content);
        _timeLeft = TIMELEFT;
    }

    private static void CanvasPreDraw(bool isUILayer)
    {
        if (isUILayer)
            ModAsset.ShaderSwooshEffectUL.Value.Parameters["uTransform"].SetValue(RenderCanvasSystem.uTransformUILayer);
    }

    private static void DirectlyDrawSingleGroup(IEnumerable<VertexDrawInfo> drawingContents, SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, bool isUILayer)
    {
        var instance = drawingContents.First();
        instance.PreDraw(spriteBatch, graphicsDevice);
        CanvasPreDraw(isUILayer);
        foreach (var info in drawingContents) info.Draw(spriteBatch);
        instance.PostDraw(spriteBatch, graphicsDevice);
    }

    private void DirectlyDrawAllGroups(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        foreach (var drawingContentGroup in _renderDrawingContents.Values)
        {
            if (drawingContentGroup.Count == 0) continue;
            DirectlyDrawSingleGroup(drawingContentGroup, spriteBatch, graphicsDevice, IsUILayer);
        }
    }

    public void DrawContents(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice)
    {
        DirectlyDrawAllGroups(spriteBatch, graphicsDevice);
    }
}