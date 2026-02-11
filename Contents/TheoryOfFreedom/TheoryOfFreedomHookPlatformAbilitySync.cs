using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.TheoryOfFreedom;

[AutoSync]
internal class TheoryOfFreedomHookPlatformAbilitySync : NetModule
{
    private byte _whoAmI;
    private bool _canHookOnPlatform;
    public static TheoryOfFreedomHookPlatformAbilitySync Get(int whoAmI, bool canHookOnPlatform)
    {
        var packet = NetModuleLoader.Get<TheoryOfFreedomHookPlatformAbilitySync>();
        packet._whoAmI = (byte)whoAmI;
        packet._canHookOnPlatform = canHookOnPlatform;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<TheTheoryOfFreedomPlayer>();
        modPlayer.CanHookPlatform = _canHookOnPlatform;
        if (Main.dedServ)
            Get(_whoAmI, _canHookOnPlatform).Send(-1, Sender);
    }
}
