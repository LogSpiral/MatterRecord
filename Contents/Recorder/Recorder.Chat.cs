using MatterRecord.Contents.TheInterpretationOfDreams;
using System.Collections.Generic;
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

    private string GetSecondaryButtonText()
    {
        if (askForSlimeThisTime)
        {
            return this.GetLocalizedValue("FindSlime");
        }
        if (string.IsNullOrEmpty(cachedItemName) || (int)(Main.GlobalTimeWrappedHourly * 60) % 10 == 0)
        {
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (item.ModItem is not IRecordBookItem recordBook || recordBook.IsRecordUnlocked) continue;
                cachedItemName = item.Name;
                break;
            }
        }

        string defaultText = this.GetLocalization("Progress").Format($"({RecorderSystem.GetUnlockCount()}/{(int)ItemRecords.Count})");
        if (cachedItemName != null)
        {
            defaultText = $"{this.GetLocalizedValue("Collect")} ({cachedItemName})";
        }

        int count = RecorderSystem.GetUnlockCountWithoutRewards();
        bool findSlime = !RecorderSystem.CheckUnlock(ItemRecords.LordOfTheFlies) && NPC.AnyNPCs(ModContent.NPCType<DreamSlime>());

        if (count is 2 or 4 or 6 || findSlime)
        {
            if (count == 2 && RecorderSystem.CheckUnlock(ItemRecords.TheAdventureofSherlockHolmes)) return defaultText;
            if (count == 4 && RecorderSystem.CheckUnlock(ItemRecords.TheoryOfJustice)) return defaultText;
            if (count == 6 && RecorderSystem.CheckUnlock(ItemRecords.EmeraldTablet)) return defaultText;
            string key = count switch { 2 => "TheAdventureofSherlockHolmes", 4 => "TheoryofJustice", 6 or _ => "EmeraldTablet" };
            if (findSlime)
                key = "LordOfTheFlies";
            return $"{this.GetLocalizedValue("Reward")} ({Language.GetTextValue($"Mods.MatterRecord.Items.{key}.DisplayName")})";
        }

        return defaultText;
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
    private static string cachedItemName;
    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
            shopName = SHOPNAME;
        else
        {
            if (askForSlimeThisTime)
            {
                SetChatText(this.GetLocalizedValue("FindSlimeDialogue"));
                askForSlimeThisTime = false;
                askForSlimeTriggered = true;
                return;
            }
            var count = RecorderSystem.GetUnlockCountWithoutRewards();
            int rewardType;
            bool findSlime = !RecorderSystem.CheckUnlock(ItemRecords.LordOfTheFlies) && NPC.AnyNPCs(ModContent.NPCType<DreamSlime>());
            if (findSlime)
                rewardType = ModContent.ItemType<LordOfTheFlies.LordOfTheFlies>();
            else if (count == 2 && RecorderSystem.CheckUnlock(ItemRecords.TheAdventureofSherlockHolmes))
                rewardType = -1;
            else if (count == 4 && RecorderSystem.CheckUnlock(ItemRecords.TheoryOfJustice))
                rewardType = -1;
            else if (count == 6 && RecorderSystem.CheckUnlock(ItemRecords.EmeraldTablet))
                rewardType = -1;
            else
                rewardType = count switch
                {
                    2 => ModContent.ItemType<TheAdventureofSherlockHolmes.TheAdventureofSherlockHolmes>(),
                    4 => ModContent.ItemType<TheoryofJustice.TheoryofJustice>(),
                    6 => ModContent.ItemType<EmeraldTablet.EmeraldTablet>(),
                    _ => -1
                };
            if (rewardType != -1)
            {
                var index = Item.NewItem(new EntitySource_Gift(NPC), Main.LocalPlayer.Hitbox, rewardType);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
                if (Main.item[index].ModItem is IRecordBookItem recordBook)
                    UnlockRecord(Main.item[index].type, recordBook);
                return;
            }
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (item.ModItem is not IRecordBookItem recordBook
                    || recordBook.IsRecordUnlocked) continue;
                UnlockRecord(item.type, recordBook);

                return;
            }
            if (count < 6)
            {
                var content = this.GetLocalization("HowManyRequiredForReward").Format(count % 2 == 0 ? 2 : 1);
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
        }
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
