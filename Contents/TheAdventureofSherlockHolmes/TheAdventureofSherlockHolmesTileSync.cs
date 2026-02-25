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

        int xcoord = _coord.X % 200;
        int ycoord = _coord.Y % 150;


        int xoff = xcoord switch
        {
            < 18 => -1,
            >= 200 - 18 => 1,
            _ => 0
        };
        int yoff = ycoord switch
        {
            < 18 => -1,
            >= 150 - 18 => 1,
            _ => 0
        };
        if (xoff != 0)
        {
            if (yoff != 0)
            {
                NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150);
                NetMessage.SendSection(_whoAmI, point.X / 200 + xoff, point.Y / 150);
                NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150 + yoff);
                NetMessage.SendSection(_whoAmI, point.X / 200 + xoff, point.Y / 150 + yoff);
            }
            else
            {
                NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150);
                NetMessage.SendSection(_whoAmI, point.X / 200 + xoff, point.Y / 150);
            }
        }
        else
        {
            if (yoff != 0)
            {
                NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150);
                NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150 + yoff);
            }
            else 
            {
                NetMessage.SendSection(_whoAmI, point.X / 200, point.Y / 150);
            }
        }
        TheAdventureofSherlockHolmesSucceedSync.Get().Send(Sender);
    }
}
