using NetSimplified;
using System.IO;
using Terraria;
using Terraria.ID;

namespace MatterRecord.Contents.TheCountOfMonteCristo;

/// <summary>
/// 《基督山伯爵》「讨回失落财宝」获取方式的网络同步模块。
/// <para>拾取累计与触发判定只发生在拾取者本端，而物品产出必须由权威端执行才能被
/// 所有玩家看到（并正确走记录系统的「已持有 / 冷却」判定），因此客户端触发后把
/// 玩家索引发给服务器，由服务器权威产出，产出结果沿原版物品同步链路下发。</para>
/// </summary>
internal class MonteCristoCoinSync : NetModule
{
    /// <summary>触发获取判定的玩家在 Main.player 数组中的索引。</summary>
    private short _playerIndex;

    /// <summary>
    /// 构造一次获取请求（客户端 → 服务器）。
    /// </summary>
    /// <param name="playerIndex">触发判定的玩家索引（通常为 <c>Player.whoAmI</c>）。</param>
    /// <returns>待发送的同步包实例。</returns>
    public static MonteCristoCoinSync Request(int playerIndex)
    {
        var packet = NetModuleLoader.Get<MonteCristoCoinSync>();
        packet._playerIndex = (short)playerIndex;
        return packet;
    }

    /// <summary>
    /// 写入请求数据：玩家索引。
    /// </summary>
    /// <param name="p">待写入的数据包。</param>
    public override void Send(ModPacket p)
    {
        p.Write(_playerIndex);
    }

    /// <summary>
    /// 读取请求数据，字段顺序必须与 <see cref="Send"/> 保持一致。
    /// </summary>
    /// <param name="r">数据读取器。</param>
    public override void Read(BinaryReader r)
    {
        _playerIndex = r.ReadInt16();
    }

    /// <summary>
    /// 服务器权威执行：校验玩家有效后在服务器端产出《基督山伯爵》，
    /// 客户端若收到该包则忽略（本包只用于「客户端 → 服务器」方向）。
    /// </summary>
    public override void Receive()
    {
        // 索引越界（旧包 / 异常包）直接丢弃
        if (_playerIndex < 0 || _playerIndex >= Main.maxPlayers)
            return;

        Player player = Main.player[_playerIndex];
        if (player is null || !player.active)
            return;

        // 真正的产出判定（是否已持有、是否在冷却）在 GrantRecordItem 内部完成
        player.GetModPlayer<MonteCristoPlayer>().GrantRecordItem();
    }
}
