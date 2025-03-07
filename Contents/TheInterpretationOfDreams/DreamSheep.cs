using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.RGB;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class DreamSheep : ModProjectile
    {
        public override bool IsLoadingEnabled(Mod mod)
        {
            return false;
        }
    }
    public class CybrogDream : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";
        public override void SetDefaults()
        {
            base.SetDefaults();
        }
    }
}
