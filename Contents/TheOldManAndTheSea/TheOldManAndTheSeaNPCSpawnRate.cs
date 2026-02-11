namespace MatterRecord.Contents.TheOldManAndTheSea;
public class TheOldManAndTheSeaNPCSpawnRate : GlobalNPC
{
    public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
    {
        if (player.HeldItem.ModItem is not TheOldManAndTheSea) return;
        spawnRangeX = spawnRangeX * 3 / 2;
        spawnRangeY = spawnRangeY * 3 / 2;

        safeRangeX = safeRangeX * 2 / 3;
        safeRangeY = safeRangeY * 2 / 3;
        base.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (player.HeldItem.ModItem is not TheOldManAndTheSea) return;
        spawnRate /= 4;
        if (spawnRate < 1) spawnRate = 1;
        maxSpawns *= 6;

        base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
    }
}
