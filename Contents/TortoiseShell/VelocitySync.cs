using Microsoft.Xna.Framework;
using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.TortoiseShell;

[AutoSync]
internal class VelocitySync : NetModule
{
    private byte _whoAmI;
    private Vector2 _velocity;
    public static VelocitySync Get(int whoAmI, Vector2 velocity)
    {
        var packet = NetModuleLoader.Get<VelocitySync>();
        packet._whoAmI = (byte)whoAmI;
        packet._velocity = velocity;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<TortoiseShellPlayer>();
        modPlayer.SetSyncVelocity(_velocity);
        if (Main.dedServ)
            Get(_whoAmI, _velocity).Send(-1, Sender);
    }
}
