using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.LordOfTheFlies
{
    public class BeelzebubSummon : ModProjectile
    {
        private static readonly Vector2 _origin = new Vector2(20, 19f);
        private static readonly Vector2 _muzzleOffset = new Vector2(20, -0.5f);

        private const float TargetDist = 256f;
        private const float Tolerance = 30f;
        private const float TeleportDistance = 1400f;
        private const float TeleportRadius = 1000f;
        private const float ReturnThreshold = 80f;
        private const float AttackRange = 320f;

        private float _wobbleAngle = 0f;
        private float _wobbleTimer = 0f;
        private float _floatOffsetY = 0f;
        private bool ignoreSolid = false;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 1;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.MinionTargetingFeature[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 38;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.minionSlots = 1f;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        private int ComputeWeaponDamage(Player player)
        {
            float rangeFactor = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
            float genericFactor = player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);
            rangeFactor -= genericFactor;
            genericFactor += rangeFactor - 1;

            float critFactor = player.GetTotalCritChance(DamageClass.Ranged) * 0.01f;
            critFactor += 0.04f;

            int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;
            defense += player.GetModPlayer<LordOfTheFliesPlayer>().GetDefenseBonus();

            float multiplier = Math.Max((defense / 1.15f) * (0.75f + rangeFactor + critFactor) / (1 + genericFactor * 0.5f), 1);
            return (int)(1 * multiplier);
        }

        private int ComputeWeaponUseTime(Player player)
        {
            float rangeMultiplier = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
            float rangeBonus = rangeMultiplier - 1f;
            float targetFrames = 20f / (1f + rangeBonus * 1.5f);
            targetFrames = MathHelper.Clamp(targetFrames, 6f, 20f);
            return (int)Math.Round(targetFrames);
        }

        private void GetAmmoTypes(Player player, out int primaryType, out int secondaryType)
        {
            primaryType = ProjectileID.Bullet;
            secondaryType = 0;
            bool foundFirst = false;

            for (int i = 54; i <= 57 && i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item != null && !item.IsAir && item.ammo == AmmoID.Bullet)
                {
                    if (!foundFirst)
                    {
                        primaryType = item.shoot;
                        foundFirst = true;
                    }
                    else
                    {
                        secondaryType = item.shoot;
                        return;
                    }
                }
            }

            for (int i = 0; i < 54 && i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item != null && !item.IsAir && item.ammo == AmmoID.Bullet)
                {
                    if (!foundFirst)
                    {
                        primaryType = item.shoot;
                        foundFirst = true;
                    }
                    else
                    {
                        secondaryType = item.shoot;
                        return;
                    }
                }
            }
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }

            if (owner.HeldItem.type == ModContent.ItemType<LordOfTheFlies>())
            {
                Projectile.Kill();
                return;
            }

            float distToPlayer = Vector2.Distance(Projectile.Center, owner.Center);
            if (distToPlayer > TeleportDistance)
            {
                Vector2 direction = Vector2.Normalize(Projectile.Center - owner.Center);
                Vector2 teleportPos = owner.Center + direction * TeleportRadius;
                Projectile.Center = teleportPos;
                Projectile.velocity = Vector2.Zero;
                ignoreSolid = true;
                Projectile.netUpdate = true;
            }

            if (ignoreSolid)
            {
                float dist = Vector2.Distance(Projectile.Center, owner.Center);
                if (dist > ReturnThreshold)
                {
                    Vector2 toPlayer = owner.Center - Projectile.Center;
                    toPlayer.Normalize();
                    float speed = 16f;
                    Projectile.velocity = (Projectile.velocity * 20f + toPlayer * speed) / 21f;

                    if (toPlayer.X > 0)
                    {
                        Projectile.rotation = 0f;
                        Projectile.spriteDirection = 1;
                    }
                    else
                    {
                        Projectile.rotation = 0f;
                        Projectile.spriteDirection = -1;
                    }
                    float breathe = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.08f;
                    _floatOffsetY = breathe * 4f;
                    Projectile.rotation += breathe * 0.2f;
                    return;
                }
                else
                {
                    ignoreSolid = false;
                    Projectile.velocity *= 0.9f;
                }
            }

            // ---- 目标同步：仅拥有者客户端搜索，同步目标索引 ----
            if (Main.myPlayer == Projectile.owner)
            {
                NPC newTarget = FindTarget(owner);
                int newIndex = newTarget != null ? newTarget.whoAmI : -1;
                if ((int)Projectile.ai[0] != newIndex)
                {
                    Projectile.ai[0] = newIndex;
                    Projectile.netUpdate = true;
                }
            }

            // 所有客户端使用同一目标索引
            NPC target = null;
            int targetIndex = (int)Projectile.ai[0];
            if (targetIndex >= 0 && targetIndex < Main.maxNPCs)
            {
                NPC potential = Main.npc[targetIndex];
                if (potential.active && potential.CanBeChasedBy(Projectile, false))
                    target = potential;
            }
            bool hasTarget = target != null;

            Vector2 muzzleOffset = _muzzleOffset;
            if (Projectile.spriteDirection == -1)
                muzzleOffset.X = -muzzleOffset.X;
            Vector2 muzzlePos = Projectile.Center + muzzleOffset.RotatedBy(Projectile.rotation);
            Point tilePos = muzzlePos.ToTileCoordinates();
            bool muzzleInSolid = WorldGen.SolidTile(tilePos.X, tilePos.Y);

            if (hasTarget && muzzleInSolid)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                Projectile.velocity = (Projectile.velocity * 20f + toTarget * 14f) / 21f;
                if (toTarget.X > 0)
                {
                    Projectile.rotation = toTarget.ToRotation();
                    Projectile.spriteDirection = 1;
                }
                else
                {
                    Projectile.rotation = toTarget.ToRotation() + MathHelper.Pi;
                    Projectile.spriteDirection = -1;
                }
                _floatOffsetY = 0f;
                Projectile.rotation = Projectile.rotation.AngleLerp(Projectile.rotation, 0.15f);
                return;
            }

            float targetRotation = Projectile.rotation;
            float targetFloatOffset = 0f;

            if (hasTarget)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                float distance = toTarget.Length();
                toTarget.Normalize();

                bool lineOfSight = Collision.CanHitLine(muzzlePos, 0, 0, target.Center, 0, 0);

                if (distance > TargetDist + Tolerance || !lineOfSight)
                {
                    float speed = 14f;
                    Vector2 moveDirection = toTarget;
                    if (!lineOfSight && distance < (TargetDist + Tolerance) * 1.5f)
                    {
                        Vector2 perp = new Vector2(-toTarget.Y, toTarget.X);
                        if (Projectile.ai[1] == 0)
                            if (Main.rand.NextBool(3)) Projectile.ai[1] = 1;
                        float side = (Projectile.ai[1] == 0) ? -1f : 1f;
                        moveDirection = toTarget * 0.7f + perp * side * 0.3f;
                        moveDirection.Normalize();
                    }
                    Projectile.velocity = (Projectile.velocity * 20f + moveDirection * speed) / 21f;
                }
                else if (distance < TargetDist - Tolerance)
                {
                    float speed = -8f;
                    Projectile.velocity = (Projectile.velocity * 20f + toTarget * speed) / 21f;
                }
                else
                {
                    Projectile.velocity *= 0.9f;
                }

                // 攻击（仅拥有者）
                if (distance <= AttackRange && lineOfSight && !muzzleInSolid && Main.myPlayer == Projectile.owner)
                {
                    int fireInterval = ComputeWeaponUseTime(owner);
                    if (fireInterval < 4) fireInterval = 4;

                    if (Projectile.localAI[0]++ % fireInterval == 0)
                    {
                        int baseDmg = ComputeWeaponDamage(owner);
                        bool crit = Main.rand.NextFloat() < owner.GetTotalCritChance(DamageClass.Ranged) / 100f;
                        if (crit) baseDmg = (int)(baseDmg * 2f);

                        GetAmmoTypes(owner, out int primaryType, out int secondaryType);

                        bool fusion = false;
                        if (LordOfTheFliesProgression.Tier13_Fusion && secondaryType > 0 && secondaryType != ProjectileID.None && Main.rand.NextFloat() < 0.2f)
                            fusion = true;

                        var mplr = owner.GetModPlayer<LordOfTheFliesPlayer>();
                        bool sourceFull = mplr.ChargingEnergy == 120;

                        Vector2 spawnPos = muzzlePos;
                        int proj = Projectile.NewProjectile(
                            Projectile.GetSource_FromThis(),
                            spawnPos,
                            toTarget * 14f,
                            primaryType,
                            baseDmg,
                            Projectile.knockBack,
                            owner.whoAmI
                        );

                        if (proj >= 0 && proj < Main.maxProjectiles)
                        {
                            Projectile p = Main.projectile[proj];
                            p.CritChance = 0;
                            p.usesIDStaticNPCImmunity = true;
                            p.idStaticNPCHitCooldown = 4;

                            var g = p.GetGlobalProjectile<LordOfTheFliesGlobalProj>();
                            g.FromBeelzebub = true;
                            g.BeelzebubCrit = crit ? 1 : 0;

                            if (LordOfTheFliesProgression.Tier12_SourceFull && sourceFull)
                            {
                                mplr.ChargingEnergy -= 5;
                                if (mplr.ChargingEnergy < 0) mplr.ChargingEnergy = 0;
                                mplr.SourceRecoveryPauseTimer = 30;
                                if (LordOfTheFliesProgression.Tier3_Penetration)
                                {
                                    p.penetrate = 2;
                                    p.ArmorPenetration = 40;
                                }
                                g.IsFromTrialMode = true;
                            }

                            if (fusion)
                                g.SecondaryAmmoType = secondaryType;
                        }
                    }
                }

                // 转向（所有客户端统一）
                if (toTarget.X > 0)
                {
                    targetRotation = toTarget.ToRotation();
                    Projectile.spriteDirection = 1;
                }
                else
                {
                    targetRotation = toTarget.ToRotation() + MathHelper.Pi;
                    Projectile.spriteDirection = -1;
                }

                _wobbleTimer += 0.02f;
                _wobbleAngle = MathHelper.Lerp(_wobbleAngle, 0.06f * (float)Math.Sin(_wobbleTimer * 2.5f), 0.1f);
                targetRotation += _wobbleAngle;
                float tilt = Projectile.velocity.Y * 0.005f;
                targetRotation += MathHelper.Clamp(tilt, -0.08f, 0.08f);
                targetFloatOffset = 0f;
            }
            else
            {
                bool isMoving = Projectile.velocity.LengthSquared() > 1f;
                if (isMoving)
                {
                    if (Projectile.velocity.X > 0f) { targetRotation = 0f; Projectile.spriteDirection = 1; }
                    else if (Projectile.velocity.X < 0f) { targetRotation = 0f; Projectile.spriteDirection = -1; }
                    else { if (owner.direction == 1) { targetRotation = 0f; Projectile.spriteDirection = 1; } else { targetRotation = 0f; Projectile.spriteDirection = -1; } }
                }
                else
                {
                    if (owner.direction == 1) { targetRotation = 0f; Projectile.spriteDirection = 1; }
                    else { targetRotation = 0f; Projectile.spriteDirection = -1; }
                }

                float breathe = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.08f;
                targetFloatOffset = breathe * 4f;
                targetRotation += breathe * 0.2f;

                Vector2 homePos = owner.Center + new Vector2(0f, -80f);
                Vector2 toHome = homePos - Projectile.Center;
                float distance = toHome.Length();
                if (distance > 60f) { toHome.Normalize(); Projectile.velocity = (Projectile.velocity * 20f + toHome * 12f) / 21f; }
                else { Projectile.velocity *= 0.9f; }
            }

            Projectile.rotation = Projectile.rotation.AngleLerp(targetRotation, 0.15f);
            _floatOffsetY = targetFloatOffset;
        }

        private NPC FindTarget(Player owner)
        {
            float maxDist = 800f;
            NPC target = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy(Projectile, false))
                    continue;
                Point tilePos = npc.Center.ToTileCoordinates();
                if (WorldGen.SolidTile(tilePos.X, tilePos.Y))
                    continue;
                float dist = Vector2.Distance(npc.Center, Projectile.Center);
                if (dist < maxDist)
                {
                    maxDist = dist;
                    target = npc;
                }
            }
            return target;
        }

        public override bool PreDraw(Player player, ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, _floatOffsetY);
            SpriteEffects effects = (Projectile.spriteDirection == -1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(texture, drawPos, null, Projectile.GetAlpha(lightColor), Projectile.rotation, _origin, Projectile.scale, effects, 0);
            return false;
        }
    }
}