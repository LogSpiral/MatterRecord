global using Terraria;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.ModLoader;
using NetSimplified;
using System.IO;
using System.Reflection;
namespace MatterRecord;
// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class MatterRecord : Mod
{
    public static MatterRecord Instance { get; private set; }
    public override void Load()
    {
        Instance = this;
        NetModuleLoader.CurrentMod = this;
        NetModuleLoader.LoadAutoSyncsFrom(typeof(NetModuleLoader).Assembly);
        NetModuleLoader.LoadAutoSyncsFrom(Assembly.GetExecutingAssembly());
        NetModuleLoader.LoadNetModules();
    }

    public override object Call(params object[] args)
    {
        object netReply = CrossMod.HandleModCalls(args);
        if (netReply is not false)
        {
            return netReply;
        }

        return base.Call(args);
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        NetModule.ReceiveModule(reader, whoAmI);
    }
}