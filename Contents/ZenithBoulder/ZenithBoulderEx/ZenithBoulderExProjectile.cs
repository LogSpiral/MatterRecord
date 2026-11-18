using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.Graphics.Renderers;

namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithBoulderExProjectile : ModProjectile
{
    public override bool IsLoadingEnabled(Mod mod) => true;
    public override string Texture => base.Texture.Replace("Projectile", "");
    private static Asset<Texture2D> Glow { get; set; }
    public override void Load()
    {
        if (!Main.dedServ)
            Glow = ModAsset.ZenithBoulderEx_Glow_Async;
    }
    public override void SetDefaults()
    {
        Projectile.damage = 30;
        Projectile.width = 31;
        Projectile.height = 31;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = true;
        Projectile.penetrate = -1;
        Projectile.trap = true;
        base.SetDefaults();
    }

    public override void AI()
    {
        Projectile.localAI[0]++;

        if (Projectile.ai[0] != 0f && Projectile.velocity.Y <= 0f && Projectile.velocity.X == 0f)
        {
            float num218 = 0.5f;
            int i2 = (int)((Projectile.position.X - 8f) / 16f);
            int num219 = (int)(Projectile.position.Y / 16f);
            bool flag9 = false;
            bool flag10 = false;
            if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                flag9 = true;

            i2 = (int)((Projectile.position.X + (float)Projectile.width + 8f) / 16f);
            if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                flag10 = true;

            if (flag9)
            {
                Projectile.velocity.X = num218;
            }
            else if (flag10)
            {
                Projectile.velocity.X = 0f - num218;
            }
            else
            {
                i2 = (int)((Projectile.position.X - 8f - 16f) / 16f);
                num219 = (int)(Projectile.position.Y / 16f);
                flag9 = false;
                flag10 = false;
                if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                    flag9 = true;

                i2 = (int)((Projectile.position.X + (float)Projectile.width + 8f + 16f) / 16f);
                if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                    flag10 = true;

                if (flag9)
                {
                    Projectile.velocity.X = num218;
                }
                else if (flag10)
                {
                    Projectile.velocity.X = 0f - num218;
                }
                else
                {
                    i2 = (int)((Projectile.position.X - 8f - 32f) / 16f);
                    num219 = (int)(Projectile.position.Y / 16f);
                    flag9 = false;
                    flag10 = false;
                    if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                        flag9 = true;

                    i2 = (int)((Projectile.position.X + (float)Projectile.width + 8f + 32f) / 16f);
                    if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                        flag10 = true;

                    if (!flag9 && !flag10)
                    {
                        if ((int)(Projectile.Center.X / 16f) % 2 == 0)
                            flag9 = true;
                        else
                            flag10 = true;
                    }

                    if (flag9)
                        Projectile.velocity.X = num218;
                    else if (flag10)
                        Projectile.velocity.X = 0f - num218;
                }
            }
        }

        Projectile.rotation += Projectile.velocity.X * 0.06f;
        Projectile.ai[0] = 1f;
        if (Projectile.velocity.Y > 16f)
            Projectile.velocity.Y = 16f;

        if (Projectile.velocity.Y <= 6f)
        {
            if (Projectile.velocity.X > 0f && Projectile.velocity.X < 7f)
                Projectile.velocity.X += 0.05f;

            if (Projectile.velocity.X < 0f && Projectile.velocity.X > -7f)
                Projectile.velocity.X -= 0.05f;
        }
        Projectile.velocity.Y += 0.3f;

        for (int n = Projectile.oldPos.Length - 1; n > 0; n--)
        {
            Projectile.oldPos[n] = Projectile.oldPos[n - 1];
            Projectile.oldRot[n] = Projectile.oldRot[n - 1];
        }
        Projectile.oldPos[0] = Projectile.Center;
        Projectile.oldRot[0] = Projectile.rotation;
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var tex = Glow.Value;
        int m = Projectile.oldPos.Length;
        for (int n = m - 1; n >= 0; n--)
        {
            float fac = Utils.GetLerpValue(0, m - 1, n);
            float scaler = MathHelper.Lerp(1, 0.2f, fac);
            Main.EntitySpriteDraw(tex, Projectile.oldPos[n] - Main.screenPosition + Main.rand.NextVector2Unit() * MathHelper.Lerp(0, Main.rand.NextFloat(0, 8), fac), null, Main.hslToRgb((fac - Main.GlobalTimeWrappedHourly) % 1, 1, 0.95f) * scaler * MathHelper.Clamp(Projectile.localAI[0] / m, 0, 1), Projectile.oldRot[n], new Vector2(16), scaler * 1.2f, 0, 0);//
        }
        return base.PreDraw(ref lightColor);
    }

    private static readonly ParticleOrchestraType[] ParticleTypes_HitEffects = 
                                        [
                                             ParticleOrchestraType.Excalibur,
                                             ParticleOrchestraType.NightsEdge,
                                             ParticleOrchestraType.TerraBlade,
                                             ParticleOrchestraType.TrueExcalibur,
                                             ParticleOrchestraType.TrueNightsEdge
                                        ];
    private static readonly ParticleOrchestraType[] ParticleTypes_CollideEffects = 
                                        [
                                             ParticleOrchestraType.BlackLightningHit,
                                             ParticleOrchestraType.Excalibur,
                                             ParticleOrchestraType.Keybrand,
                                             ParticleOrchestraType.NightsEdge,
                                             ParticleOrchestraType.PrincessWeapon,
                                             ParticleOrchestraType.RainbowRodHit,
                                             ParticleOrchestraType.StardustPunch,
                                             ParticleOrchestraType.StellarTune,
                                             ParticleOrchestraType.TerraBlade,
                                             ParticleOrchestraType.TrueExcalibur,
                                             ParticleOrchestraType.TrueNightsEdge
                                        ];

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        float y = Projectile.Center.Y;
        int x = (int)Projectile.Center.X / 16;
        //while (y > Main.screenPosition.Y && WorldGen.InWorld(x, (int)y / 16) && !Framing.GetTileSafely(x, (int)y / 16).HasTile)
        //    y -= 16;

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center with { Y = y - 256 },
            Vector2.UnitY * 16,
            ModContent.ProjectileType<ZenithSubBoulder>(),
            70, 30f, Main.myPlayer, target.whoAmI, 2, Main.rand.NextFloat(30, 60));
        ParticleOrchestraSettings setting = new()
        {
            MovementVector = default,
            PositionInWorld = target.Center
        };
        ParticleOrchestrator.SpawnParticlesDirect(Main.rand.Next(ParticleTypes_HitEffects), setting);
        target.AddBuff(BuffID.OnFire, 300);
        target.AddBuff(BuffID.Burning, 60);
        target.AddBuff(BuffID.Webbed, 30);
        target.AddBuff(BuffID.Slow, 180);
        base.OnHitNPC(target, hit, damageDone);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        float y = Projectile.Center.Y;
        int x = (int)Projectile.Center.X / 16;
        //while (y > Main.screenPosition.Y && WorldGen.InWorld(x, (int)y / 16) && !Framing.GetTileSafely(x, (int)y / 16).HasTile)
        //    y -= 16;

        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center with { Y = y - 256 },
            Vector2.UnitY * 16,
            ModContent.ProjectileType<ZenithSubBoulder>(),
            70, 30f, Main.myPlayer, target.whoAmI, 1, Main.rand.NextFloat(30, 60));
        ParticleOrchestraSettings setting = new()
        {
            MovementVector = default,
            PositionInWorld = target.Center
        };
        ParticleOrchestrator.SpawnParticlesDirect(Main.rand.Next(ParticleTypes_HitEffects), setting);
        target.AddBuff(BuffID.OnFire, 300);
        target.AddBuff(BuffID.Burning, 60);
        target.AddBuff(BuffID.Webbed, 30);
        target.AddBuff(BuffID.Slow, 180);

        base.OnHitPlayer(target, info);
    }

    public override bool OnTileCollide(Vector2 lastVelocity)
    {
        float num36 = Math.Abs(lastVelocity.X);
        float num37 = Math.Abs(lastVelocity.Y);
        float num38 = 0.95f;
        float num39 = 0.95f;
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

            Projectile.velocity.Y = (0f - lastVelocity.Y) * num39;
        }

        if (Projectile.velocity.X != lastVelocity.X)
        {
            if (Math.Abs(lastVelocity.X) > 5f)
                flag12 = true;

            Projectile.velocity.X = (0f - lastVelocity.X) * num38;
        }

        if (flag12)
        {
            ScreenShakeSystem.Instance.ScreenOffset += lastVelocity;
            Projectile.localAI[1] += 1f;
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);

            float pitch = MathHelper.Clamp(-Vector2.Distance(Projectile.Center, Main.LocalPlayer.Center) / 1200f, -2, 0);


            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact with { Pitch = pitch, Volume = 2f }, Projectile.position);

            for (int n = 0; n < 10; n++)
            {
                ParticleOrchestraSettings setting = new()
                {
                    MovementVector = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 64) - lastVelocity * 4,
                    PositionInWorld = Projectile.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 128)
                };
                ParticleOrchestrator.SpawnParticlesDirect(Main.rand.Next(ParticleTypes_CollideEffects), setting);
            }

            for (int n = 0; n < 5; n++) 
            {
                if (Main.rand.NextBool(20)) 
                {
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        -lastVelocity + Main.rand.NextVector2Unit() * Main.rand.NextFloat(1, 4),
                        ModContent.ProjectileType<ZenithGhoulder>(),
                        70,
                        10,
                        Projectile.owner);
                    continue;
                }
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    -lastVelocity + Main.rand.NextVector2Unit() * Main.rand.NextFloat(4, 16),
                    ProjectileID.MiniBoulder,
                    70,
                    10,
                    Projectile.owner);
            }
        }

        if (Projectile.velocity.Length() < 0.1f && Projectile.localAI[0] > 50f)
            Projectile.Kill();

        if (Projectile.localAI[1] > 20f)
            Projectile.Kill();
        return false;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        for (int num564 = 0; num564 < 30; num564++)
        {
            int num565 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.MoonBoulder);
            if (Main.rand.NextBool(2))
            {
                Dust dust2 = Main.dust[num565];
                dust2.scale *= 1.4f;
            }

            Projectile.velocity *= 1.9f;
        }
        if (Main.netMode != NetmodeID.MultiplayerClient)
            Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Hitbox, new Item(ModContent.ItemType<ZenithBoulderEx>()));
        base.OnKill(timeLeft);
    }
}
