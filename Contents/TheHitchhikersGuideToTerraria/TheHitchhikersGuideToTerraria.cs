using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using BookmarkItem = MatterRecord.Contents.Bookmark.Bookmark;   

namespace MatterRecord.Contents.TheHitchhikersGuideToTerraria;

/// <summary>
/// 《泰拉瑞亚漫游指南》物品本体。
/// <para>外观与基础属性复用原版法术书（<see cref="ItemID.SpellTome"/>），因此不再需要额外的贴图资源。</para>
/// <para>物品的发放由 <see cref="TheHitchhikersGuideToTerrariaPlayer"/> 负责：新角色创建时即有，老角色进入世界时补发一次。</para>
/// <para>物品用法：</para>
/// <para>1. 手持左键：领取一次性小礼物（内容见 <see cref="GiftItems"/>）。</para>
/// <para>2. 手持右键：切换「事象记录」的获取配置（<see cref="MatterRecordConfig.AllowingRecordRecipe"/>）。</para>
/// <para>3. 背包内右键：切换本模组指南的 tooltip 显示；显示时物品 tip 替换为指南文本，再次右键恢复。</para>
/// </summary>
public class TheHitchhikersGuideToTerraria : ModItem
{
    /// <summary>本地化键前缀（由物品类名自动映射到 hjson 中的同名片块）。</summary>
    private const string LocalizationKeyPrefix = "Mods.MatterRecord.Items.TheHitchhikersGuideToTerraria.";

    /// <summary>
    /// 复用原版法术书贴图：写法参考 <c>CosmosScissors</c>，指向 vanilla 的资源路径，
    /// 从而无需在 mod 里准备同名贴图。
    /// </summary>
    public override string Texture => $"Terraria/Images/Item_{ItemID.SpellTome}";

    /// <summary>
    /// 背包内右键可领取的礼物清单。
    /// <para>⚠️ 用户可在此处修改礼物内容：每一项为「物品类型, 数量」，可按需要增删条目。</para>
    /// </summary>
    public static readonly (int Type, int Stack)[] GiftItems =
    [
        (ModContent.ItemType<BookmarkItem>(), 1),   
        (ItemID.BugNet, 1),
        (ItemID.RecallPotion, 5),
        (ItemID.Book, 30),
        (ItemID.Torch, 30),
        (ItemID.LesserManaPotion, 10),
        (ItemID.LesserHealingPotion, 10),
    ];

    /// <summary>
    /// 按子键名读取本物品的本地化文案。
    /// <para>⚠️ 必须在运行时调用：本地化翻译要等 ReloadLanguage 之后才可用，
    /// 若在 Load 阶段取值会固化成键名原文。</para>
    /// </summary>
    /// <param name="key">子键名，例如 <c>ConfigOn</c>。</param>
    /// <returns>当前语言下的文案；键不存在时返回键名本身。</returns>
    internal static string GetText(string key) => Language.GetTextValue(LocalizationKeyPrefix + key);

    /// <summary>
    /// 以原版 <see cref="ItemID.SpellTome"/> 为模板克隆默认属性，再按本物品的用途覆盖差异字段。
    /// </summary>
    public override void SetDefaults()
    {
        // 直接继承原版法术书的：尺寸、使用动画/时长、HoldUp 使用样式、稀有度等
        Item.CloneDefaults(ItemID.SpellTome);

        // —— 以下为「漫游指南」特有的设定，必须显式覆盖 —— //

        // 每个角色终身只发放一份，禁止堆叠
        Item.maxStack = 1;

        Item.rare = ItemRarityID.Green;
        Item.value = Item.buyPrice(copper: 0);

        Item.consumable = false;

        // 显式固定使用样式为 HoldUp：左键与右键都靠它进入 UseItem 分支
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useTurn = true;
        Item.autoReuse = false;

        base.SetDefaults();
    }

