using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.Faust;

[AutoSync]
internal class FaustSync : NetModule
{
    private byte _whoAmI;
    private ulong _consumedMoney;
    public static FaustSync Get(int whoAmI, ulong consumedMoney)
    {
        var packet = NetModuleLoader.Get<FaustSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._consumedMoney = consumedMoney;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<FaustPlayer>();
        modPlayer.ConsumedMoney = _consumedMoney;
        if (Main.dedServ)
            Get(_whoAmI, _consumedMoney).Send(-1, Sender);
    }
}
