namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithBoulderEx : ModItem
{
    public override bool IsLoadingEnabled(Mod mod) => true;
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<ZenithBoulderExTile>());

        Item.width = Item.height = 32;
        Item.value = Item.sellPrice(0, 0, 5);
        Item.rare = ItemRarityID.Purple;
        Item.createTile = ModContent.TileType<ZenithBoulderExTile>();

        base.SetDefaults();
    }

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 1;

        base.SetStaticDefaults();
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.LunarBar)             // 向神的贡品(x
            .AddIngredient(ItemID.Boulder)              // 神, 万物之源
            .AddIngredient(ItemID.BouncyBoulder)        // 弹力巨石, 提供超强弹性
            .AddIngredient(ItemID.RollingCactus)        // 仙人掌球, 提供溅射能力！现在撞墙会生成迷你巨石
            .AddIngredient(ItemID.BoulderStatue)        // 巨石雕像, 提供召唤巨石的能力
            // .AddIngredient(ItemID.RainbowBoulder)    // 彩虹巨石, 提供酷炫拖尾！
            // .AddIngredient(ItemID.LavaBoulder)       // 熔岩巨石, 提供亡语岩浆与滞留岩浆(好过分
            // .AddIngredient(ItemID.LavaBoulder)       // 生命水晶巨石, 不好把特色纳入，还是算了
            // .AddIngredient(ItemID.SpiderBoulder)     // 蜘蛛巨石, 提供蛛网滞留与亡语蜘蛛
            // .AddIngredient(ItemID.Poulder)           // 臭臭巨石, 好恶心要不还是不要加进来了太恶趣味了
            // .AddIngredient(ItemID.Ghoulder)          // 鬼魂巨石，子巨石将具有鬼魂巨石的特性！撞墙反弹的时候将会有20%生成鬼魂巨石
            .AddTile(TileID.HeavyWorkBench)
            .DisableDecraft();
            // .Register(); // 目前先不提供合成途径
        base.AddRecipes();
    }
}