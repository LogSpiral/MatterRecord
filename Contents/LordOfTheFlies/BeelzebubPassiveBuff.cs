using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace MatterRecord.Contents.LordOfTheFlies
{
    public class BeelzebubPassiveSystem : ModSystem
    {
        internal static HashSet<int> AmmoProjectileTypes = new HashSet<int>();

        public override void PostSetupContent()
        {
            BuildAmmoWhitelist();
        }

        private void BuildAmmoWhitelist()
        {
            AmmoProjectileTypes.Clear();
            for (int i = 0; i < ItemLoader.ItemCount; i++)
            {
                Item item = new Item();
                item.SetDefaults(i);
                if (item.ammo != AmmoID.None)
                {
                    int projType = item.shoot;
                    if (projType > 0 && projType != ProjectileID.None)
                    {
                        AmmoProjectileTypes.Add(projType);
                    }
                }
            }
          
        }
    }

    public class BeelzebubPassiveBuff : ModPlayer
    {
        private bool _hasBeelzebub = false;

        public override void UpdateEquips()
        {
            _hasBeelzebub = false;
            int summonType = ModContent.ProjectileType<BeelzebubSummon>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == summonType)
                {
                    _hasBeelzebub = true;
                    break;
                }
            }

            if (_hasBeelzebub)
            {
                Player.AddBuff(BuffID.Hunter, 2);
                Player.AddBuff(BuffID.Dangersense, 2);
            }
        }

        public bool HasBeelzebub => _hasBeelzebub;
        public override void ResetEffects() { }
    }

    public class BeelzebubHomingProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        // 标记该射弹是否已经命中过敌人（穿透后停止追踪）
        private bool _hasHit = false;

        // 捕获命中事件
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 只要命中过就标记，无论是否是追踪射弹（但只有受我们控制的射弹会追踪，所以安全）
            _hasHit = true;
            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        public override void PostAI(Projectile projectile)
        {
            // ---- 基础筛选 ----
            if (!projectile.friendly || projectile.hostile)
                return;

            if (projectile.owner < 0 || projectile.owner >= 255)
                return;

            Player player = Main.player[projectile.owner];
            if (!player.active || player.dead)
                return;

            var buffPlayer = player.GetModPlayer<BeelzebubPassiveBuff>();
            if (!buffPlayer.HasBeelzebub)
                return;

            // 项11 进度锁：别西卜弹药追踪
            if (!LordOfTheFliesProgression.Tier11_Homing)
                return;

            // 白名单检查
            if (!BeelzebubPassiveSystem.AmmoProjectileTypes.Contains(projectile.type))
                return;

            // 伤害类型必须为远程
            if (projectile.DamageType != DamageClass.Ranged)
                return;

            // ★ 新增：若已命中过敌人，则停止追踪
            if (_hasHit)
                return;

            float speed = projectile.velocity.Length();
            if (speed < 0.5f)
                return;

            // ---- 目标查找（半径 240） ----
            NPC target = null;
            float maxDistSqr = 240f * 240f;   // 用户已改

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.CanBeChasedBy(projectile, false))
                {
                    float distSqr = projectile.DistanceSQ(npc.Center);
                    if (distSqr < maxDistSqr)
                    {
                        maxDistSqr = distSqr;
                        target = npc;
                    }
                }
            }

            if (target == null)
                return;

            // ---- 转向逻辑（角度限制） ----
            Vector2 currentDir = projectile.velocity.SafeNormalize(Vector2.Zero);
            Vector2 targetDir = (target.Center - projectile.Center).SafeNormalize(currentDir);

            float maxTurnSpeed = 0.10f;  // 可调整

            float dot = MathHelper.Clamp(Vector2.Dot(currentDir, targetDir), -1f, 1f);
            float angleDiff = (float)Math.Acos(dot);

            if (angleDiff > 0.001f)
            {
                if (angleDiff > maxTurnSpeed)
                {
                    float sign = (currentDir.X * targetDir.Y - currentDir.Y * targetDir.X) > 0 ? 1f : -1f;
                    Vector2 newDir = currentDir.RotatedBy(sign * maxTurnSpeed);
                    projectile.velocity = newDir * speed;
                }
                else
                {
                    projectile.velocity = targetDir * speed;
                }
            }
        }
    }
}