using NetSimplified;
using System.IO;

namespace MatterRecord.Contents.Eraser;

/// <summary>
/// 橡皮擦「擦除」实体的网络同步模块。
/// 多人模式下，客户端将命中的实体（NPC / 弹幕）索引发送给服务器，
/// 由服务器权威执行擦除并通过 SyncNPC / SyncProjectile 广播给所有玩家，
/// 避免仅本地修改状态导致实体被服务器同步「复活」。
/// </summary>
internal class EraserEraseSync : NetModule
{
    /// <summary>实体类型：0 = NPC，1 = 弹幕。</summary>
    private byte _entityType;

    /// <summary>实体在 Main.npc / Main.projectile 数组中的索引。</summary>
    private short _entityIndex;

    /// <summary>
    /// 构造一个擦除请求（客户端 → 服务器）。
    /// </summary>
    /// <param name="entityType">实体类型：0 = NPC，1 = 弹幕。</param>
    /// <param name="entityIndex">实体索引。</param>
    public static EraserEraseSync Get(byte entityType, int entityIndex)
    {
        var packet = NetModuleLoader.Get<EraserEraseSync>();
        packet._entityType = entityType;
        packet._entityIndex = (short)entityIndex;
        return packet;
    }

    public override void Send(ModPacket p)
    {
        p.Write(_entityType);
        p.Write(_entityIndex);
    }

    public override void Read(BinaryReader r)
    {
        _entityType = r.ReadByte();
        _entityIndex = r.ReadInt16();
    }

    public override void Receive()
    {
        if (_entityType == 0)
        {
            // NPC：由服务器权威秒杀
            NPC npc = Main.npc[_entityIndex];
            if (npc.active && npc.life > 0)
                Eraser.KillNpc(npc);
        }
        else
        {
            // 弹幕：由服务器权威移除
            Projectile proj = Main.projectile[_entityIndex];
            if (proj.active)
                Eraser.KillProjectile(proj);
        }
    }
}
