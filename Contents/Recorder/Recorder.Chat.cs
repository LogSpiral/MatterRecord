using MatterRecord.Contents.Faust;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.Localization;
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
        // TODO 找到宠物时的对话与奖励

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

    public override void SetChatButtons(ref string button, ref string button2)
    {
        if (chatTimer == 0 || (int)(Main.GlobalTimeWrappedHourly * 60) % 2 == 0)
            chatTimer++;
        if (currentChat != null)
        {
            int length = currentChat.Length;

            if (chatTimer <= length)
                Main.npcChatText = currentChat[..chatTimer];
        }
        button = this.GetLocalizedValue("Copy");
        button2 = this.GetLocalizedValue("Collect");
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
            shopName = SHOPNAME;
        else
        {
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (item.ModItem is not IRecordBookItem recordBook
                    || recordBook.IsRecordUnlocked) continue;
                UnlockRecord(item.type, recordBook);

                break;
            }
        }
    }
}
