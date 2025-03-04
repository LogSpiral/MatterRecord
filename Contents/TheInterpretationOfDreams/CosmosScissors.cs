using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class CosmosScissors : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.StylistKilLaKillScissorsIWish}";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.StylistKilLaKillScissorsIWish);
            Item.damage = 200;
            base.SetDefaults();
        }
    }
}
