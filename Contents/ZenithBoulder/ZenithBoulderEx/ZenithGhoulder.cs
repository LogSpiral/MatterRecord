using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithGhoulder : ModProjectile
{
    private int TargetPlayerIndex { get => (int)Projectile.ai[2]; set => Projectile.ai[2] = value; }
    private bool IsGhoustModeEnabled { get => Projectile.ai[1] == 1; set => Projectile.ai[1] = value ? 1 : 0; }
    public override void SetDefaults()
    {
        Projectile.width = 31;
        Projectile.height = 31;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 600;
        base.SetDefaults();
    }

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 10;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }

    public override void AI()
    {
        var projectile = Projectile;
        projectile.localAI[2] += 1f;
        if (Main.netMode == NetmodeID.Server && projectile.Center.Y < Main.worldSurface * 16.0 && projectile.localAI[2] % 60f == 0f)
            projectile.netUpdate = true;

        bool flag9 = false;
        if (projectile.rotation < Math.PI * -2.0)
            projectile.rotation += (float)Math.PI * 2f;
        else if (projectile.rotation > Math.PI * 2.0)
            projectile.rotation -= (float)Math.PI * 2f;

        TargetPlayerIndex = -1;

        if (IsGhoustModeEnabled)
        {
            flag9 = true;
            bool flag10 = false;
            int num205 = Player.FindClosest(projectile.position, projectile.width, projectile.height);
            TargetPlayerIndex = num205;
            if (projectile.Right.X > Main.player[num205].Left.X && projectile.Left.X < Main.player[num205].Right.X)
            {
                flag10 = true;
            }
            else if (projectile.Center.X < Main.player[num205].Center.X)
            {
                if (Main.player[num205].direction > 0)
                    flag10 = true;
            }
            else if (Main.player[num205].direction < 0)
            {
                flag10 = true;
            }

            if (flag10)
            {
                if (projectile.localAI[0] < 84f)
                    projectile.localAI[0] += 3f;
                else
                    projectile.localAI[0] = 84f;

                if (projectile.frame < 7)
                {
                    projectile.frameCounter++;
                    if (projectile.frameCounter > 3)
                    {
                        projectile.frame++;
                        projectile.frameCounter = 0;
                    }
                }
                else if (projectile.frame > 7)
                {
                    projectile.frameCounter++;
                    if (projectile.frameCounter > 3)
                    {
                        projectile.frame--;
                        projectile.frameCounter = 0;
                    }
                }
                else
                {
                    projectile.frame = 7;
                    projectile.frameCounter = 0;
                }

                projectile.tileCollide = false;
                float num206 = 6f;
                int num207 = 35;
                projectile.rotation *= 0.6f;
                if (projectile.velocity.X < 0f)
                    projectile.spriteDirection = -1;
                else if (projectile.velocity.X > 0f)
                    projectile.spriteDirection = 1;

                Vector2 vector24 = Main.player[num205].Center - projectile.Center;
                vector24.Normalize();
                vector24 *= num206;
                projectile.velocity = (projectile.velocity * num207 + vector24) / (num207 + 1);
            }
            else
            {
                if (projectile.localAI[0] > 0f)
                    projectile.localAI[0] -= 3f;
                else
                    projectile.localAI[0] = 0f;

                if (projectile.frame >= 7)
                {
                    projectile.frameCounter++;
                    if (projectile.frameCounter > 3)
                    {
                        if (projectile.frame >= 9)
                            projectile.frame = 4;
                        else
                            projectile.frame++;

                        projectile.frameCounter = 0;
                    }
                }
                else if (projectile.frame > 0)
                {
                    projectile.frameCounter++;
                    if (projectile.frameCounter > 3)
                    {
                        projectile.frame--;
                        projectile.frameCounter = 0;
                    }
                }
                else
                {
                    projectile.frame = 0;
                    projectile.frameCounter = 0;
                }

                projectile.tileCollide = true;
                projectile.velocity.X *= 0.92f;
                projectile.velocity.Y += 0.3f;
            }
        }
        else
        {
            projectile.rotation += projectile.velocity.X * 0.06f;
            projectile.frame = 0;
        }

        if (projectile.ai[0] != 0f && projectile.velocity.Y <= 0f && !flag9 && projectile.velocity.X == 0f)
        {
            float num208 = 0.5f;
            int i2 = (int)((projectile.position.X - 8f) / 16f);
            int num209 = (int)(projectile.position.Y / 16f);
            bool flag11 = false;
            bool flag12 = false;
            if (WorldGen.SolidTile(i2, num209) || WorldGen.SolidTile(i2, num209 + 1))
                flag11 = true;

            i2 = (int)((projectile.position.X + projectile.width + 8f) / 16f);
            if (WorldGen.SolidTile(i2, num209) || WorldGen.SolidTile(i2, num209 + 1))
                flag12 = true;

            if (flag11)
            {
                projectile.velocity.X = num208;
            }
            else if (flag12)
            {
                projectile.velocity.X = 0f - num208;
            }
            else
            {
                i2 = (int)((projectile.position.X - 8f - 16f) / 16f);
                num209 = (int)(projectile.position.Y / 16f);
                flag11 = false;
                flag12 = false;
                if (WorldGen.SolidTile(i2, num209) || WorldGen.SolidTile(i2, num209 + 1))
                    flag11 = true;

                i2 = (int)((projectile.position.X + projectile.width + 8f + 16f) / 16f);
                if (WorldGen.SolidTile(i2, num209) || WorldGen.SolidTile(i2, num209 + 1))
                    flag12 = true;

                if (flag11)
                {
                    projectile.velocity.X = num208;
                }
                else if (flag12)
                {
                    projectile.velocity.X = 0f - num208;
                }
                else
                {
                    i2 = (int)((projectile.position.X - 8f - 32f) / 16f);
                    num209 = (int)(projectile.position.Y / 16f);
                    flag11 = false;
                    flag12 = false;
                    if (WorldGen.SolidTile(i2, num209) || WorldGen.SolidTile(i2, num209 + 1))
                        flag11 = true;

                    i2 = (int)((projectile.position.X + projectile.width + 8f + 32f) / 16f);
                    if (WorldGen.SolidTile(i2, num209) || WorldGen.SolidTile(i2, num209 + 1))
                        flag12 = true;

                    if (!flag11 && !flag12)
                    {
                        if ((int)(projectile.Center.X / 16f) % 2 == 0)
                            flag11 = true;
                        else
                            flag12 = true;
                    }

                    if (flag11)
                        projectile.velocity.X = num208;
                    else if (flag12)
                        projectile.velocity.X = 0f - num208;
                }
            }
        }

        projectile.ai[0] = 1f;
        if (projectile.velocity.Y > 16f)
            projectile.velocity.Y = 16f;

        if (!IsGhoustModeEnabled)
        {
            if (projectile.velocity.X > 0f && projectile.velocity.X < 7f)
                projectile.velocity.X += 0.05f;

            if (projectile.velocity.X < 0f && projectile.velocity.X > -7f)
                projectile.velocity.X -= 0.05f;
            projectile.velocity.Y += 0.3f;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (!Main.player.IndexInRange(TargetPlayerIndex)) return false;
        if (TextureAssets.Projectile[Type] is not { IsLoaded: true } asset) return false;
        var target = Main.player[TargetPlayerIndex];
        const float distanceMax = 40f * 16;
        float distance = Vector2.Distance(Projectile.Center, target.Center);
        if (distance > distanceMax) return false;
        float alpha = MathHelper.SmoothStep(1, 0, distance / distanceMax);

        if (Projectile.timeLeft < 60)
            alpha *= MathHelper.SmoothStep(0, 1, Projectile.timeLeft / 60f);

        if (Projectile.timeLeft > 540)
            alpha *= MathHelper.SmoothStep(0, 1, (600 - Projectile.timeLeft) / 60f);

        float alpha2 = Projectile.localAI[0] / 84f;
        Main.EntitySpriteDraw(
            asset.Value,
            Projectile.position + new Vector2(17) - Main.screenPosition,
            asset.Frame(1, 10, 0, Projectile.frame),
            lightColor * alpha,
            Projectile.rotation,
            new Vector2(17),
            1,
            0,
            0);
        var addColor = lightColor with { A = 0 };

        for (int n = 0; n < 10; n++) 
        {
            Main.EntitySpriteDraw(
                asset.Value,
                Projectile.oldPos[n] + new Vector2(17) - Main.screenPosition,
                asset.Frame(1, 10, 0, Projectile.frame),
                addColor * (alpha * alpha2 * 0.5f * (1 - n * .1f)),
                Projectile.rotation,
                new Vector2(17),
                1,
                0,
                0);
        }
        return false;
    }

    public override bool OnTileCollide(Vector2 lastVelocity)
    {
        float num36 = Math.Abs(lastVelocity.X);
        float num37 = Math.Abs(lastVelocity.Y);
        float num38 = 0.9f;
        float num39 = 0.9f;
        if (num36 < 0.5f)
            num38 = 0.1f;
        else if (num36 < 0.75f)
            num38 = 0.25f;
        else if (num36 < 1f)
            num38 = 0.5f;

        if (num37 < 0.5f)
            num39 = 0.1f;
        else if (num37 < 0.75f)
            num39 = 0.25f;
        else if (num37 < 1f)
            num39 = 0.5f;

        bool flag12 = false;
        if (Projectile.velocity.Y != lastVelocity.Y)
        {
            if (Math.Abs(lastVelocity.Y) > 5f)
                flag12 = true;

            IsGhoustModeEnabled = true;
            Projectile.velocity.Y = (0f - lastVelocity.Y) * num39;
        }

        if (Projectile.velocity.X != lastVelocity.X)
        {
            if (Math.Abs(lastVelocity.X) > 5f)
                flag12 = true;

            IsGhoustModeEnabled = true;
            Projectile.velocity.X = (0f - lastVelocity.X) * num38;
        }

        if (flag12)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        }
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (int n = 0; n < 10; n++) 
        {
            Dust.NewDustPerfect(
                Projectile.Center, 
                DustID.Stone, 
                Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, 4), 
                0,
                default, 
                Main.rand.NextFloat(0.5f, 2f));
        }
    }
}
