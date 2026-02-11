global using Terraria;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.ModLoader;
using NetSimplified;
using System.IO;
namespace MatterRecord;
// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class MatterRecord : Mod
{
    public static MatterRecord Instance { get; private set; }
    public override void Load()
    {
        Instance = this;
        AddContent<NetModuleLoader>();
    }
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        NetModule.ReceiveModule(reader, whoAmI);
    }
}