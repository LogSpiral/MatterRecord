using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
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
        if (_pendingSet2 == null)
        {
            orig?.Invoke(self, id, spawnparams);
            return;
        }
        if (_pendingSet2.Contains(self.type)
                || _pendingSet2.Contains(self.netID))
            self.active = false;
        _pendingSet2 = null;
    }

    public override void Unload()
    {
        On_NPC.NewNPC -= NewNpcMask;
        On_NPC.SetDefaultsFromNetId -= NetIdCheck;
        base.Unload();
    }
    private static readonly Dictionary<int, HashSet<int>> _cachedBannedSet = [];

    private static HashSet<int> _pendingSet1;
    private static HashSet<int> _pendingSet2;
    private static readonly Dictionary<int, int> _bannedListUpdateCounter = [];
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        // 我草密码的这东西怎么只对ModNPC有效的

        var position = spawnInfo.Player.position;
        int index = spawnInfo.Player.whoAmI;
        if (!_cachedBannedSet.TryGetValue(index, out var set))
            set = _cachedBannedSet[index] = [];

        if (!_bannedListUpdateCounter.TryGetValue(index, out var value))
            _bannedListUpdateCounter[index] = 0;

        if (value <= 0)
        {
            _bannedListUpdateCounter[index] = 5;
            set.Clear();
            foreach (var plr in Main.ActivePlayers)
            {
                if (Math.Abs(plr.position.X - position.X) >= 1000 || Math.Abs(plr.position.Y - position.Y) >= 1000) continue;
                if (!plr.TryGetModPlayer<EnAttendantGodotPlayer>(out var mplr) || !mplr.EnAttendantGodotEquipped) continue;
                set.UnionWith(mplr.BannedNPCType);

            }
        }
        else
        {
            _bannedListUpdateCounter[index]--;
        }
        foreach (var ban in set)
            pool.Remove(ban);

        _pendingSet1 = set;
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        // 我草密码的史莱姆，我真服了
        if (_pendingSet1 == null
            || source is not EntitySource_SpawnNPC) return;
        if (_pendingSet1.Contains(npc.type)
            || _pendingSet1.Contains(npc.netID))
            npc.active = false;
        _pendingSet2 = _pendingSet1;
        _pendingSet1 = null;
    }

    public override void SetDefaults(NPC entity)
    {
        if (_isInNewNPC)
        {
            _isInNewNPC = false;
            return;
        }
        if (_pendingSet2 == null) return;
        if (_pendingSet2.Contains(entity.type)
                || _pendingSet2.Contains(entity.netID))
            entity.active = false;
        _pendingSet2 = null;
    }
}
