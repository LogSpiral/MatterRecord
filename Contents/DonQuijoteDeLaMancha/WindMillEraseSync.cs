using NetSimplified;
using System.IO;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

/// <summary>
/// 风车「摧毁敌弹」请求的网络同步模块。
/// 多人模式下，客户端将命中的敌弹索引发送给服务器，
/// 由服务器权威移除并通过 SyncProjectile 广播给所有玩家，
/// 避免仅本地 Kill 导致弹幕被服务器同步「复活」。
/// </summary>
internal class WindMillEraseSync : NetModule
{
    /// <summary>弹幕在 Main.projectile 数组中的索引。</summary>
    private short _projIndex;

    /// <summary>构造一个摧毁请求（客户端 → 服务器）。</summary>
    /// <param name="projIndex">弹幕索引。</param>
    public static WindMillEraseSync Get(int projIndex)
    {
        var packet = NetModuleLoader.Get<WindMillEraseSync>();
        packet._projIndex = (short)projIndex;
        return packet;
    }

    public override void Send(ModPacket p) => p.Write(_projIndex);

    public override void Read(BinaryReader r) => _projIndex = r.ReadInt16();

    public override void Receive()
    {
        // 仅服务器收到该包：权威移除弹幕（若仍存活）
        Projectile proj = Main.projectile[_projIndex];
        if (proj.active)
            WindMill.KillProjectile(proj);
    }
}
