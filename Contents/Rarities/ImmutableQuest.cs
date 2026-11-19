using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;

namespace MatterRecord.Contents.Rarities
{
    public class ImmutableQuest : ModRarity
    {
        public override Color RarityColor => ItemRarity.GetColor(ItemRarityID.Quest);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }
}