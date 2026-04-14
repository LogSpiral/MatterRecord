using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader;

namespace MatterRecord.Contents.EmeraldTablet
{
    public static class LootDatabase
    {
        private static Dictionary<int, List<DropEntryInfo>> _npcDropInfos;

        private class DropEntryInfo
        {
            public int ItemID;
            public int StackMin;
            public int StackMax;
            public double DropRate;
            public List<IItemDropRuleCondition> Conditions;
        }

        public static void Build()
        {
            if (_npcDropInfos != null)
                return;

            _npcDropInfos = new Dictionary<int, List<DropEntryInfo>>();

            for (int npcId = -65; npcId < NPCLoader.NPCCount; npcId++)
            {
                if (npcId == 0) continue;

                var rules = Main.ItemDropsDB.GetRulesForNPCID(npcId, false);
                var dropRateInfos = new List<DropRateInfo>();
                var chain = new DropRateInfoChainFeed(1f);
                foreach (var rule in rules)
                    rule.ReportDroprates(dropRateInfos, chain);

                var entries = new List<DropEntryInfo>();
                foreach (var info in dropRateInfos)
                {
                    if (info.itemId <= 0) continue;
                    entries.Add(new DropEntryInfo
                    {
                        ItemID = info.itemId,
                        StackMin = info.stackMin,
                        StackMax = info.stackMax,
                        DropRate = info.dropRate,
                        Conditions = info.conditions
                    });
                }
                _npcDropInfos[npcId] = entries;
            }
        }

        public static List<DropInfo> GetDropsForNPCRealTime(int npcId, Player player)
        {
            if (_npcDropInfos == null) Build();
            if (!_npcDropInfos.TryGetValue(npcId, out var entries)) return new List<DropInfo>();

            NPC dummyNPC = new NPC();
            dummyNPC.SetDefaults(npcId);

            var resultMap = new Dictionary<int, (double rate, int min, int max, bool available)>();

            foreach (var entry in entries)
            {
                bool canDrop = true;
                if (entry.Conditions != null && entry.Conditions.Count > 0)
                {
                    var dropAttemptInfo = new DropAttemptInfo
                    {
                        player = player,
                        npc = dummyNPC,
                        IsExpertMode = Main.expertMode,
                        IsMasterMode = Main.masterMode,
                        IsInSimulation = true
                    };
                    foreach (var condition in entry.Conditions)
                    {
                        if (!condition.CanDrop(dropAttemptInfo))
                        {
                            canDrop = false;
                            break;
                        }
                    }
                }

                double effectiveRate = canDrop ? entry.DropRate : 0;

                if (resultMap.TryGetValue(entry.ItemID, out var existing))
                {
                    resultMap[entry.ItemID] = (
                        existing.rate + effectiveRate,
                        Math.Min(existing.min, entry.StackMin),
                        Math.Max(existing.max, entry.StackMax),
                        existing.available || canDrop
                    );
                }
                else
                {
                    resultMap[entry.ItemID] = (effectiveRate, entry.StackMin, entry.StackMax, canDrop);
                }
            }

            var drops = new List<DropInfo>();
            foreach (var kv in resultMap)
            {
                drops.Add(new DropInfo
                {
                    ItemID = kv.Key,
                    DropRate = kv.Value.rate,
                    StackMin = kv.Value.min,
                    StackMax = kv.Value.max,
                    IsAvailable = kv.Value.available
                });
            }
            return drops;
        }

        public static void Clear()
        {
            _npcDropInfos = null;
        }
    }
}