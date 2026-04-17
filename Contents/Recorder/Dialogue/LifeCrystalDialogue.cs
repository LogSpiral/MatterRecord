using Terraria;
using Terraria.ID;

namespace MatterRecord.Contents.Recorder.Dialogue
{
    public class LifeCrystalDialogue : DialogueModule
    {
        private readonly RecorderLocalPlayer _player;
        private int _stage;
        private int _timer;
        private int _targetDelay;
        private bool _prevControlUseItem;

        public override bool IsActive => _stage != 0;

        public LifeCrystalDialogue(RecorderLocalPlayer player)
        {
            _player = player;
        }

        public override void OnEnterWorld(Player player, LocalTriggerData data)
        {
            _prevControlUseItem = false;
        }

        public override void PostUpdate(Player player, LocalTriggerData data)
        {
            if (data.TuningCounter == 2 && data.LifeCrystalCounter == 0 && _stage == 0 && !_player.AnyModuleActive)
            {
                if (player.controlUseItem && player.HeldItem.type == ItemID.LifeCrystal && player.statLifeMax2 >= 400)
                {
                    if (!_prevControlUseItem) TryStart(data);
                }
                _prevControlUseItem = player.controlUseItem;
            }

            if (_stage == 0) return;
            _timer++;
            if (_timer >= _targetDelay)
            {
                switch (_stage)
                {
                    case 1:
                        _player.SendMessageWithBubble("‹乐九›：", "看起来你似乎已经达到了某种上限，如果不介意的话，可以分给我吗？");
                        _stage = 2;
                        _timer = 0;
                        _targetDelay = 120;
                        break;
                    case 2:
                        _player.SendMessageWithBubble("‹乐九›：", "对话的时候递给我就行");
                        _stage = 3;
                        _timer = 0;
                        _targetDelay = 0;
                        break;
                    case 3:
                        _stage = 0;
                        data.LifeCrystalCounter = 2;
                        _player.SaveData();
                        break;
                }
            }
        }

        public override void OnHurt(Player player, LocalTriggerData data, Player.HurtInfo info) { }

        private void TryStart(LocalTriggerData data)
        {
            data.LifeCrystalCounter = 1;
            _player.SaveData();
            _stage = 1;
            _timer = 0;
            _targetDelay = Main.rand.Next(180, 301);
        }
    }
}