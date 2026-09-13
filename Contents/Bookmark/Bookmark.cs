using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.Bookmark;

/// <summary>
/// 书签：装备后为玩家开启一个「事象记录专用饰品栏」，并提供少量移速与全职业伤害加成。
/// </summary>
public class Bookmark : ModItem
{
    /// <summary>
    /// 设置物品基础属性。
    /// <see cref="Item.width"/> 与 <see cref="Item.height"/> 取自 <c>Contents\Bookmark\Bookmark.png</c> 的真实像素尺寸。
    /// </summary>
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(silver: 65);
        Item.rare = ItemRarityID.Green;
    }

    /// <summary>
    /// 装备时施加属性加成。
    /// 这里选择直接修改玩家属性而非挂载 buff，是为了避免为效果额外绘制一张 buff 图标。
    /// </summary>
    /// <param name="player">装备该饰品的玩家。</param>
    /// <param name="hideVisual">是否隐藏视觉表现；本饰品无视觉特效，该参数不使用。</param>
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        // +3% 移速：moveSpeed 决定加速度上限、maxRunSpeed 决定最高速度，两者同调手感才不会割裂
        player.moveSpeed += 0.03f;
        player.maxRunSpeed += 0.03f;

        // +5% 全职业伤害：沿用项目统一写法 GetDamage(DamageClass.Generic)，不采用已过时的 player.allDamage
        player.GetDamage(DamageClass.Generic) += 0.05f;
    }

    /// <summary>
    /// 注册书签配方：以书与丝线在织布机处合成。
    /// 采用 1.4 新式 <c>CreateRecipe()</c> 链式写法；旧版 ModRecipe 已在项目内全面迁移，继续使用会编译失败。
    /// </summary>
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Book, 1)
            .AddIngredient(ItemID.Silk, 5)
            .AddTile(TileID.Loom)
            .Register();
    }
}

/// <summary>
/// 永恒书签：消耗品。
/// <para>使用后<b>永久</b>解锁「事象记录」专用饰品栏，此后不必再占用一个功能性饰品槽去装备 <see cref="Bookmark"/>。</para>
/// <para>与 <see cref="Bookmark"/> 的关系：书签是「装备即生效、取下即失效」的临时钥匙；
/// 永恒书签是把它固化到角色存档上的一次性投资。两者共用同一套栏位准入规则。</para>
/// </summary>
public class EternalBookmark : ModItem
{
    /// <summary>
    /// 复用书签贴图，不单独绘制新素材。
    /// 覆盖 Texture 即可，tModLoader 会按此路径加载 <c>Contents\Bookmark\Bookmark.png</c>；
    /// 因此目录下<b>不要</b>再放 EternalBookmark.png，否则会与这里指向的资源产生混淆。
    /// </summary>
    public override string Texture => "MatterRecord/Contents/Bookmark/Bookmark";

