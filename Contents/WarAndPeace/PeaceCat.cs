namespace MatterRecord.Contents.WarAndPeace;

public class PeaceCat : CatProj
{
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        // 弹幕存续由「是否装备战争与和平」决定，与 buff 解耦（需求：弹幕与 buff 分离）。
        // 未装备、或饰品被设为不可见时，CatVisible 都不会被点亮，弹幕随 timeLeft 自然消失；
        // 日期判断保证跨日切换时旧猫自动退场，同一时刻场上只有一只猫。
        if (!player.dead && player.GetModPlayer<WarAndPeacePlayer>().CatVisible && WarAndPeace.IsPeaceDay)
            Projectile.timeLeft = 2;
        base.AI();
    }
}
