using System.Collections.Generic;
using Terraria.Localization;
using Terraria.Utilities;

namespace MatterRecord.Contents.Recorder;

public partial class Recorder
{
    public override string GetChat()
    {
        // TODO 完整的对话注册
        WeightedRandom<string> chat = new WeightedRandom<string>();
        chat.Add(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.StandardDialogue1"));
        chat.Add(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.StandardDialogue2"));
        chat.Add(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.StandardDialogue3"));
        chat.Add(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.StandardDialogue4"));
        return chat;
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = "我去";
        button2 = "太草了！";
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        Main.NewText("何意味何意味何意味");
    }
}
