using Microsoft.Xna.Framework;
using NetSimplified;
using NetSimplified.Syncing;
using System.IO;

namespace MatterRecord.Contents.TheoryOfFreedom;

internal class HookPointSync : NetModule
{
    private byte _whoAmI;
    private Point[] _coords;
    public override void Send(ModPacket p)
    {
        var length = (byte)_coords.Length;
        p.Write(_whoAmI);
        p.Write(length);
        for (byte n = 0; n < length; n++) 
        {
            var coord = _coords[n];
            p.Write(coord.X);
            p.Write(coord.Y);
        }
    }
    public override void Read(BinaryReader r)
    {
        _whoAmI = r.ReadByte();
        int length = r.ReadByte();
        _coords = new Point[length];
        for (int n = 0; n < length; n++)
            _coords[n] = new(r.ReadInt32(), r.ReadInt32());
    }
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
