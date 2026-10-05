using System.Collections.Generic;
using Terraria.DataStructures;
namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodotGlobalNPC : GlobalNPC
{
    private static bool _isInNewNPC;
    private static int NewNpcMask(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target)
    {
        _isInNewNPC = true;
        return orig.Invoke(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
    }

    public override void Load()
    {
        On_NPC.NewNPC += NewNpcMask;
        On_NPC.SetDefaultsFromNetId += NetIdCheck;
        base.Load();
    }

    private void NetIdCheck(On_NPC.orig_SetDefaultsFromNetId orig, NPC self, int id, NPCSpawnParams spawnparams)
    {
        if (_cachedPlayerInfo2 == null) 
        {
            orig?.Invoke(self, id, spawnparams);
            return;
        }
        if (_cachedPlayerInfo2.BannedNPCType.Contains(id))
            self.active = false;
        _cachedPlayerInfo2 = null;
    }

    public override void Unload()
    {
        On_NPC.NewNPC -= NewNpcMask;
        On_NPC.SetDefaultsFromNetId -= NetIdCheck;
        base.Unload();
    }
    private static EnAttendantGodotPlayer _cachedPlayerInfo;
    private static EnAttendantGodotPlayer _cachedPlayerInfo2;
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
        _cachedPlayerInfo2 = _cachedPlayerInfo;
        _cachedPlayerInfo = null;
    }

    public override void SetDefaults(NPC entity)
    {
        if (_isInNewNPC) 
        {
            _isInNewNPC = false;
            return;
        }
        if (_cachedPlayerInfo2 == null) return;
        if (_cachedPlayerInfo2.BannedNPCType.Contains(entity.type)
                || _cachedPlayerInfo2.BannedNPCType.Contains(entity.netID))
             entity.active = false;
        _cachedPlayerInfo2 = null;
    }
}
