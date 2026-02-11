using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.EternalWine;

[AutoSync]
internal class EternalWineSync : NetModule
{
    private byte _whoAmI;
    private float _debt;
    private float _maxDebt;
    public static EternalWineSync Get(int whoAmI, float debt, float maxDebt)
    {
        var packet = NetModuleLoader.Get<EternalWineSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._debt = debt;
        packet._maxDebt = maxDebt;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<EternalWinePlayer>();
        modPlayer.SetLifeDebt(_debt, _maxDebt);
        if (Main.dedServ)
            Get(_whoAmI, _debt, _maxDebt).Send(-1, Sender);
    }
}
