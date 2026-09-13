namespace MatterRecord.Contents.WarAndPeace;

public class War : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        //Main.vanityPet[Type] = true;
    }

}
public class Holiday_War : War;
