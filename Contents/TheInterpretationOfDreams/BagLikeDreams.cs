using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class DyeTraderDream : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";
        public override void RightClick(Player player)
        {
            for (int n = 0; n < 3; n++)
                player.QuickSpawnItem(Item.GetSource_GiftOrReward(), Main.rand.Next(DreamWorld.availableDyeId));
            base.RightClick(player);
        }
        public override bool CanRightClick() => true;
    }
    public abstract class BagLikeDreams(int type, int stack = 1) : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

        protected BagLikeDreams() : this(0) { }
        public override void RightClick(Player player)
        {
            player.QuickSpawnItem(Item.GetSource_GiftOrReward(), type, stack);
            base.RightClick(player);
        }
        public override bool CanRightClick() => true;
    }
    public class GuideDream() : BagLikeDreams(Main.rand.Next([ItemID.WarriorEmblem, ItemID.SorcererEmblem, ItemID.RangerEmblem, ItemID.SummonerEmblem]));
    public class TavernkeepDream() : BagLikeDreams(ItemID.DefenderMedal, NPC.downedGolemBoss ? 8 : NPC.downedMechBossAny ? 4 : 2);
    public class ClothierDream() : BagLikeDreams(ItemID.GoldenKey);
    public class DreamOfStylist() : BagLikeDreams(ModContent.ItemType<CosmosScissors>());

}
