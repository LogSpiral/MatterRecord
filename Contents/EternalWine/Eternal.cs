namespace MatterRecord.Contents.EternalWine;

public class Eternal : ModBuff
{
    public override void Update(Player player, ref int buffIndex)
    {
        player.immune = true;
        player.immuneTime = 2;
        base.Update(player, ref buffIndex);
    }
}
