using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「天剑」：万剑归宗在背包内右键切过来的形态——把自己也变成剑。
/// <para>手持时长按左键，玩家化身为背包（含副背包）里第一把剑的贴图，朝鼠标方向持续飞行：
/// 不松手就一直飞，速度由鼠标离屏幕中心的距离决定；松手后按当前速度滑行一段再收回来。
/// 化身期间玩家隐身、免疫击退并大幅减伤，撞到的敌人按背包里最高那把剑的伤害结算——天剑自己也算一把。</para>
/// <para>实体逻辑在 <see cref="TianJianProjectile"/>，隐身与减伤在 <see cref="TianJianPlayer"/>。
/// 物品与弹幕的贴图都直接引用原版天顶剑，不额外打包 png；想换图就删掉 Texture 属性再把同名 png 放回目录。</para>
/// </summary>
public class TianJian : ModItem
{
    /// <summary>与天顶剑贴图同尺寸，不额外打包 png。</summary>
    public override string Texture => "Terraria/Images/Item_" + ItemID.Zenith;

    /// <inheritdoc />
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;

        // 化身伤害的保底值：背包里一把剑都没有、或剑都比它低时用这个（见 Shoot）
        Item.damage = 190;
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
        Item.UseSound = null;            // 出鞘声由弹幕第一帧单独播，避免长按时循环播放使用音

        Item.shoot = ModContent.ProjectileType<TianJianProjectile>();
        Item.shootSpeed = 0.01f;         // 速度由弹幕自己接管，这里只是让原版愿意走生成流程

        Item.value = Item.sellPrice(0, 20, 0, 0);
        Item.rare = ItemRarityID.Red;
        Item.ResearchUnlockCount = 1;
    }

    public override bool AllowPrefix(int pre) => false;

    /// <summary>已经有一把化身在飞的时候不许再生成（和巨型陆龟壳同一套判定）。</summary>
    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;

    /// <inheritdoc />
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        // 只在本人端生成，其余端靠弹幕同步（与万剑归宗的写法一致）
        if (player.whoAmI != Main.myPlayer)
            return false;

        // 贴图用背包里第一把剑，伤害用背包里最高的那把；天剑自己也算一把，背包里没有更高的就用它的基础伤害
        int swordType = WanJianGuiZongPlayer.FindFirstSwordType(player);
        int swordDamage = Math.Max(WanJianGuiZongPlayer.FindStrongestSwordDamage(player), Item.damage);

        int finalDamage = (int)player.GetTotalDamage(DamageClass.Melee).ApplyTo(swordDamage);
        if (finalDamage < 1)
            finalDamage = 1;

        Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, finalDamage, knockback, player.whoAmI, ai0: swordType);
        return false;
    }

    /// <summary>背包内右键切回万剑归宗。</summary>
    public override bool CanRightClick() => true;

    /// <summary>切换状态不消耗物品本身（参考浮士德 / 蝇王的写法）。</summary>
    public override bool ConsumeItem(Player player) => false;

    /// <inheritdoc />
    public override void RightClick(Player player)
    {
        // ChangeItemType 会保留收藏标记，比直接 SetDefaults 温和一点
        Item.ChangeItemType(ModContent.ItemType<WanJianGuiZong>());
        SoundEngine.PlaySound(SoundID.Item4, player.Center);
        base.RightClick(player);
    }

    /// <inheritdoc />
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (Main.dedServ)
            return;

        tooltips.Add(new TooltipLine(Mod, "SwitchBack", this.GetLocalization("SwitchBack").Value)
        {
            OverrideColor = Color.LightGreen
        });
    }
    public override void AddRecipes()
    {
        // 与天顶剑同一个工作台：山铜/秘银砧
        CreateRecipe()
            .AddIngredient(ItemID.Zenith)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}
