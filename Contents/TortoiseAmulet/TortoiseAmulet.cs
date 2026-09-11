using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TortoiseAmulet
{
    public class TurtleAmulet : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.LightRed;
            Item.value = 5000;
            Item.defense = 6; // 防御 +6
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<TurtleAmuletPlayer>().equipped = true;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.TurtleShell, 1);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }

    public class TurtleAmuletPlayer : ModPlayer
    {
        public bool equipped;
        private bool lastEquipped;
        private bool hasSummoned;

        public override void ResetEffects()
        {
            lastEquipped = equipped;
            equipped = false;
        }

        public override void PostUpdate()
        {
            if (lastEquipped && !equipped && !hasSummoned)
            {
                SummonRevengeTortoises();
                hasSummoned = true;
            }
            if (equipped)
                hasSummoned = false;
        }

        private void SummonRevengeTortoises()
        {
            const int count = 3;
            for (int i = 0; i < count; i++)
            {
                SpawnSingleTortoise();
            }
        }

        private void SpawnSingleTortoise()
        {
            Player player = Player;
            int tortoiseType = player.ZoneSnow ? NPCID.IceTortoise : NPCID.GiantTortoise;
            Vector2 baseTarget = player.Center;

            const float gravity = 0.4f;
            const float baseSpeed = 14f;
            const float maxAngleDeg = 65f;
            float maxAngleRad = MathHelper.ToRadians(maxAngleDeg);

            Vector2 targetOffset = new Vector2(Main.rand.Next(-80, 81), Main.rand.Next(-80, 81));
            Vector2 targetPos = baseTarget + targetOffset;
            targetPos.X = MathHelper.Clamp(targetPos.X, 100, Main.maxTilesX * 16 - 100);
            targetPos.Y = MathHelper.Clamp(targetPos.Y, 100, Main.maxTilesY * 16 - 100);

            Vector2 spawnPos = Vector2.Zero;
            float vx = 0f, vy = 0f;
            bool found = false;

            int minDist = 1024;
            int maxDist = 1600;

  
            for (int attempt = 0; attempt < 30; attempt++)
            {
                int side = Main.rand.NextBool() ? 1 : -1;
                float offsetX = minDist + Main.rand.Next(0, maxDist - minDist);
                float offsetY = Main.rand.Next(-200, 200);
                spawnPos = player.Center + new Vector2(side * offsetX, offsetY);
                spawnPos.X = MathHelper.Clamp(spawnPos.X, 100, Main.maxTilesX * 16 - 100);
                spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 100, Main.maxTilesY * 16 - 100);

                if (Math.Abs(spawnPos.X - player.Center.X) < minDist)
                    continue;

                float dx = targetPos.X - spawnPos.X;
                float dy = targetPos.Y - spawnPos.Y;
                float dirX = Math.Sign(dx);
                float speed = baseSpeed + Main.rand.NextFloat(-2f, 3f);
                vx = dirX * Math.Max(speed, 8f);
                float t = dx / vx; // vx 与 dx 同号，t > 0
                if (t <= 0f) continue;

                vy = (dy - 0.5f * gravity * t * t) / t;
                if (vy >= 0f) continue;

                float angleRad = (float)Math.Atan2(-vy, Math.Abs(vx));
                if (angleRad <= maxAngleRad)
                {
                    found = true;
                    break;
                }
            }

            // fallback：若未找到合适角度，强制生成一个从侧面飞来的轨迹
            if (!found)
            {
                spawnPos = targetPos + new Vector2(Main.rand.NextBool() ? -1200 : 1200, -400);
                spawnPos.X = MathHelper.Clamp(spawnPos.X, 100, Main.maxTilesX * 16 - 100);
                spawnPos.Y = MathHelper.Clamp(spawnPos.Y, 100, Main.maxTilesY * 16 - 100);
                float dx = targetPos.X - spawnPos.X;
                float dy = targetPos.Y - spawnPos.Y;
                float dirX = Math.Sign(dx);
                vx = dirX * 14f;
                float t = Math.Abs(dx) / Math.Abs(vx);
                vy = (dy - 0.5f * gravity * t * t) / t;
                if (vy >= 0f) vy = -10f;
            }

            int npcIndex = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, tortoiseType);
            NPC tortoise = Main.npc[npcIndex];
            if (tortoise == null) return;

            tortoise.damage = 9999;
            tortoise.defense = 1;
            tortoise.lifeMax = 1;
            tortoise.life = 1;

            tortoise.ai[0] = -1f;
            tortoise.ai[1] = vx;
            tortoise.ai[2] = vy;
            tortoise.ai[3] = 0f;

            tortoise.localAI[0] = spawnPos.X;
            tortoise.localAI[1] = spawnPos.Y;
            tortoise.localAI[2] = targetPos.X;   // 最终落点（原始目标）
            tortoise.localAI[3] = targetPos.Y;

            tortoise.noTileCollide = true;
            tortoise.noGravity = true;
            tortoise.velocity = Vector2.Zero;

            tortoise.knockBackResist = 0f;
            tortoise.netUpdate = true;
        }
    }

    public class TurtleAmuletGlobalNPC : GlobalNPC
    {
        private const float Gravity = 0.4f;
        private const float RotateDuration = 60f;
        private const float RotateSpeed = 0.2f;
        private const float MaxFlightFrames = 600f;
        private const float LandingCheckRadius = 160f;



        public override void PostAI(NPC npc)
        {
            if (!(npc.type == NPCID.GiantTortoise || npc.type == NPCID.IceTortoise))
                return;

            if (!npc.active || npc.life <= 0)
            {
                if (!npc.active && npc.life > 0)
                    npc.active = false;
                npc.netUpdate = true;
                return;
            }

            npc.noTileCollide = true;
            npc.knockBackResist = 0f;
            if (npc.ai[0] != -1f && npc.ai[0] != -2f)
            {
                npc.ai[0] = -1f;
                npc.netUpdate = true;
            }

            if (npc.ai[0] == -1f)
            {
                float vx = npc.ai[1];
                float vy = npc.ai[2];
                float frame = npc.ai[3];
                Vector2 startPos = new Vector2(npc.localAI[0], npc.localAI[1]);

                frame += 1f;
                npc.ai[3] = frame;

                float t = frame;
                float x = startPos.X + vx * t;
                float y = startPos.Y + vy * t + 0.5f * Gravity * t * t;
                npc.Center = new Vector2(x, y);
                npc.velocity = new Vector2(vx, vy + Gravity * t);

                if (npc.velocity != Vector2.Zero)
                {
                    float rotDir = (npc.velocity.X != 0f) ? Math.Sign(npc.velocity.X) : Math.Sign(npc.velocity.Y);
                    npc.rotation += 0.35f * rotDir;
                }

                Vector2 targetPos = new Vector2(npc.localAI[2], npc.localAI[3]);
                float distToTarget = Vector2.Distance(npc.Center, targetPos);

                if (distToTarget < LandingCheckRadius)
                {
                    Vector2 bottomCenter = new Vector2(npc.Center.X, npc.Bottom.Y);
                    bool onGround = false;
                    int tileX = (int)(bottomCenter.X / 16f);
                    int tileY = (int)((bottomCenter.Y + 1f) / 16f);
                    if (tileX >= 0 && tileX < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
                    {
                        Tile tile = Main.tile[tileX, tileY];
                        if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType])
                        {
                            onGround = true;
                        }
                    }

                    if (onGround)
                    {
                        float groundY = tileY * 16f;
                        npc.position.Y = groundY - npc.height;
                        npc.Center = new Vector2(npc.Center.X, npc.position.Y + npc.height / 2f);
                        npc.velocity = Vector2.Zero;
                        npc.ai[0] = -2f;
                        npc.ai[3] = 0f;
                        npc.netUpdate = true;
                    }
                }

                if (frame >= MaxFlightFrames)
                {
                    npc.active = false;
                    npc.netUpdate = true;
                }

                if (frame % 30 == 0)
                    npc.netUpdate = true;
            }
            else if (npc.ai[0] == -2f)
            {
                npc.velocity = Vector2.Zero;
                npc.noTileCollide = true;

                npc.rotation += RotateSpeed * npc.direction;
                npc.ai[3] += 1f;

                if (npc.ai[3] >= RotateDuration)
                {
                    for (int i = 0; i < 25; i++)
                    {
                        Dust dust = Dust.NewDustDirect(
                            npc.position,
                            npc.width,
                            npc.height,
                            DustID.Smoke,
                            0f, 0f,
                            100,
                            default,
                            2f
                        );
                        dust.velocity *= 1.5f;
                        dust.noGravity = true;
                    }
                    npc.active = false;
                    npc.netUpdate = true;
                }
            }
        }
    }
}