    /// <summary>
    /// 允许右键使用，使手持右键时 <c>player.altFunctionUse</c> 变为 2，从而与左键（0）区分开。
    /// </summary>
    public override bool AltFunctionUse(Player player) => true;

    /// <summary>
    /// 左键与右键都允许使用。
    /// </summary>
    public override bool CanUseItem(Player player) => true;

    /// <summary>
    /// 手持使用时的行为分流：
    /// <para>右键：切换「事象记录」获取配置。</para>
    /// <para>左键：领取一次性小礼物。</para>
    /// <para>只在本地玩家上执行，避免多人环境下服务端与其它客户端重复触发。</para>
    /// </summary>
    public override bool? UseItem(Player player)
    {
        // 输入类逻辑一律加 owner 守卫：服务端（Main.myPlayer 为 255）与其它客户端都不处理
        if (player.whoAmI != Main.myPlayer)
            return true;

        if (player.altFunctionUse == 2)
        {
            // 右键：切换「事象记录」的获取配置（多人下由服务器权威切换）
            TheHitchhikersGuideSync.ToggleConfig();
            return true;
        }

        // 左键：领取一次性小礼物
        ClaimGift(player);
        return true;
    }

    /// <summary>
    /// 领取一次性小礼物（原背包内右键逻辑）。
    /// <para>每个角色只能领取一次，领取状态保存在 <see cref="TheHitchhikersGuideToTerrariaPlayer.giftClaimed"/> 中并随角色存档保存。</para>
    /// </summary>
    private void ClaimGift(Player player)
    {
        var modPlayer = player.GetModPlayer<TheHitchhikersGuideToTerrariaPlayer>();

        if (modPlayer.giftClaimed)
        {
            // 已领取：只提示，不再发放
            Main.NewText(GetText("GiftClaimed"), new Color(160, 160, 160));
            return;
        }

        foreach (var (type, stack) in GiftItems)
        {
            int index = player.QuickSpawnItem(player.GetSource_Misc("TheHitchhikersGuide"), type, stack);

            // 多人下客户端生成的掉落物必须显式同步，否则其它玩家看不到
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, index, 1f);
        }

        modPlayer.giftClaimed = true;
        SoundEngine.PlaySound(SoundID.Item4, player.Center);
        Main.NewText(GetText("GiftReceived"), new Color(255, 240, 120));
    }

    /// <summary>
    /// 背包内右键始终可用：切换指南 tooltip 显示状态。
    /// </summary>
    public override bool CanRightClick() => true;

    /// <summary>
    /// 背包内右键不消耗物品本身。
    /// </summary>
    public override bool ConsumeItem(Player player) => false;

    /// <summary>
    /// 背包内右键：切换本模组指南的 tooltip 显示。
    /// <para>显示时物品 tip 替换为指南文本，再次右键恢复。</para>
    /// </summary>
    public override void RightClick(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return;

        var modPlayer = player.GetModPlayer<TheHitchhikersGuideToTerrariaPlayer>();
        modPlayer.showGuideTooltip = !modPlayer.showGuideTooltip;

        SoundEngine.PlaySound(SoundID.MenuTick);

        base.RightClick(player);
    }

    /// <summary>
    /// 根据 <see cref="TheHitchhikersGuideToTerrariaPlayer.showGuideTooltip"/> 替换 tooltip。
    /// </summary>
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var modPlayer = Main.LocalPlayer.GetModPlayer<TheHitchhikersGuideToTerrariaPlayer>();
        if (!modPlayer.showGuideTooltip)
            return;

        // 清空原有 tooltip，替换为指南文本
        tooltips.Clear();

        // 标题
        tooltips.Add(new TooltipLine(Mod, "GuideTitle", GetText("GuideTitle"))
        {
            OverrideColor = Color.Gold
        });

        // 正文：按换行拆分成多行，保证长文本可读
        string body = GetText("ModGuide");
        string[] lines = body.Split('\n');
        foreach (string line in lines)
        {
            tooltips.Add(new TooltipLine(Mod, "GuideBody", line));
        }
    }
}