namespace MatterRecord.Contents.WarAndPeace;
public class PeaceCat : CatProj
{
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.dead && (player.HasBuff(ModContent.BuffType<Peace>()) || player.HasBuff(ModContent.BuffType<Holiday_Peace>())))
            Projectile.timeLeft = 2;
        base.AI();
    }
}
