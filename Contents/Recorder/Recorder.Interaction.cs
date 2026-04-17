using MatterRecord.Contents.Recorder.Dialogue;
using MatterRecord.Contents.TheInterpretationOfDreams;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace MatterRecord.Contents.Recorder;

public partial class Recorder
{
    public static class Interactions
    {
        public class Copy : NPCInteraction
        {
            public static Copy Instance { get;} = new();
            private Copy() { }
            public override bool Condition() => TalkNPCType == ModContent.NPCType<Recorder>() && RecorderSystem.GetUnlockCount() > 0;

            public override string GetText() => CopyShopText.Value;

            public override void Interact()
            {
                Main.playerInventory = true;
                Main.stackSplit = 9999;
                Main.npcChatText = "";
                Main.SetNPCShopIndex(1);
                Main.instance.shop[Main.npcShop].SetupShop(NPCShopDatabase.GetShopName(TalkNPCType, SHOPNAME), TalkNPC);
            }
        }

        public class MultiInteraction(int targetNPCType, IEnumerable<NPCInteraction> interactions, bool cacheMode = true) : NPCInteraction
        {
            public int TargetNPCType { get; init; } = targetNPCType;
            public bool CacheMode { get; init; } = cacheMode;
            public IEnumerable<NPCInteraction> Interactions { get; init; } = interactions;
            private NPCInteraction CurrentInteraction { get; set; }
            public bool ForceUpdate { private get; set; }
            public override bool Condition()
            {
                if (TalkNPCType != TargetNPCType) return false;
                // 如果启用缓存模式则只在上一次获取结果的条件不满足时重新查找
                if (CacheMode) 
                {
                    if (ForceUpdate)
                    {
                        ForceUpdate = false;
                    }
                    else 
                    {
                        if (CurrentInteraction?.Condition() is true)
                            return true;
                    }
                }


                // 重设当前交互
                CurrentInteraction = null;
                if (!Interactions.Any()) return false;
                foreach (var interaction in Interactions)
                {
                    if (interaction.Condition())
                    {
                        CurrentInteraction = interaction;
                        return true;
                    }
                }

                // 一个都没有就开溜
                return false;
            }

            public override string GetText() => CurrentInteraction?.GetText() ?? string.Empty;

            public override void Interact() => CurrentInteraction?.Interact();
        }

        public class FirstMeet : NPCInteraction
        {
            public static FirstMeet Instance { get; } = new();
            private FirstMeet() { }
            public override bool Condition() => !Main.LocalPlayer.GetModPlayer<RecorderPlayer>().MetWithRecorder;
            public override string GetText()
            {
                var text = Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.FirstMeet.2");
                int length = currentChat.Length;
                int timer = chatTimer - length - 15;
                timer /= 2;
                if (timer <= 0) return string.Empty;
                if (timer >= text.Length) return text;
                return text[..timer];
            }
            public override void Interact()
            {
                var recordPlayer = Main.LocalPlayer.GetModPlayer<RecorderPlayer>();
                recordPlayer.MetWithRecorder = true;
                var index = Item.NewItem(new EntitySource_Gift(TalkNPC), Main.LocalPlayer.Hitbox, ModContent.ItemType<Faust.Faust>());
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
                index = Item.NewItem(new EntitySource_Gift(TalkNPC), Main.LocalPlayer.Hitbox, ModContent.ItemType<ImperfectPage.ImperfectPage>());
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
                RecorderSystem.SetUnlock(ItemRecords.Faust);
                if (TalkNPC.ModNPC is Recorder recorder)
                    SetChatText(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.FirstMeet.3"));
            }
        }

        public class AskForSlime : NPCInteraction
        {
            public static AskForSlime Instance { get; } = new();
            private AskForSlime() { }

            public override bool Condition() => askForSlimeThisTime;
            public override string GetText()
            {
                if (TalkNPC.ModNPC is not Recorder recorder) return string.Empty;
                return recorder.GetLocalizedValue("FindSlime");
            }
            public override void Interact()
            {
                if (TalkNPC.ModNPC is not Recorder recorder) return;
                SetChatText(recorder.GetLocalizedValue("FindSlimeDialogue"));
                askForSlimeThisTime = false;
                askForSlimeTriggered = true;
            }
        }

        public class SlimeReward : NPCInteraction
        {
            public static SlimeReward Instance { get; } = new();
            private SlimeReward() { }

            public override bool Condition() => !RecorderSystem.CheckUnlock(ItemRecords.LordOfTheFlies) && NPC.AnyNPCs(ModContent.NPCType<DreamSlime>());
            public override string GetText()
            {
                if (TalkNPC.ModNPC is not Recorder recorder) return string.Empty;
                return $"{recorder.GetLocalizedValue("Reward")} ({Language.GetTextValue($"Mods.MatterRecord.Items.LordOfTheFlies.DisplayName")})";
            }
            public override void Interact()
            {
                SpawnItemAndUnlock(TalkNPC, ModContent.ItemType<LordOfTheFlies.LordOfTheFlies>());
            }
        }

