using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TheOldManAndTheSea
{
    public class TheOldManAndTheSea : ModItem
    {
        //public override string Texture => $"Terraria/Images/Item_{ItemID.GoldenFishingRod}";
        public override Color? GetAlpha(Color lightColor)
        {
            return null;
        }
        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanFishInLava[Item.type] = true; // Allows the pole to fish in lava
        }

        public override void SetDefaults()
        {
            // These are copied through the CloneDefaults method:
            // Item.width = 24;
            // Item.height = 28;
            // Item.useStyle = ItemUseStyleID.Swing;
            // Item.useAnimation = 8;
            // Item.useTime = 8;
            // Item.UseSound = SoundID.Item1;
            Item.CloneDefaults(ItemID.WoodFishingPole);
            Item.buyPrice(2);
            Item.fishingPole = 75; // Sets the poles fishing power
            Item.shootSpeed = 12f; // Sets the speed in which the bobbers are launched. Wooden Fishing Pole is 9f and Golden Fishing Rod is 17f.
            Item.shoot = ModContent.ProjectileType<TheOldManAndTheSeaBobber>(); // The bobber projectile. Note that this will be overridden by Fishing Bobber accessories if present, so don't assume the bobber spawned is the specified projectile. https://terraria.wiki.gg/wiki/Fishing_Bobbers
        }

        // Grants the High Test Fishing Line bool if holding the item.
        // NOTE: Only triggers through the hotbar, not if you hold the item by hand outside of the inventory.
        public override void HoldItem(Player player)
        {
            player.accFishingLine = true;
        }

        // Overrides the default shooting method to fire multiple bobbers.
        // NOTE: This will allow the fishing rod to summon multiple Duke Fishrons with multiple Truffle Worms in the inventory.
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            //int bobberAmount = Main.rand.Next(3, 6); // 3 to 5 bobbers
            //float spreadAmount = 75f; // how much the different bobbers are spread out.

            //for (int index = 0; index < bobberAmount; ++index)
            //{
            //    Vector2 bobberSpeed = velocity + new Vector2(Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f, Main.rand.NextFloat(-spreadAmount, spreadAmount) * 0.05f);

            //    // Generate new bobbers
            //    Projectile.NewProjectile(source, position, bobberSpeed, type, 0, 0f, player.whoAmI);
            //}
            return true;
        }

        public override void ModifyFishingLine(Projectile bobber, ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            lineOriginOffset = new Vector2(43, -30);
            lineColor = Color.White;
        }
    }
    public class TheOldManAndTheSeaBobber : ModProjectile
    {
        public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.BobberWooden}";

        // This holds the index of the fishing line color in the PossibleLineColors array.

        public override void SetDefaults()
        {
            // These are copied through the CloneDefaults method
            // Projectile.width = 14;
            // Projectile.height = 14;
            // Projectile.aiStyle = 61;
            // Projectile.bobber = true;
            // Projectile.penetrate = -1;
            // Projectile.netImportant = true;
            Projectile.CloneDefaults(ProjectileID.BobberWooden);

            DrawOriginOffsetY = -8; // Adjusts the draw position
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return true;
        }
    }

    public class TheOldManAndTheSeaNPCSpawnRate : GlobalNPC
    {
        public override void EditSpawnRange(Player player, ref int spawnRangeX, ref int spawnRangeY, ref int safeRangeX, ref int safeRangeY)
        {
            if (player.HeldItem.ModItem is not TheOldManAndTheSea) return;
            spawnRangeX = spawnRangeX * 3 / 2;
            spawnRangeY = spawnRangeY * 3 / 2;

            safeRangeX = safeRangeX * 2 / 3;
            safeRangeY = safeRangeY * 2 / 3;
            base.EditSpawnRange(player, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);
        }
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (player.HeldItem.ModItem is not TheOldManAndTheSea) return;
            spawnRate /= 4;
            if (spawnRate < 1) spawnRate = 1;
            maxSpawns *= 6;

            base.EditSpawnRate(player, ref spawnRate, ref maxSpawns);
        }
    }
}
