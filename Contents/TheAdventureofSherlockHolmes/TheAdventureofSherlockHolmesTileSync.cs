using Microsoft.Xna.Framework;
using NetSimplified;
using NetSimplified.Syncing;
namespace MatterRecord.Contents.TheAdventureofSherlockHolmes;

[AutoSync]
internal class TheAdventureofSherlockHolmesTileSync : NetModule
{
    public static TheAdventureofSherlockHolmesTileSync Get(int whoAmI, Point coord)
    {
        var packet = NetModuleLoader.Get<TheAdventureofSherlockHolmesTileSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._coord = coord;
        return packet;
    }
    private byte _whoAmI;
    private Point _coord;
    public override void Receive()
    {
        if (!Main.dedServ) return;
        var point = _coord;
        NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150);
        TheAdventureofSherlockHolmesSucceedSync.Get().Send(Sender);
    }
}
