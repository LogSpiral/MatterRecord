using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.TimeSword;

/// <summary>
/// 「游戏时剑」：以玩家的存档游玩时间为资源——存档时间直接显示在物品信息框中；
/// 左键攻击时消耗 1% 的存档时间（原为2%），伤害随「当前存档时间的平方根 / 4」成长（时间越久越强）。
/// </summary>
public class TimeSword : ModItem
{
    /// <summary>本次攻击的平方根伤害基数（基于扣减前的存档秒数，除以4后取整）。</summary>
    private int swingDamageBase;

    /// <inheritdoc />
    public override void SetDefaults()
    {
        Item.width = 36;
        Item.height = 38;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.autoReuse = true;

        Item.DamageType = DamageClass.Melee;
        Item.damage = 6;                     // 基础伤害
        Item.knockBack = 5f;
        Item.crit = 0;

        Item.value = 100;
        Item.rare = ItemRarityID.White;
        Item.UseSound = SoundID.Item1;
    }

    /// <inheritdoc />
    public override void HoldItem(Player player)
    {
        // 挥砍动画结束时重置伤害基数，避免残留到下一刀
        if (player.itemAnimation <= 0)
            swingDamageBase = 0;
    }

    /// <inheritdoc />
    public override bool? UseItem(Player player)
    {
        // 关键约定：扣时 / 伤害基数逻辑只在「玩家本地」运行。
        // 服务器端也会为远程玩家模拟执行 UseItem——若此时也读写 Main.ActivePlayerFileData，
        // 会拿到服务器自身（或 null）的档案，导致重复扣时 / 报错出岔子；
        // 而多人下本地玩家的近战伤害本就会同步给服务器结算，服务器无需再算一次攻击力，
        // 故服务器端对远程玩家的模拟只返回 true 维持挥砍表现。
        bool isLocalUser = Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer;
        if (!isLocalUser)
            return true;

        PlayerFileData playerFile = Main.ActivePlayerFileData;
        if (playerFile == null)
        {
            Main.NewText(this.GetLocalizedValue("CantLoadPlayerFile"), Color.Red);
            return true;
        }

        // ============ 左键攻击 ============
        // 先把秒表 Elapsed 并入 _playTime 并清零（保持计时继续运行）。
        // 原版 SetPlayTime 只改 _playTime 字段、不清零内部 Stopwatch；
        // 而 GetPlayTime 返回的是 _playTime + _timer.Elapsed。若不先并账清零，
        // 本会话持续累计的 Elapsed 会把刚扣掉的时间又加回来，表现为「砍几刀后时间不减反增」。
        playerFile.UpdatePlayTimerAndKeepState();

        TimeSpan currentTime = playerFile.GetPlayTime();
        int currentSeconds = (int)currentTime.TotalSeconds;

        // 【修改】伤害基数 = 平方根(当前秒数) / 4，向下取整
        swingDamageBase = (int)(Math.Sqrt(currentSeconds) / 40.0);

        // 时间消耗规则：只有当前时间 > 120 秒（2 分钟）时才减少时间，否则不消耗并给出提示
        if (currentSeconds > 120)
        {
            // 【修改】消耗量从 2% 下调为 1%
            int consume = (int)(currentSeconds * 0.01);
            int newSeconds = currentSeconds - consume;
            if (newSeconds < 0)
                newSeconds = 0;
            playerFile.SetPlayTime(TimeSpan.FromSeconds(newSeconds));
        }
        else
        {
            Main.NewText(this.GetLocalization("NotEnoughTime").Format(currentSeconds), 150, 150, 150);
        }

        return true;
    }

    /// <summary>
    /// 在物品信息框中显示当前角色的存档游玩时间（替代原右键查看功能）。
    /// </summary>
    /// <param name="tooltips">当前物品的 Tooltip 行集合，可向其中追加自定义行。</param>
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // tooltip 仅在客户端绘制；服务器端没有本地玩家档案，直接跳过
        if (Main.netMode == NetmodeID.Server)
            return;

        // 单人 / 客户端均取本地玩家档案；服务器端或菜单期可能为 null
        PlayerFileData playerFile = Main.ActivePlayerFileData;
        if (playerFile == null)
            return;

        TimeSpan totalPlayTime = playerFile.GetPlayTime();
        int totalSeconds = (int)totalPlayTime.TotalSeconds;
        string formatted = string.Format("{0:D2}:{1:D2}:{2:D2}",
            totalPlayTime.Hours, totalPlayTime.Minutes, totalPlayTime.Seconds);

        TooltipLine line = new TooltipLine(Mod, "CurrentPlayTime",
            this.GetLocalization("CurrentPlayTime").Format(totalSeconds, formatted))
        {
            OverrideColor = Color.LightGreen
        };
        tooltips.Add(line);
    }

    /// <inheritdoc />
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        // 把平方根伤害基数加到最终伤害上（基础伤害已由 Item.damage 提供）
        if (swingDamageBase > 0)
            damage += swingDamageBase;
    }

    /// <inheritdoc />
    public override void AddRecipes()
    {
        Recipe recipe = CreateRecipe();
        recipe.AddIngredient(ItemID.Wood, 10);
        recipe.AddTile(TileID.WorkBenches);
        recipe.Register();
    }
}