namespace MatterRecord.Contents.WarAndPeace;
public class War : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        //Main.vanityPet[Type] = true;
    }

    public override void Update(Player player, ref int buffIndex)
    { // This method gets called every frame your buff is active on your player.
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeeded(ref unused, ModContent.ProjectileType<WarCat>(), buffIndex);
    }
}
public class Holiday_War : War;
