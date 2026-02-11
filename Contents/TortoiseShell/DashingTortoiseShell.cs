using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MatterRecord.Contents.TortoiseShell;

public class DashingTortoiseShell : ModProjectile
{
    public override string Texture => base.Texture.Replace(nameof(DashingTortoiseShell), nameof(TortoiseShell));
    public Player Player => Main.player[Projectile.owner];

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 48;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.knockBack = 8;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 10;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        //Projectile.hide = true;
        base.SetDefaults();
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.FinalDamage *= Player.velocity.Length() / 18f;
        float k = (1 + MathF.Abs(MathF.Asin(MathF.Sin(Projectile.ai[2]))) / MathHelper.PiOver2 * 0.18f);
        modifiers.FinalDamage *= k;

        base.ModifyHitNPC(target, ref modifiers);
    }

    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.FinalDamage *= Player.velocity.Length() / 18f;

        modifiers.FinalDamage *= (1 + MathF.Asin(MathF.Sin(Projectile.ai[2])) / MathHelper.PiOver2 * 0.18f);

        Vector2 cache = target.velocity;
        target.velocity = Player.velocity;
        Player.velocity = cache;
        var mtarget = target.GetModPlayer<TortoiseShellPlayer>();
        mtarget.SetSyncVelocity(target.velocity);
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            VelocitySync.Get(Player.whoAmI, Player.velocity).Send();
            VelocitySync.Get(target.whoAmI, target.velocity).Send();
        }
        base.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!target.CanBeChasedBy()) return;
        Player.immune = true;
        Player.immuneTime = 3;
        float mass1 = 68.818f;// new Vector2(40, 56).Length();
        float mass2 = new Vector2(target.width, target.height).Length();

        Vector2 avgVec = (mass1 * Player.velocity + mass2 * target.velocity) / (mass1 + mass2);
        Player.velocity = avgVec * 2 - Player.velocity;
        target.velocity = avgVec * 2 - target.velocity;
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            VelocitySync.Get(Player.whoAmI, Player.velocity).Send();
            NPCVelocitySync.Get(target.whoAmI, target.velocity).Send();
        }
        base.OnHitNPC(target, hit, damageDone);
    }

    public override void AI()
    {
        Projectile.Center = Player.Center;
        var mplr = Player.GetModPlayer<TortoiseShellPlayer>();
        mplr.TortoiseShellActive = true;
        if (Player.channel && Projectile.ai[1] == 0)// && player.velocity.Y == 0
        {
            Projectile.timeLeft = 180;
            Player.velocity.X *= .95f;
            Player.itemTime = Player.itemAnimation = 180;
            Projectile.damage = 0;
            Projectile.friendly = false;
            Projectile.ai[0]++;
            Player.noKnockback = true;
        }
        else
        {
            mplr.TortoiseDashing = true;
            if (Projectile.ai[1] == 0)
            {
                Projectile.damage = Player.GetWeaponDamage(Player.HeldItem);
                if (Main.myPlayer == Projectile.owner)
                {
                    Player.velocity = (Main.MouseWorld - Player.Center).SafeNormalize(default) * mplr.timer * 36;
                    Projectile.ai[2] = Player.velocity.ToRotation();
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        VelocitySync.Get(Player.whoAmI, Player.velocity).Send();
                }
            }
            Projectile.ai[1]++;
            Projectile.friendly = true;
            if (Player.velocity.Length() < 0.1f && Player.velocity.Y == 0)
            {
                Projectile.timeLeft = 0;
                Player.itemAnimation = 0;
                Player.itemTime = Player.itemAnimation = 0;
            }
            Projectile.rotation += Player.velocity.Length() * .01f * (Player.velocity.X == 0 ? 1 : Math.Sign(Player.velocity.X));
        }
        if (Main.netMode != NetmodeID.Server)
        {
            for (int n = ProjectileID.Sets.TrailCacheLength[Type] - 1; n > 0; n--)
            {
                Projectile.oldPos[n] = Projectile.oldPos[n - 1];
                Projectile.oldRot[n] = Projectile.oldRot[n - 1];
            }
            Projectile.oldPos[0] = Projectile.Center;
            Projectile.oldRot[0] = Projectile.rotation;
        }

        mplr.timer = MathHelper.Clamp(Projectile.ai[0] / 180, 0, 1f);
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        var mplr = Player.GetModPlayer<TortoiseShellPlayer>();
        var direction = Player.gravDir < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;

        if (Player.whoAmI == Main.myPlayer)
        {


            Main.spriteBatch.Draw(
                ModAsset.bar.Value,
                Player.Center + new Vector2(0, -64 * Player.gravDir) - Main.screenPosition,
                null,
                Color.Black * MathHelper.SmoothStep(0, 1, Projectile.timeLeft / 180f),
                0,
                new Vector2(64, 8),
                1,
                direction,
                0);

            Main.spriteBatch.Draw(
                ModAsset.bar.Value,
                Player.Center + new Vector2(0, -64 * Player.gravDir) - Main.screenPosition,
                new Rectangle(0, 0, (int)(mplr.timer * 128), 16),
                Color.White * MathHelper.SmoothStep(0, 1, Projectile.timeLeft / 180f),
                0,
                new Vector2(64, 8),
                1,
                direction,
                0);
        }

        int max = ProjectileID.Sets.TrailCacheLength[Type];
        for (int n = 1; n < max; n++)
            Main.spriteBatch.Draw(
                ModAsset.TortoiseShell.Value,
                Projectile.oldPos[n] - Main.screenPosition,
                null,
                Lighting.GetColor(Player.Center.ToTileCoordinates()) * ((max - (float)n) / max * .5f),
                Projectile.oldRot[n],
                new Vector2(36, 24),
                1,
                direction,
                0);

        Main.spriteBatch.Draw(
            ModAsset.TortoiseShell.Value,
            Player.Center - Main.screenPosition,
            null,
            Lighting.GetColor(Player.Center.ToTileCoordinates()),
            Projectile.rotation,
            new Vector2(36, 24),
            1,
            direction,
            0);

        return false;
    }
}