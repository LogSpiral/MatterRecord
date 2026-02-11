using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;

namespace MatterRecord.Contents.CantSeword;

public class CantSewordProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 28;
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.Arkhalis);
        Projectile.aiStyle = -1;
        base.SetDefaults();
    }

    public override void AI()
    {
        var projectile = Projectile;
        Player player = Main.player[projectile.owner];
        float num = (float)Math.PI / 2f;
        Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
        int num2 = 2;
        float num3 = 0f;
        num = 0f;
        if (projectile.spriteDirection == -1)
            num = (float)Math.PI;

        if (++projectile.frame >= 28)
            projectile.frame = 0;

        projectile.soundDelay--;
        if (projectile.soundDelay <= 0)
        {
            SoundEngine.PlaySound(SoundID.Item1, projectile.Center);
            projectile.soundDelay = 12;
        }

        if (Main.myPlayer == projectile.owner)
        {
            if (player.channel && !player.noItems && !player.CCed)
            {
                float num42 = 1f;
                if (player.inventory[player.selectedItem].shoot == projectile.type)
                    num42 = player.inventory[player.selectedItem].shootSpeed * projectile.scale;

                Vector2 vec = Main.MouseWorld - vector;
                vec.Normalize();
                if (vec.HasNaNs())
                    vec = Vector2.UnitX * player.direction;

                vec *= num42;
                if (vec.X != projectile.velocity.X || vec.Y != projectile.velocity.Y)
                    projectile.netUpdate = true;

                projectile.velocity = vec;
            }
            else
            {
                projectile.Kill();
            }
        }

        Vector2 vector20 = projectile.Center + projectile.velocity * 3f;
        Lighting.AddLight(vector20, 0.8f, 0.8f, 0.8f);
        if (Main.rand.NextBool(3))
        {
            int num43 = Dust.NewDust(vector20 - projectile.Size / 2f, projectile.width, projectile.height, DustID.WhiteTorch, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 2f);
            Main.dust[num43].noGravity = true;
            Main.dust[num43].position -= projectile.velocity;
        }

        projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - projectile.Size / 2f;
        projectile.rotation = projectile.velocity.ToRotation() + num;
        projectile.spriteDirection = projectile.direction;
        projectile.timeLeft = 2;
        player.ChangeDir(projectile.direction);
        player.heldProj = projectile.whoAmI;
        player.SetDummyItemTime(num2);
        player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(projectile.velocity.Y * (float)projectile.direction, projectile.velocity.X * (float)projectile.direction) + num3);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        lightColor *= .25f;
        return base.PreDraw(ref lightColor);
    }
}
