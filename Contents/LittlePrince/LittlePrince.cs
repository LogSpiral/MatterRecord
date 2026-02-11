using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ObjectData;

namespace MatterRecord.Contents.LittlePrince;



public class LittlePrince : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.LittlePrince;
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.AbigailsFlower);
        base.AddRecipes();
    }
    //public override string Texture => $"Terraria/Images/Item_{ItemID.JungleRose}";
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 27;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(0, 1);
        Item.accessory = true;
        base.SetDefaults();
    }

    public override void UpdateEquip(Player player)
    {
        player.GetModPlayer<LittlePrincePlayer>().EquippedRose = true;
        base.UpdateEquip(player);
    }
}