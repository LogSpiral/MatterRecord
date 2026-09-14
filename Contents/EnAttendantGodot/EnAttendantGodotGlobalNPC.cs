using System.Collections.Generic;
using Terraria.DataStructures;
namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodotGlobalNPC : GlobalNPC
{
    private static EnAttendantGodotPlayer _cachedPlayerInfo;
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        // 我草密码的这东西怎么只对ModNPC有效的
        if (!spawnInfo.Player.TryGetModPlayer<EnAttendantGodotPlayer>(out var mplr)
            || !mplr.EnAttendantGodotEquipped) return;
        foreach (var key in mplr.BannedNPCType)
            pool.Remove(key);
        _cachedPlayerInfo = mplr;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        // 我草密码的史莱姆，我真服了
        if (_cachedPlayerInfo == null
            || source is not EntitySource_SpawnNPC) return;
        if (_cachedPlayerInfo.BannedNPCType.Contains(npc.type)
            || _cachedPlayerInfo.BannedNPCType.Contains(npc.netID))
            npc.active = false;
        _cachedPlayerInfo = null;
    }
}
