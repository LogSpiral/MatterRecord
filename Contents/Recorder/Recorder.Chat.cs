using MatterRecord.Contents.TheAdventureofSherlockHolmes;
using MatterRecord.Contents.TheInterpretationOfDreams;
using MatterRecord.Contents.TheoryOfFreedom;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
namespace MatterRecord.Contents.Recorder;

public partial class Recorder
{
    private static string UnlockRecord(int type, IRecordBookItem recordBook)
    {
        static string Dialogue(string key) => Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.{key}");
        RecorderSystem.SetUnlock(recordBook);
        string chatResult = Dialogue(recordBook.RecordType.ToString() + "Unlocking");
        currentChat = "...";
        chatTimer = 0;
        Main.npcChatText = "...";
        RecorderSystem.StartUnlockAnimation(type);
        RecorderSystem.OnEndAnimation = delegate
        {
            currentChat = chatResult;
            chatTimer = 0;
        };
        return chatResult;
    }

    private static string currentChat;
    private static int chatTimer;
    private static bool askForSlimeThisTime;
    private static bool askForSlimeTriggered;
    private static string cachedItemName;

    public override string GetChat()
    {
        chatTimer = 0;
        string chatResult;
        static string Dialogue(string key) => Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.{key}");
        static string DialogueWithArgs(string key, params object[] args) => Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.{key}", args);
        var recordPlayer = Main.LocalPlayer.GetModPlayer<RecorderPlayer>();
        // 首次见面奖励
        if (!recordPlayer.MetWithRecorder)
        {
            recordPlayer.MetWithRecorder = true;
            var index = Item.NewItem(new EntitySource_Gift(NPC), Main.LocalPlayer.Hitbox, ModContent.ItemType<Faust.Faust>());
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
            chatResult = DialogueWithArgs("FirstMeet", NPC.GivenName);
            currentChat = chatResult;
            return " ";
        }
        askForSlimeThisTime = !DreamWorld.UsedZoologistDream && Main.rand.NextBool(10);
        // 解锁物品
        List<IRecordBookItem> recordBooks = [];
        foreach (var item in Main.LocalPlayer.inventory)
        {
#if false
            if (item.ModItem is IRecordBookItem recordBook)
            {
                if (!recordBook.IsRecordUnlocked)
                {
                    chatResult = UnlockRecord(recordBook);
                    return " ";
                }
                else if(recordBook is not Faust.Faust)
                    recordBooks.Add(recordBook);
            }
#else
            if (item.ModItem is IRecordBookItem { RecordType: not ItemRecords.Faust } recordBook
                && recordBook.IsRecordUnlocked)
                recordBooks.Add(recordBook);
#endif
        }

        // 闲聊
        WeightedRandom<string> chat = new();
        chat.Add(DialogueWithArgs("StandardDialogue1", Main.LocalPlayer.name));
        chat.Add(Dialogue("StandardDialogue2"));
        chat.Add(Dialogue("StandardDialogue3"));
        chat.Add(Dialogue("StandardDialogue4"));

        // 特殊物品闲聊
        foreach (var recordBook in recordBooks)
            chat.Add(Dialogue(recordBook.RecordType.ToString() + "Unlocked"));

        // NPC相关闲聊
        int guideIndex = NPC.FindFirstNPC(NPCID.Guide);
        if (guideIndex != -1)
            chat.Add(DialogueWithArgs("GuideDialogue", Main.npc[guideIndex].GivenName));

        int anglerIndex = NPC.FindFirstNPC(NPCID.Angler);
        if (anglerIndex != -1)
            chat.Add(Dialogue("AnglerDialogue"));
        chatResult = chat.Get();
        currentChat = chatResult;
        return " ";
    }
    private static Dictionary<int, ItemRecords> RewardDictionary => RecorderSystem.RewardDictionary;
    private string GetSecondaryButtonText()
    {
        #region 请求寻找宠物史莱姆
        if (askForSlimeThisTime)
        {
            return this.GetLocalizedValue("FindSlime");
        }
        #endregion

        #region 找到史莱姆奖励
        bool findSlime = !RecorderSystem.CheckUnlock(ItemRecords.LordOfTheFlies) && NPC.AnyNPCs(ModContent.NPCType<DreamSlime>());
        if (findSlime)
        {
            return $"{this.GetLocalizedValue("Reward")} ({Language.GetTextValue($"Mods.MatterRecord.Items.LordOfTheFlies.DisplayName")})";
        }
        #endregion

        #region 收集道具奖励
        int count = RecorderSystem.GetUnlockCountWithoutRewards();
        if (RewardDictionary.TryGetValue(count, out var reward))
        {
            if (!RecorderSystem.CheckUnlock(reward))
                return $"{this.GetLocalizedValue("Reward")} ({Language.GetTextValue($"Mods.MatterRecord.Items.{reward}.DisplayName")})";
        }
        #endregion

        #region 解锁道具
        if (string.IsNullOrEmpty(cachedItemName) || (int)(Main.GlobalTimeWrappedHourly * 60) % 10 == 0)
        {
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (item.ModItem is not IRecordBookItem recordBook || recordBook.IsRecordUnlocked) continue;
                cachedItemName = item.Name;
                break;
            }
        }
        if (cachedItemName != null)
            return $"{this.GetLocalizedValue("Collect")} ({cachedItemName})";
        #endregion

