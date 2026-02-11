using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.LordOfTheFlies;

[AutoSync]
internal class LordOfTheFliesAnniChargingSync : NetModule
{
    private byte _whoAmI;
    private bool _isCharging;
    public static LordOfTheFliesAnniChargingSync Get(int whoAmI, bool isCharging)
    {
        var packet = NetModuleLoader.Get<LordOfTheFliesAnniChargingSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._isCharging = isCharging;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<LordOfTheFliesPlayer>();
        modPlayer.IsChargingAnnihilation = _isCharging;
        if (Main.dedServ)
            Get(_whoAmI, _isCharging).Send(-1, Sender);
    }
}
