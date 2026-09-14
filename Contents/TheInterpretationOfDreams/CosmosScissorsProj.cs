using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class CosmosScissorsProj : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.StylistKilLaKillScissorsIWish}";

        private const float OutboundTime = 30f;    // 飞出阶段帧数
        private const float OutboundDrag = 0.982f; // 飞出减速
        private const float ReturnAccel = 1.4f;   // 远距追踪加速度
        private const float MaxReturnSpeed = 22f;    // 回程速度上限
        private const float CatchDistance = 32f;    // 兜底接住距离
        private const float CaptureRange = 160f;   // 进入捕获区距离

        private ref float State => ref Projectile.ai[0]; // 0 = 飞出, 1 = 回程
        private ref float Timer => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 12;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 34;
            Projectile.height = 34;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;
            Projectile.timeLeft = 600;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Timer++;
            Projectile.rotation += 0.5f;

            if (State == 0f)
            {
                // —— 飞出 ——
                Projectile.velocity *= OutboundDrag;

                if (Timer >= OutboundTime)
                {
                    State = 1f;
                    Timer = 0f;
                    Projectile.netUpdate = true;
                }
            }
            else
            {
                // —— 回程 ——
                Vector2 toPlayer = player.Center - Projectile.Center;
                float dist = toPlayer.Length();

                Vector2 prevCenter = Projectile.Center - Projectile.velocity;
                bool caught = Collision.CheckAABBvLineCollision(
                    player.Hitbox.TopLeft(),
                    player.Hitbox.Size(),
                    prevCenter,
                    Projectile.Center
                );

                if (caught || dist < CatchDistance)
                {
                    Projectile.Kill();
                    return;
                }

                Vector2 dir = toPlayer / dist;

                if (dist < CaptureRange)
                {
                    float speed = MathHelper.Clamp(dist * 0.35f, 10f, MaxReturnSpeed);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * speed, 0.6f);
                }
                else
                {
                    Projectile.velocity += dir * ReturnAccel;
                    if (Projectile.velocity.Length() > MaxReturnSpeed)
                        Projectile.velocity = Vector2.Normalize(Projectile.velocity) * MaxReturnSpeed;
                }
            }

            // 秒杀：剪刀下方的敌怪
            if (Projectile.owner == Main.myPlayer)
                KillEnemiesBelow();

            SpawnTrailDust();
        }

        /// <summary>
        /// 秒杀「水平与剪刀重叠、垂直位于剪刀下沿以下、且生命值低于 10%」的敌怪。
        /// </summary>
        private void KillEnemiesBelow()
        {
            float left = Projectile.position.X;
            float right = Projectile.position.X + Projectile.width;
            float bottom = Projectile.position.Y + Projectile.height;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.life <= 0) continue;
                if (npc.dontTakeDamage || npc.immortal) continue;
                if (npc.type == NPCID.TargetDummy) continue;

                // ★ 生命值必须低于 10%
                if (npc.lifeMax <= 5) continue;
                if (npc.life / (float)npc.lifeMax >= 0.10f) continue;

                // 水平重叠
                bool xOverlap = npc.position.X < right && npc.position.X + npc.width > left;
                if (!xOverlap) continue;

                // 敌怪整体位于剪刀下沿以下
                if (npc.position.Y <= bottom) continue;

                // —— 秒杀 ——
                npc.StrikeInstantKill();
                SpawnKillEffect(npc.Center);
            }
        }

        private static void SpawnKillEffect(Vector2 center)
        {
            for (int i = 0; i < 16; i++)
            {
                Dust d = Dust.NewDustDirect(center, 4, 4,
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.5f);
                d.velocity = Main.rand.NextVector2Circular(6f, 6f);
                d.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(center, 4, 4,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1.2f);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.noGravity = true;
            }
        }

        private void SpawnTrailDust()
        {
            int chance = State == 0f ? 1 : 2;
            if (Main.rand.NextBool(chance))
            {
                Dust d = Dust.NewDustDirect(
                    Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 120, default, 1.2f);
                d.velocity = -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(0.6f, 0.6f);
                d.noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height,
                    DustID.PurpleTorch, 0f, 0f, 100, default, 1.2f);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Shadowflame, 0f, 0f, 100, default, 1.3f);
                d.velocity *= 1.6f;
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float t = 1f - i / (float)Projectile.oldPos.Length;
                Vector2 pos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color c = new Color(170, 90, 255) * (t * 0.5f);
                Main.EntitySpriteDraw(tex, pos, null, c, Projectile.oldRot[i], origin,
                    Projectile.scale * (0.7f + 0.3f * t), SpriteEffects.None, 0);
            }

            Main.EntitySpriteDraw(tex,
                Projectile.Center - Main.screenPosition, null,
                lightColor, Projectile.rotation, origin, Projectile.scale,
                SpriteEffects.None, 0);

            return false;
        }
    }
}