        #region 收集进度文本
        return this.GetLocalization("Progress").Format($"({RecorderSystem.GetUnlockCount()}/{(int)ItemRecords.Count})");
        #endregion
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        if (chatTimer == 0)
            cachedItemName = null;
        if (chatTimer == 0 || (int)(Main.GlobalTimeWrappedHourly * 60) % 2 == 0)
            chatTimer++;
        if (currentChat != null)
        {
            int length = currentChat.Length;

            if (chatTimer <= length)
                Main.npcChatText = currentChat[..chatTimer];
        }
        button = this.GetLocalizedValue("Copy");
        button2 = GetSecondaryButtonText();
    }
    private void OnSecondaryButtonClicked()
    {
        var type = ModContent.ItemType<TheAdventureofSherlockHolmes.TheAdventureofSherlockHolmes>();
        var typ2 = ModContent.ItemType<TheoryOfFreedom.TheoryOfFreedom>();

        #region 辅助函数
        static void SpawnItemAndUnlock(NPC npc, int itemType)
        {
            var index = Item.NewItem(new EntitySource_Gift(npc), Main.LocalPlayer.Hitbox, itemType);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
            if (Main.item[index].ModItem is IRecordBookItem recordBook)
                UnlockRecord(Main.item[index].type, recordBook);
        }
        #endregion

        #region 请求寻找宠物史莱姆
        if (askForSlimeThisTime)
        {
            SetChatText(this.GetLocalizedValue("FindSlimeDialogue"));
            askForSlimeThisTime = false;
            askForSlimeTriggered = true;
            return;
        }
        #endregion

        #region 找到史莱姆奖励
        if (!RecorderSystem.CheckUnlock(ItemRecords.LordOfTheFlies) && NPC.AnyNPCs(ModContent.NPCType<DreamSlime>()))
        {
            SpawnItemAndUnlock(NPC, ModContent.ItemType<LordOfTheFlies.LordOfTheFlies>());
            return;
        }
        #endregion

        #region 收集道具奖励
        var count = RecorderSystem.GetUnlockCountWithoutRewards();
        if (RewardDictionary.TryGetValue(count, out var reward) && !RecorderSystem.CheckUnlock(reward) && RecorderSystem.Instance.RecordToItemType.TryGetValue(reward, out var id))
        {
            SpawnItemAndUnlock(NPC, id);
            return;
        }

        #endregion

        #region 解锁道具
        foreach (var item in Main.LocalPlayer.inventory)
        {
            if (item.ModItem is not IRecordBookItem recordBook
                || recordBook.IsRecordUnlocked) continue;
            UnlockRecord(item.type, recordBook);
            return;
        }
        #endregion

        #region 收集进度文本

        KeyValuePair<int, ItemRecords>? target = null;

        foreach (var pair in RewardDictionary)
            if (pair.Key > count && (target == null || target.Value.Key > pair.Key))
                target = pair;

        if (target != null)
        {
            var content = this.GetLocalization("HowManyRequiredForReward").Format(target.Value.Key - count);
            SetChatText(content);
        }
        else
        {
            var totalCount = RecorderSystem.GetUnlockCount();
            if (totalCount == (int)ItemRecords.Count)
            {
                SetChatText(this.GetLocalizedValue("AllRecordUnlocked"));
            }
            else
            {
                var content = this.GetLocalization("HowManyRequiredToFullClear").Format((int)ItemRecords.Count - totalCount);
                SetChatText(content);
            }
        }
        #endregion

    }
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
            shopName = SHOPNAME;
        else
            OnSecondaryButtonClicked();
    }
    private void SetChatText(string content)
    {
        currentChat = content;
        chatTimer = 0;
        Main.npcChatText = " ";
    }
    public override void SaveData(TagCompound tag)
    {
        tag["s"] = askForSlimeTriggered;
    }
    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet<bool>("s", out var flag))
            askForSlimeTriggered = flag;
        else
            askForSlimeTriggered = false;
    }
}
