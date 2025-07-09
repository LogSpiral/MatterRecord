using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent;
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
    public class CybrogDream() : BasicDream(TextureAssets.NpcHead[16], () => NPC.downedPlantBoss)
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";
        public override void SetDefaults()
        {
            base.SetDefaults();
        }
        public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Nanites);
    }
}
