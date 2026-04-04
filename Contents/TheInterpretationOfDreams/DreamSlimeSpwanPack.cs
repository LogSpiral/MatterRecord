using Microsoft.Xna.Framework;
using NetSimplified;
using NetSimplified.Syncing;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

[AutoSync]
internal class DreamSlimeSpwanPack : NetModule
{
    private int _coordX;
    private int _coordY;
    public static DreamSlimeSpwanPack Get(Vector2 position)
    {
        var packet = NetModuleLoader.Get<DreamSlimeSpwanPack>();
        packet._coordX = (int)position.X;
        packet._coordY = (int)position.Y;
        return packet;
    }
    public override void Receive()
    {
        if (Main.netMode == NetmodeID.MultiplayerClient) return;
        int index = NPC.NewNPC(new EntitySource_Misc("Using ZoologiseDream"), _coordX, _coordY, ModContent.NPCType<DreamSlime>());
        if (Main.dedServ)
            NetMessage.SendData(MessageID.SyncNPC, number: index);

    }
}
