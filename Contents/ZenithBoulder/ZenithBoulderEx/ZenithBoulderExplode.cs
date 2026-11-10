using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithBoulderExplode : ModProjectile
{
    public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.Boulder}";

    public override void SetDefaults()
    {
        Projectile.timeLeft = 2;
        Projectile.penetrate = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.width = 160;
        Projectile.height = 160;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = true;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        return false;
    }
}
