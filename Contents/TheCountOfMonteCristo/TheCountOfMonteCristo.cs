using MatterRecord.Contents.Rarities;
using MatterRecord.Contents.Recorder;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheCountOfMonteCristo
{
    public class TheCountOfMonteCristo : ModItem, IRecordBookItem
    {
        ItemRecords IRecordBookItem.RecordType => ItemRecords.TheCountOfMonteCristo;

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ModContent.RarityType<ImmutableQuest>();
            Item.value = Item.buyPrice(copper: 5);
        }

        /// <summary>
        /// 注册《基督山伯爵》的「事象记录」配方：书本 + 蜡烛 @ 书架。
        /// 蜡烛取原版普通蜡烛（<see cref="ItemID.Candle"/> = 105），
        /// 与其余事象记录物品保持同一范式（含配置开关条件、禁用拆解）。
        /// </summary>
        public override void AddRecipes()
        {
            // 复用 Utils.cs 中既有的记录物品配方扩展方法，该方法已内置
            // AddIngredient(ItemID.Book) / AddTile(TileID.Bookcases) /
            // AllowingRecordRecipe 条件 / DisableDecraft，无需在此重复书写。
            this.RegisterBookRecipe(ItemID.Candle);
        }
    }
}
