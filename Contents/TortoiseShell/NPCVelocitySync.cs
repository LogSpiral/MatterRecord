using Microsoft.Xna.Framework;
using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.TortoiseShell;

[AutoSync]
internal class NPCVelocitySync : NetModule
{
    private byte _whoAmI;
    private Vector2 _velocity;
    public static NPCVelocitySync Get(int whoAmI, Vector2 velocity)
    {
        var packet = NetModuleLoader.Get<NPCVelocitySync>();
        packet._whoAmI = (byte)whoAmI;
        packet._velocity = velocity;
        return packet;
    }
    public override void Receive()
    {
        var npc = Main.npc[_whoAmI];
        npc.velocity = _velocity;
        if (Main.dedServ)
            Get(_whoAmI, _velocity).Send(-1, Sender);
    }
}