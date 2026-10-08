using NetSimplified;
using System.IO;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

/// <summary>
/// 行走种植的落地请求（客户端 → 服务端）。
/// <para>种子由客户端自己扣（客户端对自己背包有权威），图格改动交给服务端执行；
/// 服务端只复核坐标与下方物块，草药样式由服务端重新推导，不采信客户端传来的值。</para>
/// </summary>
internal class WalkingPlantRequest : NetModule
{
    /// <summary>目标图格 X。</summary>
    private short _tileX;

    /// <summary>目标图格 Y（玩家脚下那一格）。</summary>
    private short _tileY;

    public static WalkingPlantRequest Get(int x, int y)
    {
        var packet = NetModuleLoader.Get<WalkingPlantRequest>();
        packet._tileX = (short)x;
        packet._tileY = (short)y;
        return packet;
    }

    public override void Send(ModPacket p)
    {
        p.Write(_tileX);
        p.Write(_tileY);
    }

    public override void Read(BinaryReader r)
    {
        _tileX = r.ReadInt16();
        _tileY = r.ReadInt16();
    }

    public override void Receive()
    {
        // 只有服务端能改图格；单机 / Host & Play 走的是 ModPlayer 里的本地落地分支
        if (Main.netMode != NetmodeID.Server)
            return;

        // 客户端在发请求前已经判定过佩戴状态与种子，这里只复核位置是否还能种
        if (!CompendiumWalkingPlant.TryGetPlantTarget(_tileX, _tileY, out int herbStyle, out bool needKillFlower))
            return;

        CompendiumWalkingPlant.ApplyPlant(_tileX, _tileY, herbStyle, needKillFlower);
    }
}
