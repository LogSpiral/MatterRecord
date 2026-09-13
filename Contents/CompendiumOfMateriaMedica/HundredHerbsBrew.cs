namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

/// <summary>
/// 百草酿：由一瓶水 + 每种草药各一个酿成。
/// 饮用后获得 5 分钟的「百草生香」增益（FragrantHerbs）。
/// 其配方中包含全部七种草药，所以装备本草纲目（或同队有人装备）时，
/// CompendiumGlobalItem 会自动识别并激活所有草药效果，本物品无需额外处理。
/// </summary>
public class HundredHerbsBrew : ModItem
{
    public override void SetStaticDefaults()
    {
        // 英文显示名、Tooltip 等由本地化文件提供
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.useAnimation = 17;
        Item.useTime = 17;
        Item.useTurn = true;
        Item.consumable = true;
        Item.maxStack = 9999;
        Item.value = Item.buyPrice(silver: 10);
        Item.rare = ItemRarityID.LightRed;
        Item.UseSound = SoundID.Item3;

        // 关键：设置增益类型与时长（5 分钟 = 300 秒 = 18000 帧）
        Item.buffType = ModContent.BuffType<FragrantHerbs>();
        Item.buffTime = 300 * 60;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.BottledWater)
            .AddIngredient(ItemID.Blinkroot)
            .AddIngredient(ItemID.Daybloom)
            .AddIngredient(ItemID.Deathweed)
            .AddIngredient(ItemID.Fireblossom)
            .AddIngredient(ItemID.Moonglow)
            .AddIngredient(ItemID.Shiverthorn)
            .AddIngredient(ItemID.Waterleaf)
            .AddTile(TileID.Bottles)          // 炼药台/瓶子处制作；如需炼药台可换成 TileID.AlchemyTable
            .Register();
    }
}