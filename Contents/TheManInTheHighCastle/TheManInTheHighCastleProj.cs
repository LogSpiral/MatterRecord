using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using Microsoft.Xna.Framework;
using System;
using System.Reflection.Emit;
using Terraria.Audio;
using Terraria.GameContent;

namespace MatterRecord.Contents.TheManInTheHighCastle;

public class TheManInTheHighCastleProj : ModProjectile
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.ScytheWhip}";
    public override void SetDefaults()
    {
        Projectile.timeLeft = 10;
        Projectile.aiStyle = -1;
        Projectile.DamageType = DamageClass.Default;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        Projectile.width = 200;
        Projectile.height = 200;
        base.SetDefaults();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var player = Main.player[Projectile.owner];
        float angle = (float)Main.time * .1f * MathF.PI;
        angle -= MathF.Sin(angle) * 0.75f;
        angle *= player.direction;
        for (int n = 0; n < 2; n++)
        {
            angle %= MathHelper.TwoPi;
            if (angle < MathHelper.Pi ^ false ^ Projectile.ai[1] > 0) // 因为1.4.4还没法获取到Layer所以就先这样吧（
                Main.spriteBatch.Draw(TextureAssets.Item[ItemID.ScytheWhip].Value,
                    player.Center - Main.screenPosition,
                    null,
                    Color.Lerp(lightColor, Color.Black, 0.2f - 0.2f * MathF.Sin(angle) * Projectile.ai[1]) with { A = lightColor.A },
                    MathHelper.PiOver4,
                    angle,
                    0,
                    new Vector2(0, 32),
                    new Vector2(2, MathF.Abs(Projectile.ai[1]) * 1.5f) * (Projectile.ai[2] * .1f),
                    player.direction < 0);
            angle += MathHelper.Pi;
        }

        return false;
    }
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        TheManInTheHighCastlePlayer mPlr = owner.GetModPlayer<TheManInTheHighCastlePlayer>();
        if (mPlr.GrapCountCache == 0
            || owner.velocity.LengthSquared() < 0.1f
            || owner.dead)
        {
            Projectile.ai[2]--;
            if (Projectile.ai[2] < 0)
                Projectile.timeLeft = 0;
        }
        else
        {
            Projectile.timeLeft = 10;
            if (Projectile.ai[2] < 10)
                Projectile.ai[2]++;
        }
        mPlr.Rotation += owner.velocity.X * .1f;
        mPlr.UseRotation = true;
        Projectile.Center = owner.Center;
        owner.heldProj = Projectile.whoAmI;
        float angle = (float)Main.time * .1f * MathF.PI;
        angle -= MathF.Sin(angle) * 0.75f;
        angle *= owner.direction;
        owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, angle - MathHelper.PiOver2);
        owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, angle + MathHelper.PiOver2);
        if (!Main.dedServ && Projectile.ai[0] % 10 == 0)
        {
            if (Projectile.ai[0] % 20 == 0)
                SoundEngine.PlaySound(SoundID.Item71, owner.Center);
            var u = UltraSwoosh.NewUltraSwooshOnDefaultCanvas(30, Main.rand.NextFloat(100, 125), owner.Center, (angle - 2, angle));
            u.xScaler = 4f / 3 / MathF.Abs(Projectile.ai[1]);
            u.weaponTex = TextureAssets.Projectile[Type].Value;
            u.ColorVector = new(0, 1, 0);
            u.rotation = mPlr.Rotation;

            u = UltraSwoosh.NewUltraSwooshOnDefaultCanvas(30, Main.rand.NextFloat(100, 125), owner.Center, (angle - 1, angle + 1));
            u.xScaler = 4f / 3 / MathF.Abs(Projectile.ai[1]);
            u.weaponTex = TextureAssets.Projectile[Type].Value;
            u.ColorVector = new(0, 1, 0);
            u.rotation = mPlr.Rotation;
        }
        // owner.direction = MathF.Sign(owner.velocity.X) * ((int)Projectile.ai[0] % 20 - 10) / -10;
        Projectile.ai[0]++;
        Projectile.ai[1] = MathF.Cos(Projectile.ai[0] / 10f);
        owner.itemAnimation = 2;
        owner.itemTime = 2;
        owner.noKnockback = true;
    }
}
