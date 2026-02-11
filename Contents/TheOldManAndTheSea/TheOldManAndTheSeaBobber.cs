namespace MatterRecord.Contents.TheOldManAndTheSea;

public class TheOldManAndTheSeaBobber : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.BobberWooden);
        DrawOriginOffsetY = 0;
    }
}
