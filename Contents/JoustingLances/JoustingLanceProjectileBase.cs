using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.GameContent;

namespace MatterRecord.Contents.JoustingLances;

public abstract class JoustingLanceProjectileBase : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.JoustingLance);
        Projectile.aiStyle = -1;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        modifiers.FinalDamage *= 0.1f + Main.player[Projectile.owner].velocity.Length() / 7f * 0.9f;
        modifiers.Knockback *= Main.player[Projectile.owner].velocity.Length() / 7f;
    }
    public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
    {
        modifiers.FinalDamage *= 0.1f + Main.player[Projectile.owner].velocity.Length() / 7f * 0.9f;
        modifiers.Knockback *= Main.player[Projectile.owner].velocity.Length() / 7f;
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        Vector2 center = player.RotatedRelativePoint(player.MountedCenter);
        Projectile.direction = player.direction;
        player.heldProj = Projectile.whoAmI;
        player.MatchItemTimeToItemAnimation();
        Projectile.Center = center;
        int itemAnimationMax = player.itemAnimationMax;
        int itemAnimation = player.itemAnimation;
        int num = Main.player[Projectile.owner].itemAnimationMax / 3;
        float num2 = MathHelper.Min(itemAnimation, num);
        float num3 = (float)itemAnimation - num2;
        float num4 = 28f;
        float num5 = 0.4f;
        float num6 = 0.4f;
        bool flag2 = true;
        Projectile.spriteDirection = -Projectile.direction;

        Projectile.alpha -= 40;
        if (Projectile.alpha < 0)
            Projectile.alpha = 0;

        float num7 = (float)(itemAnimationMax - num) - num3;
        float num8 = (float)num - num2;
        float num9 = num4 + num5 * num7 - num6 * num8;
        Projectile.position += Projectile.velocity * num9;

        if (flag2 && player.channel && player.itemAnimation < num)
            player.SetDummyItemTime(num);

        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

        Projectile.ai[0] += 1f;
        bool flag3 = Projectile.ai[0] >= (float)itemAnimationMax;
        if (flag2)
            flag3 &= !player.channel;

        if (flag3)
            Projectile.Kill();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var tex = TextureAssets.Projectile[Projectile.type].Value;
        Main.EntitySpriteDraw(tex,
            Projectile.Center - Projectile.velocity.SafeNormalize(default) * 128f - Main.screenPosition,
            null,
            lightColor,
            Projectile.rotation,
            new Vector2(0, tex.Height),
            1,
            0,
            0);
        return false;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!target.friendly && target.life < 0 && RecorderSystem.ShouldSpawnRecordItem<DonQuijoteDeLaMancha.DonQuijoteDeLaMancha>())
            Main.LocalPlayer.QuickSpawnItem(target.GetItemSource_Loot(), ModContent.ItemType<DonQuijoteDeLaMancha.DonQuijoteDeLaMancha>());
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float f = Projectile.rotation
            - (float)Math.PI / 4f
            - (float)Math.PI / 2f
            - ((Projectile.spriteDirection == 1) ? ((float)Math.PI) : 0f);

        float collisionPoint2 = 0f;
        float num10 = 95f;
        Rectangle lanceHitboxBounds = new Rectangle(0, 0, 300, 300);
        lanceHitboxBounds.X = (int)Projectile.position.X - lanceHitboxBounds.Width / 2;
        lanceHitboxBounds.Y = (int)Projectile.position.Y - lanceHitboxBounds.Height / 2;
        if (
            lanceHitboxBounds.Intersects(targetHitbox) &&
            Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Center,
                Projectile.Center + f.ToRotationVector2() * num10,
                23f * Projectile.scale,
                ref collisionPoint2))
            return true;

        return false;
    }
}
