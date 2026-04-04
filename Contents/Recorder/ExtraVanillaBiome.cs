using Terraria.GameContent.Personalities;

namespace MatterRecord.Contents.Recorder;


public class UnderworldBiome : ILoadable, IShoppingBiome
{
    string IShoppingBiome.NameKey => "Underworld";

    bool IShoppingBiome.IsInBiome(Player player) => player.ZoneUnderworldHeight;

    void ILoadable.Load(Mod mod)
    {
    }

    void ILoadable.Unload()
    {
    }
}
