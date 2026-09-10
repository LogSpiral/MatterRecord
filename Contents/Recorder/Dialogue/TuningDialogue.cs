using Terraria;
using Terraria.Localization;

namespace MatterRecord.Contents.Recorder.Dialogue
{
    public class TuningDialogue : DialogueModule
    {
        private readonly RecorderLocalPlayer _player;
        private int _stage;
        private int _timer;
        private int _targetDelay;
        private bool _wasDayTime;

        public override bool IsActive => _stage != 0;

        public TuningDialogue(RecorderLocalPlayer player)
        {
            _player = player;
        }

        public override void OnEnterWorld(Player player, LocalTriggerData data)
        {
            _wasDayTime = Main.dayTime;
        }

        public override void PostUpdate(Player player, LocalTriggerData data)
        {
            bool justEnteredNight = _wasDayTime && !Main.dayTime;
            _wasDayTime = Main.dayTime;

            if (justEnteredNight && data.TuningCounter == 0 && _stage == 0 && !_player.AnyModuleActive)
            {
                if (Main.rand.NextBool(2))
                {
                    data.TuningCounter = 1;
                    _player.SaveData();
                    _stage = 1;
                    _timer = 0;
                    _targetDelay = Main.rand.Next(900, 2101);
                }
            }

            if (_stage == 0) return;
            _timer++;
            if (_timer >= _targetDelay)
            {
                switch (_stage)
                {
                    case 1:
                        _player.SendMessageWithBubble(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.MysteryBubble"), Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.TuningHello"));
                        _stage = 2;
                        _timer = 0;
                        _targetDelay = Main.rand.Next(180, 301);
                        break;
                    case 2:
                        _player.SendMessageWithBubble(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.MysteryBubble"), Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.TuningLink"));
                        _stage = 3;
                        _timer = 0;
                        _targetDelay = 120;
                        break;
                    case 3:
                        string playerName = Main.LocalPlayer.name;
                        _player.SendMessageWithBubble(Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.NinaBubble"), Language.GetTextValue("Mods.MatterRecord.Dialogue.Recorder.TuningEvening", playerName));
                        _stage = 4;
                        _timer = 0;
                        _targetDelay = 0;
                        break;
                    case 4:
                        _stage = 0;
                        data.TuningCounter = 2;
                        _player.SaveData();
                        break;
                }
            }
        }

        public override void OnHurt(Player player, LocalTriggerData data, Player.HurtInfo info) { }
    }
}