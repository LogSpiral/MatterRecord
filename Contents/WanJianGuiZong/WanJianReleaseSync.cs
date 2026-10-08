using System.IO;
using Microsoft.Xna.Framework;
using NetSimplified;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 放剑的网络请求（持剑客户端 → 服务器）。
/// <para>为什么需要它：<see cref="Projectile.Update"/> 里那段处理 <c>netUpdate</c> 的代码带
/// <c>owner != Main.myPlayer</c> 判断——在服务器上，客户端召唤出来的剑 owner 是那个客户端，
/// 永远不等于服务器自己的 <c>Main.myPlayer</c>，所以服务器不会把状态变化转发出去；
/// 只在客户端设 netUpdate 的结果就是：队友那边剑一直挂在召唤者身边盘旋，直到弹幕超时被服务器杀掉。</para>
/// <para>所以放出这张牌改由持剑客户端发请求，服务器改自己那份副本、再显式广播 SyncProjectile，
/// 这样所有客户端都能看到剑真正飞出去。</para>
/// </summary>
internal class WanJianReleaseSync : NetModule
{
    private byte _whoAmI;
    private float _directionX;
    private float _directionY;

    /// <summary>构造「把某玩家手上盘旋中的剑全部放出去」的请求包。</summary>
    public static WanJianReleaseSync Get(int whoAmI, Vector2 direction)
    {
        var packet = NetModuleLoader.Get<WanJianReleaseSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._directionX = direction.X;
        packet._directionY = direction.Y;
        return packet;
    }

    public override void Send(ModPacket p)
    {
        p.Write(_whoAmI);
        p.Write(_directionX);
        p.Write(_directionY);
        base.Send(p);
    }

    public override void Read(BinaryReader r)
    {
        _whoAmI = r.ReadByte();
        _directionX = r.ReadSingle();
        _directionY = r.ReadSingle();
        base.Read(r);
    }

    public override void Receive()
    {
        // 只由服务器处理：改自己那份副本并广播给所有客户端
        if (Main.netMode != NetmodeID.Server)
            return;

        WanJianGuiZongPlayer.ReleaseSwords(_whoAmI, new Vector2(_directionX, _directionY), broadcast: true);
    }
}
