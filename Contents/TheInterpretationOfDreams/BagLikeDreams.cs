using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public abstract class BagLikeDreams(int type, int stack = 1, Func<IEnumerable<(int, int)>> itemGetter = null) : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";
        public override void SetDefaults()
        {
            Item.maxStack = 9999;
            base.SetDefaults();
        }
        protected BagLikeDreams() : this(0) { }
        public override void RightClick(Player player)
        {
            var collection = itemGetter?.Invoke();
            if (collection != null)
                foreach (var pair in collection)
                    player.QuickSpawnItem(Item.GetSource_GiftOrReward(), pair.Item1, pair.Item2);
            else
                player.QuickSpawnItem(Item.GetSource_GiftOrReward(), type, stack);

            base.RightClick(player);
        }
        public override bool CanRightClick() => true;
    }
    public class GuideDream() : BagLikeDreams(0, 0, () => [(ItemID.WarriorEmblem,1), (ItemID.SummonerEmblem, 1), (ItemID.SorcererEmblem, 1), (ItemID.RangerEmblem, 1)]);
    public class TavernkeepDream() : BagLikeDreams(0, 0, () => [(ItemID.DefenderMedal, NPC.downedGolemBoss ? 8 : NPC.downedMechBossAny ? 4 : 2)]);
    public class ClothierDream() : BagLikeDreams(ItemID.GoldenKey);
    public class StylistDream() : BagLikeDreams(ModContent.ItemType<CosmosScissors>());
    public class DyeTraderDream() : BagLikeDreams(0, 0, () => [(Main.rand.Next(DreamWorld.availableDyeId), 3)]);
}
