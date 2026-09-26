using System.Collections.Generic;
using MatterRecord.Contents.TodaySword;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「万剑归宗」：长按左键把背包与猪猪存钱罐等副背包里的剑逐一唤出，围绕玩家盘旋（此刻不造成伤害）；
/// 松开左键时，所有飞剑一起沿鼠标方向刺出去并追踪敌人。
/// 飞剑只是投影，不会消耗剑本身；飞剑的贴图与伤害取自对应的剑（判定方式见 <see cref="DailySwordSystem.IsSword"/>）。
/// </summary>
public class WanJianGuiZong : ModItem
{
    /// <summary>
    /// 本体直接用天顶剑的物品贴图（不额外打包 png，直接引用原版资源）。
    /// 想换成自己画的图：删掉这个属性，再把 WanJianGuiZong.png 放回同目录即可。
    /// </summary>
    public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;

    /// <inheritdoc />
    public override void SetDefaults()
    {
        // 与天顶剑贴图同尺寸
        Item.width = 24;
        Item.height = 24;

        // 本体不造成伤害，飞剑的伤害由各把剑自身决定
        Item.damage = 1;
        Item.DamageType = DamageClass.Melee;
        Item.knockBack = 7f;
        Item.crit = 0;

        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.channel = true;
        Item.autoReuse = true;
        Item.UseSound = null;            // 声音由每把剑出鞘时单独播放，避免长按时循环播放使用音

        Item.value = Item.sellPrice(0, 20, 0, 0);
        Item.rare = ItemRarityID.Red;
        Item.ResearchUnlockCount = 1;
    }

    /// <inheritdoc />
    public override bool? UseItem(Player player)
    {
        // 唤剑逻辑只在本地玩家所在端跑。
        // 注意这里不能用 Main.netMode == NetmodeID.Server 判断：主机开房间时主机进程的 netMode 就是 2，
        // 那样房主自己反而用不了这把武器。专用服务器上 Main.myPlayer 是 255，不会命中任何玩家。
        if (player.whoAmI != Main.myPlayer)
            return true;

        // 只有确实处于长按状态才开一轮；animation 残留导致的重复调用会因为已进入 channeling 而空转
        if (!player.channel)
            return true;

        player.GetModPlayer<WanJianGuiZongPlayer>().StartChannel();

        return true;
    }
    public override bool AllowPrefix(int pre) => false;
    /// <inheritdoc />
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (Main.dedServ)
            return;

        int count = WanJianGuiZongPlayer.CountAvailableSwords(Main.LocalPlayer);
        if (count > WanJianGuiZongPlayer.MaxSummonCount)
            count = WanJianGuiZongPlayer.MaxSummonCount;

        TooltipLine line = new TooltipLine(Mod, "AvailableSwords",
            this.GetLocalization("AvailableSwords").Format(count))
        {
            OverrideColor = Color.LightGreen
        };
        tooltips.Add(line);
    }

    /// <inheritdoc />
    public override void AddRecipes()
    {
        // 与天顶剑同一个工作台：山铜/秘银砧
        CreateRecipe()
            .AddIngredient(ItemID.Zenith)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
