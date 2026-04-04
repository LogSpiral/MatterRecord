namespace MatterRecord.Contents.CompendiumOfMateriaMedica
{
    public class FragrantHerbs : ModBuff
    {
        public override void SetStaticDefaults()
        {

            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}