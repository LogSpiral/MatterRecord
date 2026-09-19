using NetSimplified;
using System.IO;
using Terraria;
using Terraria.ID;

namespace MatterRecord.Contents.LittlePrince;

/// <summary>
/// 小王子死亡拦截触发的世界效果同步模块。
/// 死亡拦截（<see cref="LittlePrincePlayer.PreKill"/>）只在玩家本地客户端成立，
/// 而推开敌怪、反弹弹幕、砌玻璃罩都必须由权威端执行才能正确广播，
/// 因此客户端拦截成功后把「玩家索引 + 是否砌罩」发给服务器，由服务器权威执行。
/// 世界状态本身沿原版链路（SyncNPC / SyncProjectile / 图格广播）下发，无需服务器再转发本包。
/// </summary>
internal class LittlePrinceReviveSync : NetModule
{
    /// <summary>触发拦截的玩家在 Main.player 数组中的索引。</summary>
    private short _playerIndex;

    /// <summary>
    /// 是否需要生成玻璃罩。由请求端携带：服务器端玩家的饰品可见性设置不会同步到服务器，
    /// 权威端无法自行查询，只能由客户端告知。
    /// </summary>
    private bool _buildWall;

    /// <summary>
    /// 构造一次复活效果请求（客户端 → 服务器）。
    /// </summary>
    /// <param name="playerIndex">触发拦截的玩家索引（通常为 <c>Player.whoAmI</c>）。</param>
    /// <param name="buildWall">是否生成玻璃罩（对应客户端的饰品可见性）。</param>
    /// <returns>待发送的同步包实例。</returns>
    public static LittlePrinceReviveSync Get(int playerIndex, bool buildWall)
    {
        var packet = NetModuleLoader.Get<LittlePrinceReviveSync>();
        packet._playerIndex = (short)playerIndex;
        packet._buildWall = buildWall;
        return packet;
    }

    /// <summary>
    /// 写入请求数据：玩家索引 + 是否砌罩。
    /// </summary>
    /// <param name="p">待写入的数据包。</param>
    public override void Send(ModPacket p)
    {
        p.Write(_playerIndex);
        p.Write(_buildWall);
    }

    /// <summary>
    /// 读取请求数据，字段顺序必须与 <see cref="Send"/> 保持一致。
    /// </summary>
    /// <param name="r">数据读取器。</param>
    public override void Read(BinaryReader r)
    {
        _playerIndex = r.ReadInt16();
        _buildWall = r.ReadBoolean();
    }

    /// <summary>
    /// 服务器权威执行：校验目标玩家有效后应用击退、弹幕反弹与（可选的）玻璃罩。
    /// 客户端若收到该包则直接忽略，避免各端重复修改世界状态。
    /// </summary>
    public override void Receive()
    {
        if (Main.netMode != NetmodeID.Server)
            return; // 只有服务器有权执行世界效果

        if (_playerIndex < 0 || _playerIndex >= Main.maxPlayers)
            return; // 索引越界（旧包/异常包）直接丢弃

        Player player = Main.player[_playerIndex];
        if (player is null || !player.active || player.dead)
            return; // 玩家已掉线或再次死亡：不再执行

        LittlePrinceReviveEffects.Apply(player, _buildWall);
    }
}
