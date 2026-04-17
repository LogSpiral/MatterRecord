global using Terraria;
global using Terraria.ID;
global using Terraria.IO;
global using Terraria.ModLoader;
using NetSimplified;
using System.IO;
using System.Reflection;
using Terraria.GameContent.Creative;
namespace MatterRecord;
// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
public class MatterRecord : Mod
{
    public static MatterRecord Instance { get; private set; }
    public override void Load()
    {
        Instance = this;
        AddContent<NetModuleLoader>();

        // 在 2.0 版本，如果不需要 Diagnostic 工具，可以不注册 NetModuleLoader 作为 ModType
        // AddContent<NetModuleLoader>();
        // 设置当前模组实例以供 NetModuleLoader 使用
        NetModuleLoader.CurrentMod = this;

        // 先注册基础库中的 AutoSyncType 与示例模组内的 AutoSyncType
        NetModuleLoader.LoadAutoSyncsFrom(typeof(NetModuleLoader).Assembly);
        NetModuleLoader.LoadAutoSyncsFrom(Assembly.GetExecutingAssembly());

        // 加载并注册 NetModule 实例
        NetModuleLoader.LoadNetModules();
    }
    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        NetModule.ReceiveModule(reader, whoAmI);
    }
    public override void PostSetupContent()
    {
        for (int n = 0; n < ItemLoader.ItemCount; n++)
            CreativeUI.ResearchItem(n);
    }
}
public class RelockPlayer : ModPlayer 
{
    public override void OnEnterWorld()
    {
        for (int n = 0; n < ItemLoader.ItemCount; n++)
            CreativeUI.ResearchItem(n);
    }
}