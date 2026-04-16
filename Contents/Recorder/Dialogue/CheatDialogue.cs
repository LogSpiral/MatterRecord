using Terraria;
using Terraria.ID;

namespace MatterRecord.Contents.Recorder.Dialogue
{
    public class CheatDialogue : DialogueModule
    {
        private readonly RecorderLocalPlayer _player;
        private int _stage;
        private int _timer;
        private int _firstDelay;

        public override bool IsActive => _stage != 0;

        public CheatDialogue(RecorderLocalPlayer player)
        {
            _player = player;
        }

        public override void OnEnterWorld(Player player, LocalTriggerData data)
        {
            string currentWorld = Main.worldName;
            bool isMultiplayer = Main.netMode == NetmodeID.MultiplayerClient;
            bool cheatModsEnabled = _player.AreCheatModsEnabled();

            if (isMultiplayer && data.CheatCounter == 0 && data.CheatWorldName != currentWorld && !cheatModsEnabled)
            {
                data.CheatCounter = 1;
                data.CheatWorldName = currentWorld;
                _player.SaveData();
            }
            else if (!isMultiplayer && data.CheatCounter == 1 && _player.GetFirstRecorder() != null && cheatModsEnabled && data.TuningCounter == 2)
            {
                TryStart();
            }
        }

        public override void PostUpdate(Player player, LocalTriggerData data)
        {
            if (_stage == 0) return;
            _timer++;
            if (_stage == 1 && _timer >= _firstDelay)
            {
                _player.SendMessageWithBubble("‹乐九›：", "嗯哼？");
                _stage = 2;
                _timer = 0;
            }
            else if (_stage == 2 && _timer >= 120)
            {
                _player.SendMessageWithBubble("‹乐九›：", "在偷偷拿什么呢？");
                _stage = 0;
                data.CheatCounter = 2;
                _player.SaveData();
            }
        }

        public override void OnHurt(Player player, LocalTriggerData data, Player.HurtInfo info) { }

        private void TryStart()
        {
            if (!_player.AnyModuleActive)
            {
                _stage = 1;
                _firstDelay = Main.rand.Next(180, 301);
            }
        }
    }
}