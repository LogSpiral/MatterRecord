using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent;

namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithSubBoulder : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Boulder}";

    public float Timer { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
    public int TargetIndex => (int)Projectile.ai[0];
    public int TargetType => (int)Projectile.ai[1];

    public Vector2 TargetPosition => TargetType switch
    {
        1 => Main.player[(int)Projectile.ai[0]].Center,
        2 => Main.npc[(int)Projectile.ai[0]].Center,
        0 or _ => Projectile.Center + Projectile.velocity
    };

    public override void SetDefaults()
    {
        //Projectile.friendly = true;
        //Projectile.hostile = true;
        Projectile.width = 34;
        Projectile.height = 34;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
    }

    public override void AI()
    {
        // ai[0] 目标索引
        // ai[1] 目标类型
        // localAI[0] 计时器
        if (Timer == 0)
        {
            Projectile.localAI[1] = Projectile.Center.X;
            Projectile.localAI[2] = Projectile.Center.Y;
        }
        Timer++;

        var target = TargetPosition;
        if (TargetType != 0)
            Projectile.velocity = TargetPosition - Projectile.Center;
        float t = Timer / Projectile.ai[2];
        //float factor = t switch
        //{
        //    < 0.25f => 0,
        //    < 0.75f => MathHelper.SmoothStep(0, -0.25f, Utils.GetLerpValue(0.25f, 0.75f, t)),
        //    _ => (t - 0.75f) * (t - 0.75f) * 20f - 0.25f
        //};
        float factor = t switch
        {
            < 0.25f => 0,
            < 0.75f => MathHelper.SmoothStep(0, -0.25f, Utils.GetLerpValue(0.25f, 0.75f, t))
                        + 12 * MathF.Pow(t, 4) - 24 * MathF.Pow(t, 3) + 16.5f * MathF.Pow(t, 2) - 4.5f * t + 27 / 64f,
            _ => 6 * MathF.Pow(t - 0.75f, 2) - 0.25f + 56 * MathF.Pow(t - 0.75f, 3)
        };
        Projectile.Center = Vector2.Lerp(new Vector2(Projectile.localAI[1], Projectile.localAI[2]), target, factor);

        Projectile.oldPos[1] = Projectile.oldPos[0];
        Projectile.oldPos[0] = Projectile.Center;

        if (Timer > Projectile.ai[2])
            Projectile.Kill();
        base.AI();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float size = 1f;
        float t = Timer / Projectile.ai[2];
        if (t < .75f)
            size = MathHelper.SmoothStep(0, 1, t / .75f);
        Main.EntitySpriteDraw(
        TextureAssets.Projectile[Type].Value,
        Projectile.Center - Main.screenPosition,
        null,
        lightColor,
        Projectile.rotation,
        new Vector2(17f),
        size, 0, 0);


        if (t < 0.75f)
        {
            var target = TargetPosition;

            float k = t / 0.75f;

            static float ssmoothstep(float k) => k * k * k * (4 - 3 * k);

            float f = 1.0f;
            if (k < 0.25)
                f = 1.0f - ssmoothstep(1.0f - 4.0f * k);
            if (k > 0.5)
                f = 1.0f - ssmoothstep(2.0f * k - 1.0f);
            f *= .25f;
            var barColor = Color.White with { A = 0 } * f;

            float r = 32 * k;
            float r2 = 16 * k + 16;
            for (int n = 0; n < 6; n++)
            {
                float offset = n / 6f * MathF.Tau + Main.GlobalTimeWrappedHourly * 4;
                Vector2 start = Projectile.Center + offset.ToRotationVector2() * r2;
                Vector2 end = target + (offset + MathF.PI * .666f).ToRotationVector2() * r;

                Main.EntitySpriteDraw(
                    ModAsset.ZenithBoulderBar.Value,
                    start - Main.screenPosition,
                    null,
                    barColor,
                    (end - start).ToRotation(),
                    new Vector2(0, 0.5f), new Vector2(1, 2), 0, 0);
            }
        }


        return false;
    }

    public override bool ShouldUpdatePosition() => false;

    public override void OnKill(int timeLeft)
    {
        var velocity = Projectile.oldPos[0] - Projectile.oldPos[1];
        ZenithBoulderExSystem.Instance.ScreenOffset += velocity * .25f;
        SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, Projectile.Center);
        for (int n = 0; n < 30; n++)
        {
            Dust.NewDustPerfect(Projectile.Center, DustID.Stone, -velocity * Main.rand.NextFloat(0.5f, 1.0f) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(64), 0, default, Main.rand.NextFloat(0.5f, 2f));
        }
        Projectile.NewProjectile(
            Projectile.GetSource_FromThis(),
            Projectile.Center,
            default,
            ModContent.ProjectileType<ZenithBoulderExplode>(),
            Projectile.damage,
            Projectile.knockBack,
            Projectile.owner);
        base.OnKill(timeLeft);
    }
}
