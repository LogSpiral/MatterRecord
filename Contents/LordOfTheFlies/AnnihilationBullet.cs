using LogSpiralLibrary;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace MatterRecord.Contents.LordOfTheFlies;

public class AnnihilationBullet : ModProjectile
{
    public override void SetDefaults()
    {
        ProjectileID.Sets.TrailingMode[Projectile.type] = 1;
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
        Projectile.width = 6;
        Projectile.scale = 1f;
        Projectile.height = 6;
        Projectile.tileCollide = true;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.timeLeft = 180;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.light = .5f;
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // 因为湮灭弹固定源自于审判模式所以固定有暴击双倍，这里平衡回来就得 / 40
        modifiers.FlatBonusDamage += target.lifeMax / 40;
        base.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        for (int n = 0; n < 30; n++)
        {
            Dust.NewDustPerfect(
                target.Center,
                MyDustId.GreenBubble,
                Projectile.velocity * .25f + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f),
                0,
                Color.White,
                Main.rand.NextFloat(0, 1.5f)).noGravity = true;
        }
        base.OnHitNPC(target, hit, damageDone);
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity = oldVelocity;
        Collision.HitTiles(Projectile.position, Projectile.velocity * .25f, Projectile.width, Projectile.height);
        for (int n = 0; n < 3; n++)
        {
            Dust.NewDustPerfect(Projectile.Center,
                MyDustId.GreenBubble,
                Projectile.velocity * .25f + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 4f),
                0,
                Color.White,
                Main.rand.NextFloat(0, 1.5f)).noGravity = true;
        }
        return false;
    }

    public override void AI()
    {
        if (Projectile.velocity != Vector2.Zero)
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }
        Projectile.velocity *= 1.02f;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float point = 0;
        return
            projHitbox.Intersects(targetHitbox)
            || Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + Projectile.velocity * 2, 10, ref point);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.DrawShaderTail(Projectile, LogSpiralLibraryMod.HeatMap[6].Value, LogSpiralLibraryMod.AniTex[0].Value, LogSpiralLibraryMod.BaseTex[12].Value, 20, new Vector2(Projectile.width, Projectile.height) * .5f, (1 - Projectile.timeLeft / 180f).HillFactor2());
        spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, new Vector2(10, 3), new Vector2(2, 1), 0, 0);
        return false;
    }
}