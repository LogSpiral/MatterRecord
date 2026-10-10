using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

/// <summary>
/// 堂吉诃德「连击复活」的状态同步模块。
/// <para>
/// 死亡拦截只在玩家本地客户端成立（<see cref="DonQuijoteDeLaManchaPlayer.PreKill"/>），
/// 但服务端与旁观端那份副本同样吃到了这一发致命伤：客户端上报的 PlayerHurt 会在服务端重放一次
/// （MessageBuffer case 117），接触伤害又是在 Player.Update 里各端各算一份，
/// 于是它们的血量都停在 0。原版的 netLife 链路只保证服务器知道新血量，不会清掉别处可能已经置位的
/// <see cref="Player.dead"/>，所以这里由发起端主动把「我还活着、血是这么多」交给服务器，
/// 服务器修正自己的副本后再广播出去，队友才不会继续按空血/已死处理这名玩家。
/// </para>
/// </summary>
[AutoSync]
internal class DonQuijoteReviveSync : NetModule
{
    /// <summary>触发复活的玩家在 <see cref="Main.player"/> 数组中的索引。</summary>
    private byte _whoAmI;

    /// <summary>复活后的血量（发起端权威值）。</summary>
    private short _life;

    /// <summary>构造一次复活同步（客户端 → 服务器 → 其它客户端）。</summary>
    /// <param name="whoAmI">触发复活的玩家索引。</param>
    /// <param name="life">复活后的血量。</param>
    public static DonQuijoteReviveSync Get(int whoAmI, int life)
    {
        var packet = NetModuleLoader.Get<DonQuijoteReviveSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._life = (short)(life < 0 ? 0 : life > short.MaxValue ? short.MaxValue : life);
        return packet;
    }

    public override void Receive()
    {
        // 单机不会发这个包；预览端也不该改别人的血量
        if (Main.netMode != NetmodeID.Server && Main.netMode != NetmodeID.MultiplayerClient)
            return;

        if (_whoAmI >= Main.maxPlayers)
            return;

        Player player = Main.player[_whoAmI];
        if (player is null || !player.active)
            return;

        // 只往上抬：别把其它端还没结算伤害时更高的血量按下去，但被误置的死亡标记必须清掉
        if (player.statLife < _life)
            player.statLife = _life;
        player.dead = false;

        if (Main.netMode == NetmodeID.Server)
            Get(_whoAmI, _life).Send(-1, Sender);
    }
}