    /// <summary>
    /// 设置物品基础属性。
    /// 尺寸与书签保持一致，沿用原版消耗品的通用使用动画（HoldUp 起手 + 30 帧）。
    /// </summary>
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.UseSound = SoundID.Item4;
        Item.consumable = true;
        Item.value = Item.buyPrice(gold: 2);
        Item.rare = ItemRarityID.LightRed;
    }

    /// <summary>
    /// 呼吸微光叠层颜色，与未解锁记录书本同款。
    /// <para>在 <see cref="Color.Transparent"/>（RGB=0、A=0）与 <c>Color.White with { A = 0 }</c>（RGB=255、A=0）
    /// 之间插值，得到一份「alpha 恒为 0、RGB 在 0~255 之间来回」的颜色。</para>
    /// <para>Terraria 默认走预乘 alpha 混合：alpha=0 意味着这一层不遮挡画面，
    /// 但 RGB 分量会被直接加到目标像素上，于是画出来的是加法光而不是半透明叠色。
    /// <c>cos(GlobalTimeWrappedHourly * 6)</c> 把这条曲线压到 0~0.5 区间、周期约一秒，
    /// 就有了肉眼可见的呼吸式闪烁。</para>
    /// </summary>
    private static Color BreathingGlow => Color.Lerp(
        Color.Transparent,
        Color.White with { A = 0 },
        MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f);

    /// <summary>
    /// 物品栏内的呼吸微光。
    /// 放在 PostDraw 而非 PreDraw：原版已经按常规流程把 Bookmark 贴图完整画好了，
    /// 这里只需要「再补一层亮光」，不需要自己重绘、也不需要拦截原版绘制。
    /// </summary>
    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        spriteBatch.Draw(
            TextureAssets.Item[Type].Value,
            position,
            frame,
            BreathingGlow,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            0f);
    }

    /// <summary>
    /// 世界中掉落物的呼吸微光。
    /// 原点、旋转、缩放都沿用物品当前的绘制参数，光效才会严丝合缝地贴在贴图上，不会因为物品旋转/滚动而错位。
    /// </summary>
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        var texture = TextureAssets.Item[Type].Value;
        spriteBatch.Draw(
            texture,
            Item.Center - Main.screenPosition,
            null,
            BreathingGlow,
            rotation,
            texture.Size() * .5f,
            scale,
            SpriteEffects.None,
            0f);
    }

    /// <summary>
    /// 已解锁时禁止再次使用。
    /// 若不拦截，玩家会白白损失一个永恒书签；此处返回 false 让物品在原地「抖动」提示不可用。
    /// </summary>
    /// <param name="player">尝试使用的玩家。</param>
    /// <returns>尚未解锁返回 true。</returns>
    public override bool CanUseItem(Player player)
    {
        return !player.GetModPlayer<BookmarkPlayer>().EternalBookmarkUnlocked;
    }

    /// <summary>
    /// 使用后把永久解锁标记写到玩家身上。
    /// 该标记由 <see cref="BookmarkPlayer"/> 负责存档与读取，栏位启用判定则见 <see cref="BookmarkSlot.IsEnabled"/>。
    /// </summary>
    /// <param name="player">使用该物品的玩家。</param>
    /// <returns>返回 true 以正常消耗物品并播放使用音效。</returns>
    public override bool? UseItem(Player player)
    {
        player.GetModPlayer<BookmarkPlayer>().EternalBookmarkUnlocked = true;
        return true;
    }

    /// <summary>
    /// 注册永恒书签配方：书签 ×1 + 神圣锭 ×2，在秘银砧处合成。
    /// <c>TileID.MythrilAnvil</c> 在原版语义中同时覆盖秘银砧与山铜砧，故无需再额外登记山铜砧。
    /// </summary>
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<Bookmark>(1)
            .AddIngredient(ItemID.HallowedBar, 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

/// <summary>
/// 书签系统的玩家数据载体。
/// 目前只保存一项状态：玩家是否已使用过 <see cref="EternalBookmark"/>。
/// </summary>
public class BookmarkPlayer : ModPlayer
{
    /// <summary>
    /// 是否已使用过 <see cref="EternalBookmark"/>，即「事象记录」专用饰品栏是否已永久解锁。
    /// 未解锁时，栏位仍可由装备 <see cref="Bookmark"/> 临时开启。
    /// </summary>
    public bool EternalBookmarkUnlocked { get; set; }

    /// <summary>
    /// 把解锁状态写入角色存档。
    /// 只在已解锁时写入 key，未解锁的玩家存档不会多出冗余字段。
    /// </summary>
    /// <param name="tag">存档数据容器。</param>
    public override void SaveData(TagCompound tag)
    {
        if (EternalBookmarkUnlocked)
            tag["eternalBookmark"] = true;
    }

    /// <summary>
    /// 从角色存档读取解锁状态。
    /// 旧存档没有该字段，<c>ContainsKey</c> 会安全返回 false，等价于「尚未解锁」。
    /// </summary>
    /// <param name="tag">存档数据容器。</param>
    public override void LoadData(TagCompound tag)
    {
        EternalBookmarkUnlocked = tag.ContainsKey("eternalBookmark");
    }
}

/// <summary>
/// 「事象记录」专用饰品栏。
/// <para>启用条件：玩家已使用过 <see cref="EternalBookmark"/> 永久解锁，
/// <b>或</b>在功能性饰品槽中装备了 <see cref="Bookmark"/>。</para>
/// <para>准入限制：只接受同时满足「在记录系统内」（实现 <see cref="IRecordBookItem"/>）与
/// 「是饰品」（<see cref="Item.accessory"/>）两个条件的物品。</para>
/// </summary>
public class BookmarkSlot : ModAccessorySlot
{
    /// <summary>栏位的悬浮提示文本。</summary>
    public static LocalizedText SlotNameText { get; private set; }

    /// <summary>
    /// 在内容加载阶段绑定本地化文本。
    /// 放在此阶段而非 Load 早期，是为了避免翻译值尚未填充时就把 key 原文固化进字段。
    /// </summary>
    public override void SetupContent()
    {
        SlotNameText = Mod.GetLocalization($"{nameof(BookmarkSlot)}.SlotName");
    }

    /// <summary>
    /// 判断玩家是否已在功能性饰品槽中装备书签。
    /// </summary>
    /// <param name="player">待检查的玩家。</param>
    /// <returns>已装备书签返回 true。</returns>
    private static bool HasBookmarkEquipped(Player player)
    {
        // 功能性饰品槽是 3 .. 3+maxAccessoryIndex；13 起是幻化槽，故此处不从 13 开始遍历
        int maxAccessoryIndex = 5 + player.extraAccessorySlots;
        for (int i = 3; i < 3 + maxAccessoryIndex; i++)
        {
            if (player.armor[i].type == ModContent.ItemType<Bookmark>())
                return true;
        }

        return false;
    }

    /// <summary>
    /// 栏位是否启用：永久解锁（永恒书签）或临时装备书签，满足其一即可。
    /// 永久解锁是「或」而非「替换」关系——玩家解锁后仍可把书签当普通饰品佩戴吃加成。
    /// </summary>
    public override bool IsEnabled() =>
        Player.GetModPlayer<BookmarkPlayer>().EternalBookmarkUnlocked || HasBookmarkEquipped(Player);

    /// <summary>
    /// 未启用时的显示策略：栏位内仍留有物品时继续显示。
    /// 若返回 false，玩家取下书签后原本存放的事象记录饰品会看不见也取不回。
    /// </summary>
    public override bool IsVisibleWhenNotEnabled() => !IsEmpty;

    /// <summary>
    /// 准入限制：功能槽与幻化槽只接受「事象记录类饰品」，染料槽只接受染料。
    /// </summary>
    /// <param name="checkItem">待放入的物品。</param>
    /// <param name="context">目标子槽类型。</param>
    /// <returns>允许放入返回 true。</returns>
    public override bool CanAcceptItem(Item checkItem, AccessorySlotType context)
    {
        // 染料槽沿用原版语义：只接受染料，避免饰品被塞进染色位
        if (context == AccessorySlotType.DyeSlot)
            return checkItem.dye > 0;

        // 两个条件缺一不可：只判 IRecordBookItem 会让蝇王、堂吉诃德等记录武器混入；
        // 只判 accessory 会让陆龟护身符等普通饰品混入。
        // 未解锁的记录饰品无需在此拦截，项目已有 RecordBookItem.CanEquipAccessory 统一把关。
        return checkItem.accessory && checkItem.ModItem is IRecordBookItem;
    }

    /// <summary>
    /// 右键快速装备优先级：栏位启用时，让事象记录类饰品优先落入本栏位。
    /// </summary>
    /// <param name="item">被快速装备的物品。</param>
    /// <param name="accSlotToSwapTo">原本将要放入的饰品槽下标。</param>
    /// <returns>应优先放入本栏位返回 true。</returns>
    public override bool ModifyDefaultSwapSlot(Item item, int accSlotToSwapTo)
    {
        return IsEnabled() && item.accessory && item.ModItem is IRecordBookItem;
    }

    /// <summary>栏位图标：复用记录系统的书本贴图（本模组资源）。</summary>
    public override string FunctionalTexture => "MatterRecord/Contents/Recorder/RecordBook";

    /// <summary>
    /// 鼠标悬浮在栏位上时显示自定义文本。
    /// </summary>
    /// <param name="context">当前悬浮的子槽类型。</param>
    public override void OnMouseHover(AccessorySlotType context)
    {
        Main.hoverItemName = SlotNameText?.Value ?? string.Empty;
    }
}