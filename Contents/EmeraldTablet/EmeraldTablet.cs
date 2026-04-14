using MatterRecord.Contents.Recorder;

namespace MatterRecord.Contents.EmeraldTablet;

public class EmeraldTablet : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.EmeraldTablet;

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 24;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useAnimation = 15;
        Item.useTime = 15;
        Item.noUseGraphic = true;
        Item.noMelee = true;
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.AlchemyTable);
    }

    // 允许右键
    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        // 右键：打开/关闭 UI
        if (player.altFunctionUse == 2)
        {
            player.itemTime = player.itemAnimation = 0;
            if (player.whoAmI == Main.myPlayer)
                EmeraldUI.Toggle();
            return false;
        }
        // 左键：什么都不做，且不产生挥动动画
        player.itemTime = player.itemAnimation = 0;
        return false;
    }
}