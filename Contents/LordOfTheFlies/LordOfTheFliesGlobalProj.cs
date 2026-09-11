using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.Audio;
using MatterRecord.Contents.Eraser;
using EraserItem = MatterRecord.Contents.Eraser.Eraser;

namespace MatterRecord.Contents.LordOfTheFlies
{
    public class LordOfTheFliesGlobalProj : GlobalProjectile
    {
        public bool IsFromTrialMode;
        public bool IsFromLOF;
        public int SecondaryAmmoType = 0;

        public bool FromBeelzebub = false;
        public int BeelzebubCrit = 0;

        public override bool InstancePerEntity => true;

        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (source is EntitySource_ItemUse itemUseSource && itemUseSource?.Item?.ModItem is LordOfTheFlies)
                IsFromLOF = true;
            base.OnSpawn(projectile, source);
        }

        public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (IsFromTrialMode)
            {
                modifiers.SetCrit();
                // 无视 20 护甲（项3 进度锁）
                if (LordOfTheFliesProgression.Tier3_Penetration)
                    modifiers.ArmorPenetration += 20;
            }
        }

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ---- 湮灭弹抹杀普通敌人（项14 进度锁，月总解锁） ----
            // 服务器/单人直接权威抹杀，避免实体被 SyncNPC 同步「复活」。
            //
            // Boss 判定参考原版 Boss 血条，而非仅看 npc.boss：
            // 事件类 Boss（南瓜王、冰霜女王等）的 npc.boss 可能为 false，但它们拥有 Boss 头像索引
            // 会显示 Boss 血条，同样视为 Boss 不抹杀。依据 tModLoader 文档：
            // "any NPC with a boss head index will automatically display a common boss bar"。
            bool isBossLike = target.boss
                || target.realLife >= 0 // 多体节 Boss 的体节（如世界吞噬者身体段）
                || target.GetBossHeadTextureIndex() >= 0 // 有 Boss 头像索引 → 有 Boss 血条（覆盖事件类 Boss）
                || target.BossBar != null; // mod 自定义 Boss 血条

            if (LordOfTheFliesProgression.Tier14_NoConsume
                && projectile.type == ModContent.ProjectileType<AnnihilationBullet>()
                && !isBossLike
                && target.rarity == 0 && !target.friendly && !target.dontTakeDamage)
            {
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    EraserEraseSync.Get(0, target.whoAmI).Send();
                    target.active = false;
                    target.life = 0;
                }
                else
                {
                    EraserItem.KillNpc(target);
                }
            }

            // ---- 蝇王强化1：命中叠层防御（项1 进度锁，蝇王/别西卜子弹均生效） ----
            if (LordOfTheFliesProgression.Tier1_DefenseOnHit && (IsFromLOF || FromBeelzebub))
            {
                Player owner = Main.player[projectile.owner];
                if (owner != null && owner.active)
                    owner.GetModPlayer<LordOfTheFliesPlayer>().AddDefenseStack();
            }

            // ---- 审判模式普通子弹爆炸（非湮灭弹，项10 进度锁） ----
            if (LordOfTheFliesProgression.Tier10_Explosion && IsFromTrialMode && projectile.type != ModContent.ProjectileType<AnnihilationBullet>())
            {
                if (Main.rand.NextFloat() < 0.3f) // 30% 概率
                {
                    float explosionRadius = 120f;
                    float explosionDamage = damageDone * 0.5f;

                    Player owner = Main.player[projectile.owner];
                    if (owner != null && owner.active)
                    {
                        for (int i = 0; i < Main.maxNPCs; i++)
                        {
                            NPC npc = Main.npc[i];
                            if (!npc.active || npc.friendly || npc.dontTakeDamage)
                                continue;

                            float distance = Vector2.Distance(npc.Center, projectile.Center);
                            if (distance <= explosionRadius)
                            {
                                // 距离衰减：边缘伤害为 50%
                                float damageMultiplier = 1f - (distance / explosionRadius) * 0.5f;
                                int finalDamage = (int)(explosionDamage * damageMultiplier);
                                if (finalDamage < 1) finalDamage = 1;

                                // 构造 HitInfo，击退设为 0
                                NPC.HitInfo explosionHit = new NPC.HitInfo
                                {
                                    Damage = finalDamage,
                                    Knockback = 0f, // 无击退，防止傀儡被炸飞
                                    Crit = false,
                                    HitDirection = (npc.Center.X > projectile.Center.X) ? 1 : -1,
                                    DamageType = DamageClass.Ranged // 确保伤害类型正确
                                };

                                // 直接对 NPC 造成伤害，并计入玩家统计
                                owner.StrikeNPCDirect(npc, explosionHit);
                            }
                        }
                    }

                    // ---- 视觉特效（仅客户端） ----
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(2f, 10f);
                            Dust dust = Dust.NewDustPerfect(
                                projectile.Center + Main.rand.NextVector2Unit() * 20f,
                                DustID.Torch,
                                velocity,
                                0,
                                Color.OrangeRed,
                                Main.rand.NextFloat(1f, 2.5f)
                            );
                            dust.noGravity = true;
                        }
                        for (int i = 0; i < 15; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 5f);
                            Dust dust = Dust.NewDustPerfect(
                                projectile.Center + Main.rand.NextVector2Unit() * 30f,
                                DustID.Smoke,
                                velocity,
                                0,
                                Color.Gray,
                                Main.rand.NextFloat(1.5f, 3f)
                            );
                            dust.noGravity = true;
                        }
                        for (int i = 0; i < 8; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 3f);
                            Dust dust = Dust.NewDustPerfect(
                                projectile.Center + Main.rand.NextVector2Unit() * 10f,
                                DustID.GoldFlame,
                                velocity,
                                0,
                                Color.White,
                                Main.rand.NextFloat(1f, 2f)
                            );
                            dust.noGravity = true;
                        }
                    }

                    // 播放音效
                    SoundEngine.PlaySound(SoundID.Item14, projectile.Center);
                }
            }

            // ---- 融合幽灵子弹 ----
            if (SecondaryAmmoType > 0 && SecondaryAmmoType != ProjectileID.None)
            {
                int dummyIndex = Projectile.NewProjectile(
                    projectile.GetSource_FromThis(),
                    projectile.Center,
                    Vector2.Zero,
                    SecondaryAmmoType,
                    damageDone / 2,
                    0f,
                    projectile.owner
                );
                if (dummyIndex >= 0 && dummyIndex < Main.maxProjectiles)
                {
                    Projectile dummy = Main.projectile[dummyIndex];
                    dummy.Center = projectile.Center;
                    dummy.timeLeft = 1;
                    dummy.penetrate = 1;
                    dummy.alpha = 255;
                    dummy.hide = true;
                }
                SecondaryAmmoType = 0;
            }

            // ---- 蝇王暴击额外伤害 ----
            if (IsFromLOF)
            {
                var owner = Main.player[projectile.owner];
                if (hit.Crit)
                {
                    var rangedModifier = owner.rangedDamage;
                    var critChance = owner.GetTotalCritChance(DamageClass.Ranged) * 0.01f;
                    critChance = MathHelper.Clamp(critChance, 0, 1);
                    // 基础 +5 始终生效；生命值百分比部分受项5进度锁控制
                    float lifePercentBonus = LordOfTheFliesProgression.Tier5_CritLifePercent
                        ? target.lifeMax * critChance * 0.0005f
                        : 0f;
                    var dmg = rangedModifier.ApplyTo(Main.DamageVar(lifePercentBonus + 5, owner.luck));
                    NPC.HitInfo info = hit;
                    info.Damage = (int)dmg;
                    info.Knockback = 0;
                    owner.StrikeNPCDirect(target, info);
                }
            }

            if (FromBeelzebub && BeelzebubCrit == 1)
            {
                var owner = Main.player[projectile.owner];
                var rangedModifier = owner.rangedDamage;
                var critChance = owner.GetTotalCritChance(DamageClass.Ranged) * 0.01f;
                critChance = MathHelper.Clamp(critChance, 0, 1);
                float lifePercentBonus = LordOfTheFliesProgression.Tier5_CritLifePercent
                    ? target.lifeMax * critChance * 0.0005f
                    : 0f;
                var dmg = rangedModifier.ApplyTo(Main.DamageVar(lifePercentBonus + 5, owner.luck));
                NPC.HitInfo info = hit;
                info.Damage = (int)dmg;
                info.Knockback = 0;
                owner.StrikeNPCDirect(target, info);
            }

            base.OnHitNPC(projectile, target, hit, damageDone);
        }

        // ---- 网络同步 ----
        public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
        {
            binaryWriter.Write(IsFromTrialMode);
            binaryWriter.Write(IsFromLOF);
            binaryWriter.Write(SecondaryAmmoType);
            binaryWriter.Write(FromBeelzebub);
            binaryWriter.Write(BeelzebubCrit);
            base.SendExtraAI(projectile, bitWriter, binaryWriter);
        }

        public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
        {
            IsFromTrialMode = binaryReader.ReadBoolean();
            IsFromLOF = binaryReader.ReadBoolean();
            SecondaryAmmoType = binaryReader.ReadInt32();
            FromBeelzebub = binaryReader.ReadBoolean();
            BeelzebubCrit = binaryReader.ReadInt32();
            base.ReceiveExtraAI(projectile, bitReader, binaryReader);
        }
    }
}