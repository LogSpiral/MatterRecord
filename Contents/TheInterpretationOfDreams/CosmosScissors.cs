using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public class CosmosScissors : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.StylistKilLaKillScissorsIWish}";

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.StylistKilLaKillScissorsIWish);
            Item.damage = 200;
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useTurn = false;
            Item.useTime = 18;
            Item.useAnimation = 18;
            Item.knockBack = 7f;
            Item.shoot = ModContent.ProjectileType<CosmosScissorsProj>();
            Item.shootSpeed = 20f;
            Item.UseSound = SoundID.Item1;
        }

        public override bool CanUseItem(Player player)
            => player.ownedProjectileCounts[ModContent.ProjectileType<CosmosScissorsProj>()] == 0;
    }
}