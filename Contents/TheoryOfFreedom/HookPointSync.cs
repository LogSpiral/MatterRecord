using Microsoft.Xna.Framework;
using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.TheoryOfFreedom;

[AutoSync]
internal class HookPointSync : NetModule
{
    private byte _whoAmI;
    private Point[] _coords;
    public static HookPointSync Get(int whoAmI, Point[] coords)
    {
        var packet = NetModuleLoader.Get<HookPointSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._coords = coords;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<TheTheoryOfFreedomPlayer>();
        modPlayer.TargetTileCoords.Clear();
        modPlayer.TargetTileCoords.AddRange(_coords);
        if (Main.dedServ)
            Get(_whoAmI, _coords).Send(-1, Sender);
    }
}
