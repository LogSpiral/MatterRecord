using MatterRecord.Contents.ImperfectPage;
using MatterRecord.Contents.TheInterpretationOfDreams;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.Utilities;
namespace MatterRecord.Contents.Recorder;

public partial class Recorder
{
    private static LocalizedText CopyShopText { get; set; }
    private void InitializeLocalization()
    {
        CopyShopText = this.GetLocalization("Copy");
    }

    private static Interactions.MultiInteraction MultiInteraction { get; set; }
    private static void InteractionRegister()
    {
        MultiInteraction ??= new Interactions.MultiInteraction(
                ModContent.NPCType<Recorder>(),
                [
                    Interactions.FirstMeet.Instance,
                    Interactions.AskForSlime.Instance,
                    Interactions.SlimeReward.Instance,
                    Interactions.CollectingReward.Instance,
                    Interactions.UnlockingRecord.Instance,
                    Interactions.RecordCollectingProgress.Instance
                ]);
        NPCInteractions.All.Add(Interactions.Copy.Instance);
        NPCInteractions.All.Add(MultiInteraction);
    }
    private static string UnlockRecord(int type, IRecordBookItem recordBook, bool fromReward = false)
    {
        static string Dialogue(string key) => Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.{key}");
        RecorderSystem.SetUnlock(recordBook);
        string chatResult;
        if (fromReward && recordBook.RecordType == ItemRecords.LordOfTheFlies)
            chatResult = Dialogue("PetFoundDialogue");
        else
            chatResult = Dialogue(recordBook.RecordType.ToString() + "Unlocking");
        currentChat = "...";
        chatTimer = 0;
        Main.npcChatText = "...";
        RecorderSystem.StartUnlockAnimation(type);
        RecorderSystem.OnEndAnimation = delegate
        {
            currentChat = chatResult;
            chatTimer = 0;
        };
        MultiInteraction.ForceUpdate = true;
        return chatResult;
    }

    private static string currentChat;
    private static int chatTimer;
    private static bool askForSlimeThisTime;
    private static bool askForSlimeTriggered;
    private static string cachedItemName;
    private static int cachedItemType;

    public override string GetChat()
    {
        MultiInteraction?.ForceUpdate = true;
        chatTimer = 0;
        string chatResult;
        static string Dialogue(string key) => Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.{key}");
        static string DialogueWithArgs(string key, params object[] args) => Language.GetTextValue($"Mods.MatterRecord.Dialogue.Recorder.{key}", args);
        var recordPlayer = Main.LocalPlayer.GetModPlayer<RecorderPlayer>();
        // 首次见面奖励
        if (!recordPlayer.MetWithRecorder)
        {
            chatResult = DialogueWithArgs("FirstMeet.1", NPC.GivenName);
            currentChat = chatResult;
            return " ";
        }
        askForSlimeThisTime = 
            !DreamWorld.UsedZoologistDream 
            && !RecorderSystem.CheckUnlock(ItemRecords.LordOfTheFlies) 
            && Main.rand.NextBool(10) 
            && !NPC.AnyNPCs(ModContent.NPCType<DreamSlime>());


        #region 闲聊
        WeightedRandom<string> chat = new();
        chat.Add(DialogueWithArgs("StandardDialogue1", Main.LocalPlayer.name));
        chat.Add(Dialogue("StandardDialogue2"));
        chat.Add(Dialogue("StandardDialogue3"));
        chat.Add(Dialogue("StandardDialogue4"));
        #endregion

        #region 特殊物品闲聊
        foreach (var item in Main.LocalPlayer.inventory)
        {
            if (item.ModItem is IRecordBookItem { RecordType: not ItemRecords.Faust } recordBook
                && recordBook.IsRecordUnlocked)
            {
                if (recordBook.RecordType is ItemRecords.CompendiumOfMateriaMedica)
                    chat.Add(DialogueWithArgs(recordBook.RecordType.ToString() + "Unlocked", Main.LocalPlayer.name));
                else
                    chat.Add(Dialogue(recordBook.RecordType.ToString() + "Unlocked"));
            }
        }
        #endregion

        #region NPC相关闲聊
        int guideIndex = NPC.FindFirstNPC(NPCID.Guide);
        if (guideIndex != -1)
            chat.Add(DialogueWithArgs("GuideDialogue", Main.npc[guideIndex].GivenName));

        int anglerIndex = NPC.FindFirstNPC(NPCID.Angler);
        if (anglerIndex != -1)
            chat.Add(DialogueWithArgs("AnglerDialogue", Main.npc[anglerIndex].GivenName));

        int pirateIndex = NPC.FindFirstNPC(NPCID.Pirate);
        if (pirateIndex != -1 && anglerIndex != -1)
            chat.Add(DialogueWithArgs("PirateDialogue", Main.npc[pirateIndex].GivenName, Main.npc[anglerIndex].GivenName));

        int princessIndex = NPC.FindFirstNPC(NPCID.Princess);
        if (princessIndex != -1)
            chat.Add(DialogueWithArgs("PrincessDialogue", Main.npc[princessIndex].GivenName));

        #endregion

        chatResult = chat.Get();
        currentChat = chatResult;
        return " ";
    }
    private static Dictionary<int, ItemRecords> RewardDictionary => RecorderSystem.RewardDictionary;

    private static void UpdateChat()
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
    }

    private static void SpawnItemAndUnlock(NPC npc, int itemType)
    {
        var index = Item.NewItem(new EntitySource_Gift(npc), Main.LocalPlayer.Hitbox, itemType);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
        if (Main.item[index].ModItem is IRecordBookItem recordBook)
            UnlockRecord(Main.item[index].type, recordBook, true);
    }
    private static void SetChatText(string content)
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
