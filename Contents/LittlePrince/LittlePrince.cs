using MatterRecord.Contents.Rarities;
using MatterRecord.Contents.Recorder;

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
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ModContent.RarityType<ImmutableQuest>();
        Item.accessory = true;
        base.SetDefaults();
    }

    /// <summary>
    /// 装备状态与可见性标记。改用 <see cref="UpdateAccessory"/> 是因为它带 hideVisual 参数，
    /// 可以拿到「饰品可见性是否被玩家关闭」。
    /// </summary>
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var modPlayer = player.GetModPlayer<LittlePrincePlayer>();
        modPlayer.EquippedRose = true;
        modPlayer.EquippedRoseVisible = !hideVisual;
        base.UpdateAccessory(player, hideVisual);
    }
}