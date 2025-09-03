using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public abstract class BagLikeDreams(int index, Func<bool> condition, int type, int stack = 1, Func<IEnumerable<(int, int)>> itemGetter = null) : BasicDream(TextureAssets.NpcHead[index], condition)
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

        public override void SetDefaults()
        {
            Item.maxStack = 9999;
            base.SetDefaults();
        }

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

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            return base.PreDrawInWorld(spriteBatch, lightColor, alphaColor, ref rotation, ref scale, whoAmI);
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            return base.PreDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }
    }

    public class GuideDream() : BagLikeDreams(1, () => true, 0, 0, () => [(ItemID.WarriorEmblem, 1), (ItemID.SummonerEmblem, 1), (ItemID.SorcererEmblem, 1), (ItemID.RangerEmblem, 1)])
    {
        public override bool CanRightClick() => Main.hardMode;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (!Main.hardMode)
                tooltips.Add(new TooltipLine(Mod, "Unlock", this.GetLocalizedValue("Unlock")) { OverrideColor = Color.Lerp(Color.Gray, Color.DarkGray, Main.mouseTextColor / 255f) });
            base.ModifyTooltips(tooltips);
        }

        public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.GuideVoodooDoll);
    }

    public class TavernkeepDream() : BagLikeDreams(24, () => NPC.savedBartender, 0, 0, () => [(ItemID.DefenderMedal, NPC.downedGolemBoss ? 8 : NPC.downedMechBossAny ? 4 : 2)])
    {
        public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.DD2ElderCrystal);
    }

    public class ClothierDream() : BagLikeDreams(7, () => NPC.downedBoss3, ItemID.GoldenKey)
    {
        public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.BlackThread);
    }

    public class StylistDream() : BagLikeDreams(20, () => NPC.savedStylist, ModContent.ItemType<CosmosScissors>())
    {
        public override bool CanRightClick() => NPC.downedPlantBoss;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (!NPC.downedPlantBoss)
                tooltips.Add(new TooltipLine(Mod, "Unlock", this.GetLocalizedValue("Unlock")) { OverrideColor = Color.Lerp(Color.Gray, Color.DarkGray, Main.mouseTextColor / 255f) });
            base.ModifyTooltips(tooltips);
        }

        public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.HairDyeRemover);
    }

    public class DyeTraderDream() : BagLikeDreams(14, NPC.SpawnAllowed_DyeTrader, 0, 0, () => [(Main.rand.Next(DreamWorld.availableDyeId), 3)])
    {
        public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.SilverDye);
    }
}