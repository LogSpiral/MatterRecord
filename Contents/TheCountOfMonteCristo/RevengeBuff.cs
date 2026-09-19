using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheCountOfMonteCristo
{
    public class RevengeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
      
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.statDefense += 5;
        }
    }
}