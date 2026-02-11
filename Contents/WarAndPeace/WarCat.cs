namespace MatterRecord.Contents.WarAndPeace;
public class WarCat : CatProj
{
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.dead && (player.HasBuff(ModContent.BuffType<War>()) || player.HasBuff(ModContent.BuffType<Holiday_War>())))
            Projectile.timeLeft = 2;
        base.AI();
    }
}
