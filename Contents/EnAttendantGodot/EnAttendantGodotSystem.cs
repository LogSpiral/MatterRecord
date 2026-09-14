using System.Collections.Generic;

namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodotSystem : ModSystem
{
    private readonly Dictionary<int, int> _bannerItemType2NPCType = [];
    public IReadOnlyDictionary<int, int> BannerItemType2NPCType => _bannerItemType2NPCType;
    public static EnAttendantGodotSystem Instance { get; private set; }
    public override void PostSetupContent()
    {
        _bannerItemType2NPCType.Clear();
        for (int type = -10; type < NPCLoader.NPCCount; type++) 
        {
            if (type == 0) continue;
            int bannerId = Item.NPCtoBanner(type);
            if (bannerId <= 0) continue;
            _bannerItemType2NPCType.TryAdd(Item.BannerToItem(bannerId), type);
        }
    }
    public override void Load()
    {
        Instance = this;
    }
    public override void Unload()
    {
        Instance = null;
    }
}
