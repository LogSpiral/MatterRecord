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
    private ushort _tauntTimer; // 嘲讽剩余帧数（多人下同步到服务器权威端，保证位置欺骗在服务器上生效）
    private ushort _comboCount; // 连击数（owner 本地权威，同步到服务器/其它端保证挥砍尺寸一致）
    public static DonQuijoteDeLaManchaAISync Get(
        int whoAmI,
        int dashCoolDown,
        int dashCoolDownMax,
        bool dashing,
        bool nextHitImmune,
        int stabTimeLeft,
        int tauntTimer,
        int comboCount)
    {
        var packet = NetModuleLoader.Get<DonQuijoteDeLaManchaAISync>();
        packet._whoAmI = (byte)whoAmI;
        packet._dashCoolDown = (ushort)dashCoolDown;
        packet._dashCoolDownMax = (ushort)dashCoolDownMax;
        packet._dashing = dashing;
        packet._nextHitImmune = nextHitImmune;
        packet._stabTimeLeft = (ushort)stabTimeLeft;
        packet._tauntTimer = (ushort)tauntTimer;
        packet._comboCount = (ushort)comboCount;
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
        modPlayer.TauntTimer = _tauntTimer;
        modPlayer.ComboCount = _comboCount;
        if (Main.dedServ)
            Get(
                _whoAmI,
                _dashCoolDown,
                _dashCoolDownMax,
                _dashing,
                _nextHitImmune,
                _stabTimeLeft,
                _tauntTimer,
                _comboCount)
                .Send(-1, Sender);
    }
}
