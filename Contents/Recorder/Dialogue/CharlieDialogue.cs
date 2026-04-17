using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using MatterRecord.Contents.LordOfTheFlies;

namespace MatterRecord.Contents.Recorder.Dialogue
{
    public class CharlieDialogue : DialogueModule
    {
        private readonly RecorderLocalPlayer _player;
        private int _lastHitTimer;
        private bool _justTriggered;

        public override bool IsActive => false;

        public CharlieDialogue(RecorderLocalPlayer player)
        {
            _player = player;
        }

        public override void OnEnterWorld(Player player, LocalTriggerData data)
        {
            _lastHitTimer = 0;
            _justTriggered = false;
        }

        public override void PostUpdate(Player player, LocalTriggerData data)
        {
            // 必须满足：调频已完成 + 记录者存活 + 计数器未满（<6）
            if (data.TuningCounter != 2 || _player.GetFirstRecorder() == null || data.CharlieCounter >= 6)
                return;

            int currentHitTimer = DontStarveDarknessDamageDealer.darknessHitTimer;
            bool willHurt = currentHitTimer >= 35 && !player.immune;

            if (willHurt && _lastHitTimer < 55 && !_justTriggered)
            {
                TriggerCharlie(player, data);
                _justTriggered = true;
            }
            else if (!willHurt)
            {
                _justTriggered = false;
            }

            _lastHitTimer = currentHitTimer;
        }

        public override void OnHurt(Player player, LocalTriggerData data, Player.HurtInfo info) { }

        private void TriggerCharlie(Player player, LocalTriggerData data)
        {
            data.CharlieCounter++;
            _player.SaveData();

            string content = data.CharlieCounter switch
            {
                1 => "看来有人出门忘了带火把？",
                2 => "真是没有记性的家伙",
                3 => "我说，你不会是故意的吧",
                4 => "这不好玩",
                5 => "……",
                6 => "湮灭弹储备归零，祝你好运",
                _ => ""
            };
            if (!string.IsNullOrEmpty(content))
                _player.SendMessageWithBubble("‹乐九›：", content);

            SpawnCharlieProjectile(player);
        }

        private void SpawnCharlieProjectile(Player player)
        {
            NPC recorder = _player.GetFirstRecorder();
            if (recorder == null) return;

            Vector2 dirToRecorder = recorder.Center - player.Center;
            if (dirToRecorder == Vector2.Zero) return;
            dirToRecorder.Normalize();

            Vector2 screenPlayerPos = player.Center - Main.screenPosition;
            float screenW = Main.screenWidth;
            float screenH = Main.screenHeight;
            Vector2 step = dirToRecorder;
            float tMin = float.MaxValue;
            Vector2? intersect = null;

            // 与左边界
            if (step.X != 0)
            {
                float t = -screenPlayerPos.X / step.X;
                if (t > 0)
                {
                    float y = screenPlayerPos.Y + t * step.Y;
                    if (y >= 0 && y <= screenH) { tMin = t; intersect = new Vector2(0, y); }
                }
            }
            // 与右边界
            if (step.X != 0)
            {
                float t = (screenW - screenPlayerPos.X) / step.X;
                if (t > 0)
                {
                    float y = screenPlayerPos.Y + t * step.Y;
                    if (y >= 0 && y <= screenH) { if (t < tMin) { tMin = t; intersect = new Vector2(screenW, y); } }
                }
            }
            // 与上边界
            if (step.Y != 0)
            {
                float t = -screenPlayerPos.Y / step.Y;
                if (t > 0)
                {
                    float x = screenPlayerPos.X + t * step.X;
                    if (x >= 0 && x <= screenW) { if (t < tMin) { tMin = t; intersect = new Vector2(x, 0); } }
                }
            }
            // 与下边界
            if (step.Y != 0)
            {
                float t = (screenH - screenPlayerPos.Y) / step.Y;
                if (t > 0)
                {
                    float x = screenPlayerPos.X + t * step.X;
                    if (x >= 0 && x <= screenW) { if (t < tMin) { tMin = t; intersect = new Vector2(x, screenH); } }
                }
            }

            if (!intersect.HasValue) return;

            Vector2 spawnScreen = intersect.Value + step * 160;
            Vector2 spawnWorld = Main.screenPosition + spawnScreen;
            Vector2 velocity = -step * 50f;

            int projType = ModContent.ProjectileType<AnnihilationBullet>();
            Projectile.NewProjectile(player.GetSource_FromThis(), spawnWorld, velocity, projType, 0, 0, player.whoAmI);
        }
    }
}