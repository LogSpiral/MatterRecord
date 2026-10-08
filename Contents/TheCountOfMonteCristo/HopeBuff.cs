using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheCountOfMonteCristo
{
    public class HopeBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
          
            Main.debuff[Type] = false;
            Main.buffNoTimeDisplay[Type] = true;   // 无限时长
        }

        public override void Update(Player player, ref int buffIndex)
        {
            // 每帧标记该buff为无限时长（防止倒计时）
            player.buffTime[buffIndex] = 2;
        }
    }
}