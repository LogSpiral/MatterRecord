using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TortoiseShell;

public class TortoiseShell : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 100;
        Item.DamageType = DamageClass.Melee;
        Item.useTime = Item.useAnimation = 60;
        Item.knockBack = 8;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.width = 72;
        Item.height = 48;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(0, 0, 15, 0);
        Item.shoot = ModContent.ProjectileType<DashingTortoiseShell>();
        Item.shootSpeed = 0.01f;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.noMelee = true;
        Item.ResearchUnlockCount = 1;

        ItemID.Sets.ShimmerTransformToItem[ItemID.TurtleShell] = Type;
        ItemID.Sets.ShimmerTransformToItem[Type] = ItemID.TurtleShell;

        base.SetDefaults();
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;
}