        public class CollectingReward : NPCInteraction
        {
            public static CollectingReward Instance { get; } = new();
            private CollectingReward() { }
            public override bool Condition()
            {
                int count = RecorderSystem.GetUnlockCountWithoutRewards();
                return RewardDictionary.TryGetValue(count, out var reward) && !RecorderSystem.CheckUnlock(reward);
            }
            public override string GetText()
            {
                if (TalkNPC.ModNPC is not Recorder recorder) return string.Empty;
                int count = RecorderSystem.GetUnlockCountWithoutRewards();
                if (RewardDictionary.TryGetValue(count, out var reward) && !RecorderSystem.CheckUnlock(reward))
                    return $"{recorder.GetLocalizedValue("Reward")} ({Language.GetTextValue($"Mods.MatterRecord.Items.{reward}.DisplayName")})";
                return string.Empty;
            }
            public override void Interact()
            {
                var count = RecorderSystem.GetUnlockCountWithoutRewards();
                if (RewardDictionary.TryGetValue(count, out var reward)
                    && !RecorderSystem.CheckUnlock(reward)
                    && RecorderSystem.Instance.RecordToItemType.TryGetValue(reward, out var id))
                {
                    SpawnItemAndUnlock(TalkNPC, id);
                    return;
                }
            }
        }

        public class UnlockingRecord : NPCInteraction
        {
            public static UnlockingRecord Instance { get; } = new();
            private UnlockingRecord() { }
            public override bool Condition()
            {
                if (string.IsNullOrEmpty(cachedItemName) || (int)(Main.GlobalTimeWrappedHourly * 60) % 10 == 0)
                {
                    foreach (var item in Main.LocalPlayer.inventory)
                    {
                        if (item.ModItem is not IRecordBookItem recordBook || recordBook.IsRecordUnlocked) continue;
                        cachedItemName = item.Name;
                        cachedItemType = item.type;
                        return true;
                    }
                    cachedItemName = string.Empty;
                    cachedItemType = -1;
                }
                return cachedItemType != -1;
            }
            public override string GetText()
            {
                if (cachedItemName != null && TalkNPC.ModNPC is Recorder recorder)
                    return $"{recorder.GetLocalizedValue("Collect")} ({cachedItemName})";
                return string.Empty;
            }
            public override void Interact()
            {
                if (ContentSamples.ItemsByType[cachedItemType].ModItem is not IRecordBookItem recordBookItem) return;
                UnlockRecord(cachedItemType, recordBookItem);
            }
        }

        public class GivingLifeCrystal : NPCInteraction
        {
            public static GivingLifeCrystal Instance { get; } = new();
            private GivingLifeCrystal() { }
            public override bool Condition()
            {
                var localPlayer = Main.LocalPlayer.GetModPlayer<RecorderLocalPlayer>();
                bool hasLifeCrystal = Main.LocalPlayer.HasItem(ItemID.LifeCrystal);
                if (localPlayer.LocalData.LifeCrystalCounter == 2 && hasLifeCrystal)
                    return true;
                return false;
            }

            public override string GetText() => "给予生命水晶";

            public override void Interact()
            {
                var NPC = TalkNPC;
                var localPlayer = Main.LocalPlayer.GetModPlayer<RecorderLocalPlayer>();
                bool hasLifeCrystal = Main.LocalPlayer.HasItem(ItemID.LifeCrystal);
                if (localPlayer.LocalData.LifeCrystalCounter == 2 && hasLifeCrystal)
                {
                    int itemIndex = Main.LocalPlayer.FindItem(ItemID.LifeCrystal);
                    if (itemIndex != -1)
                    {
                        // 消耗一个生命水晶
                        Main.LocalPlayer.inventory[itemIndex].stack--;
                        if (Main.LocalPlayer.inventory[itemIndex].stack <= 0)
                            Main.LocalPlayer.inventory[itemIndex] = new Item();

                        // 播放音效
                        SoundEngine.PlaySound(SoundID.Item29, NPC.Center);

                        // 增加记录者生命值与上限
                        NPC.lifeMax += 20;
                        NPC.life += 20;
                        if (NPC.life > NPC.lifeMax) NPC.life = NPC.lifeMax;

                        // 显示绿色数字
                        CombatText.NewText(NPC.Hitbox, CombatText.HealLife, 20);

                        // 多人同步
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, NPC.whoAmI);

                        // 输出指定对话
                        SetChatText("好像变得更健康了一点点，你平时都吃这个的吗？");
                    }
                }
            }
        }

        public class RecordCollectingProgress : NPCInteraction 
        {
            public static RecordCollectingProgress Instance { get; } = new();
            private RecordCollectingProgress() { }

            public override bool Condition() => true;
            public override string GetText()
            {
                if (TalkNPC.ModNPC is not Recorder recorder) return string.Empty;
                return recorder.GetLocalization("Progress").Format($"({RecorderSystem.GetUnlockCount()}/{(int)ItemRecords.Count})");
            }
            public override void Interact()
            {
                if (TalkNPC.ModNPC is not Recorder recorder) return;
                int count = RecorderSystem.GetUnlockCount();
                KeyValuePair<int, ItemRecords>? target = null;

                foreach (var pair in RewardDictionary)
                    if (pair.Key > count && (target == null || target.Value.Key > pair.Key))
                        target = pair;

                if (target != null)
                {
                    var content = recorder.GetLocalization("HowManyRequiredForReward").Format(target.Value.Key - count);
                    SetChatText(content);
                }
                else
                {
                    var totalCount = RecorderSystem.GetUnlockCount();
                    if (totalCount == (int)ItemRecords.Count)
                    {
                        SetChatText(recorder.GetLocalizedValue("AllRecordUnlocked"));
                    }
                    else
                    {
                        var content = recorder.GetLocalization("HowManyRequiredToFullClear").Format((int)ItemRecords.Count - totalCount);
                        SetChatText(content);
                    }
                }
            }
        }
    }
}
