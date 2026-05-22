using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.EasterEgg
{
    public class TorchGodPlayer : ModPlayer
    {
        private bool isInDialogue = false;
        private int dialogueStep = 0;
        private int dialogueTimer = 0;
        private Vector2 dialoguePosition;
        private Vector2 dialogueStartPosition;
        private int initialDelay = 150;
        private bool spawnedEffect = false;
        private int projectionID = -1;
        private int checkTimer = 0;
        private const float maxDialogueDistance = 1000f;

        // 反射字段（原版私有/公共字段）
        private static FieldInfo _torchGodCooldown;
        private static FieldInfo _numberOfTorchAttacksMade;
        private static FieldInfo _happyFunTorchTime;

        static TorchGodPlayer()
        {
            Type playerType = typeof(Player);
            _torchGodCooldown = playerType.GetField("torchGodCooldown", BindingFlags.NonPublic | BindingFlags.Instance);
            _numberOfTorchAttacksMade = playerType.GetField("numberOfTorchAttacksMade", BindingFlags.NonPublic | BindingFlags.Instance);
            _happyFunTorchTime = playerType.GetField("happyFunTorchTime", BindingFlags.Public | BindingFlags.Instance);
        }

        public override void PostUpdate()
        {
            if (Main.myPlayer != Player.whoAmI) return;

            if (isInDialogue)
            {
                _happyFunTorchTime?.SetValue(Player, false);
                _torchGodCooldown?.SetValue(Player, 100);

                if (Player.dead || !Player.active || Vector2.Distance(Player.Center, dialogueStartPosition) > maxDialogueDistance)
                {
                    CleanupDialogue(false);
                    return;
                }

                dialoguePosition = Player.Center + new Vector2(300f, 4f);
                Lighting.AddLight(dialoguePosition, 0.9f, 0.7f, 0.3f);
                Lighting.AddLight(Player.Center, 0.9f, 0.7f, 0.3f);

                if (!spawnedEffect)
                {
                    SpawnSmokeEffect(dialoguePosition);
                    spawnedEffect = true;
                }

                if (dialogueStep == 0 && initialDelay > 0)
                {
                    initialDelay--;
                    return;
                }

                if (dialogueTimer > 0)
                {
                    dialogueTimer--;
                    return;
                }

                string text = GetDialogueText(dialogueStep);
                if (text != null)
                {
                    Rectangle targetRect = dialogueStep < 2 ? Player.getRect() : new Rectangle((int)dialoguePosition.X, (int)dialoguePosition.Y, 32, 32);
                    Color textColor = dialogueStep < 2 ? Color.White : Color.Orange;
                    CombatText.NewText(targetRect, textColor, text);
                    dialogueTimer = 150;
                    dialogueStep++;
                }
                else
                {
                    CleanupDialogue(true);
                }
                return;
            }

            if (++checkTimer >= 120)
            {
                checkTimer = 0;
                CheckAndStartDialogue();
            }
        }

        private string GetDialogueText(int index) => index switch
        {
            0 => "嘿火把神，你动不动就转换火把的日子结束了",
            1 => "把徽章给我",
            2 => "想要的话，你得自己来拿",
            3 => "这规矩你早就懂的",
            _ => null
        };

        private void SpawnSmokeEffect(Vector2 position)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(position - new Vector2(8, 8), 16, 16, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                dust.noGravity = true;
            }
        }

        private void StartDialogue()
        {
            if (isInDialogue) return;

            dialogueStartPosition = Player.Center;
            dialoguePosition = Player.Center + new Vector2(300f, 4f);
            isInDialogue = true;
            dialogueStep = 0;
            dialogueTimer = 0;
            initialDelay = 150;
            spawnedEffect = false;

            Player.AddBuff(BuffID.Blackout, 600);

            if (Main.netMode != NetmodeID.Server)
            {
                projectionID = Projectile.NewProjectile(Player.GetSource_FromThis(), dialoguePosition, Vector2.Zero,
                    ModContent.ProjectileType<TorchGodProjection>(), 0, 0, Player.whoAmI);
                if (projectionID >= 0 && projectionID < Main.maxProjectiles)
                {
                    Main.projectile[projectionID].netImportant = false;
                    SpawnSmokeEffect(dialoguePosition);
                }
            }
        }

        private void CleanupDialogue(bool triggerEvent)
        {
            if (!isInDialogue) return;

            isInDialogue = false;
            Player.ClearBuff(BuffID.Blackout);

            if (projectionID >= 0 && Main.projectile[projectionID].active)
            {
                SpawnSmokeEffect(Main.projectile[projectionID].Center);
                Main.projectile[projectionID].active = false;
            }
            projectionID = -1;

            if (triggerEvent)
            {
                _happyFunTorchTime?.SetValue(Player, true);
                _numberOfTorchAttacksMade?.SetValue(Player, 0);
                _torchGodCooldown?.SetValue(Player, 0);
            }
            else
            {
                _happyFunTorchTime?.SetValue(Player, false);
                _torchGodCooldown?.SetValue(Player, 0);
            }
        }

        private void CheckAndStartDialogue()
        {
            if (Player.dead || !Player.active || isInDialogue) return;

            bool inEvent = (bool)(_happyFunTorchTime?.GetValue(Player) ?? false);
            if (inEvent) return;

            int torchGodCooldown = (int)(_torchGodCooldown?.GetValue(Player) ?? 0);
            if (torchGodCooldown > 0) return;

            if (Player.unlockedBiomeTorches) return;
            if (!(Player.position.Y > Main.worldSurface * 16.0)) return;

            bool hasFavor = false;
            for (int i = 0; i < 58; i++)
            {
                if (Player.inventory[i].type == ItemID.TorchGodsFavor)
                {
                    hasFavor = true;
                    break;
                }
            }
            if (hasFavor) return;

            if (CountNearbyTorches() <= 100) return;

            var today = DateTime.Today;
            bool isJune15 = today.Month == 6 && today.Day == 15;

            if (isJune15)
            {
                StartDialogue();
            }
            else if (EasterEggSystem.GetData().TorchGodEasterEggTriggered)
            {
                StartDialogue();
                EasterEggSystem.ResetTorchGodTrigger();
            }
        }

        private int CountNearbyTorches()
        {
            int count = 0, range = 40;
            int cx = (int)(Player.Center.X / 16), cy = (int)(Player.Center.Y / 16);
            int minX = Math.Max(cx - range, 10), maxX = Math.Min(cx + range, Main.maxTilesX - 10);
            int minY = Math.Max(cy - range, 10), maxY = Math.Min(cy + range, Main.maxTilesY - 10);

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile != null && tile.HasTile && TileID.Sets.Torch[tile.TileType])
                        count++;
                }
            return count;
        }

        public bool IsInDialogue() => isInDialogue;
        public Vector2 GetDialoguePosition() => dialoguePosition;

        public override void OnEnterWorld()
        {
            isInDialogue = false;
            dialogueStep = dialogueTimer = checkTimer = 0;
            if (projectionID >= 0 && Main.projectile[projectionID].active)
                Main.projectile[projectionID].active = false;
            projectionID = -1;
            Player.ClearBuff(BuffID.Blackout);
            _happyFunTorchTime?.SetValue(Player, false);
            _torchGodCooldown?.SetValue(Player, 0);
        }
    }

    public class TorchGodProjection : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = false;
            Projectile.timeLeft = 2;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            var modPlayer = Main.player[Projectile.owner].GetModPlayer<TorchGodPlayer>();
            if (modPlayer?.IsInDialogue() == true)
            {
                Projectile.Center = modPlayer.GetDialoguePosition();
                Projectile.timeLeft = 2;
            }
            else
            {
                Projectile.active = false;
            }

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(1f, 1f);
            }
        }

        public override bool? CanDamage() => false;
    }
}