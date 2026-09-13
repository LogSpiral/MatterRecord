namespace MatterRecord.Contents.WarAndPeace;

public class Peace : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        //Main.vanityPet[Type] = true;
    }

}
public class Holiday_Peace : Peace;
