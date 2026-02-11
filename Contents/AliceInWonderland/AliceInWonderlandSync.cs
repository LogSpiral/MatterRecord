using MatterRecord.Contents.LordOfTheFlies;
using Microsoft.Xna.Framework;
using NetSimplified;
using System.IO;

namespace MatterRecord.Contents.AliceInWonderland;


internal class AliceInWonderlandSync : NetModule
{
    private byte _whoAmI;
    private bool _hasValue;
    private Vector2 _start;
    private Vector2 _end;
    public static AliceInWonderlandSync Get(int whoAmI)
    {
        var packet = NetModuleLoader.Get<AliceInWonderlandSync>();
        packet._whoAmI = (byte)whoAmI;
        return packet;
    }
    public static AliceInWonderlandSync Get(int whoAmI, Vector2 start, Vector2 end)
    {
        var packet = NetModuleLoader.Get<AliceInWonderlandSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._hasValue = true;
        packet._start = start;
        packet._end = end;
        return packet;
    }
    public override void Send(ModPacket p)
    {
        p.Write(_whoAmI);
        if (_hasValue)
        {
            p.Write(true);
            p.WriteVector2(_start);
            p.WriteVector2(_end);
        }
        else
        {
            p.Write(false);
        }
        base.Send(p);
    }
    public override void Read(BinaryReader r)
    {
        _whoAmI = r.ReadByte();
        _hasValue = r.ReadBoolean();
        if (_hasValue)
        {
            _start = r.ReadVector2();
            _end = r.ReadVector2();
        }
        base.Read(r);
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<AliceInWonderlandPlayer>();
        if (_hasValue)
        {
            modPlayer.CurrentPortalStart = _start;
            modPlayer.CurrentPortalEnd = _end;
            if (Main.dedServ)
                Get(_whoAmI, _start, _end).Send(-1, Sender);
        }
        else
        {
            modPlayer.CurrentPortalStart = null;
            modPlayer.CurrentPortalEnd = null;
            if (Main.dedServ)
                Get(_whoAmI).Send(-1, Sender);
        }
    }
}
