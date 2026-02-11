using NetSimplified;
using NetSimplified.Syncing;
namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

[AutoSync]
internal class DonQuijoteDeLaManchaAISync : NetModule
{
    private byte _whoAmI;
    private ushort _dashCoolDown;
    private ushort _dashCoolDownMax;
    private bool _dashing;
    private bool _nextHitImmune;
    private ushort _stabTimeLeft;
    public static DonQuijoteDeLaManchaAISync Get(
        int whoAmI,
        int dashCoolDown,
        int dashCoolDownMax,
        bool dashing,
        bool nextHitImmune,
        int stabTimeLeft)
    {
        var packet = NetModuleLoader.Get<DonQuijoteDeLaManchaAISync>();
        packet._whoAmI = (byte)whoAmI;
        packet._dashCoolDown = (ushort)dashCoolDown;
        packet._dashCoolDownMax = (ushort)dashCoolDownMax;
        packet._dashing = dashing;
        packet._nextHitImmune = nextHitImmune;
        packet._stabTimeLeft = (ushort)stabTimeLeft;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        modPlayer.DashCoolDown = _dashCoolDown;
        modPlayer.DashCoolDownMax = _dashCoolDownMax;
        modPlayer.Dashing = _dashing;
        modPlayer.NextHitImmune = _nextHitImmune;
        modPlayer.StabTimeLeft = _stabTimeLeft;
        if (Main.dedServ)
            Get(
                _whoAmI, 
                _dashCoolDown, 
                _dashCoolDownMax, 
                _dashing,
                _nextHitImmune, 
                _stabTimeLeft)
                .Send(-1, Sender);
    }
}
