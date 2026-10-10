using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace MatterRecord.Contents.TodaySword;

/// <summary>
/// 「今日推剑」广告弹窗 UI 管理系统（仅客户端存在界面的端运行）。
/// </summary>
public class AdUISystem : ModSystem
{
    private UserInterface _adInterface;
    private AdUIState _adUI;

    /// <inheritdoc />
    public override void Load()
    {
        if (Main.dedServ)
            return;

        _adInterface = new UserInterface();
        _adUI = new AdUIState();
        _adUI.Activate();
    }

    /// <inheritdoc />
    public override void Unload()
    {
        if (Main.dedServ)
            return;

        _adUI?.Deactivate();
        _adUI = null;
        _adInterface = null;
    }

    /// <inheritdoc />
    public override void UpdateUI(GameTime gameTime)
    {
        if (Main.dedServ)
            return;

        _adInterface?.Update(gameTime);
    }

    /// <inheritdoc />
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Main.dedServ)
            return;

        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "MatterRecord: TodaySword Ad UI",
                delegate
                {
                    _adInterface.Draw(Main.spriteBatch, new GameTime());
                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    /// <summary>弹出广告弹窗。</summary>
    public void ShowAdUI()
    {
        if (Main.dedServ)
            return;
        _adUI.RefreshAdTitle();
        _adInterface?.SetState(_adUI);
    }

    /// <summary>关闭广告弹窗。</summary>
    public void HideAdUI()
    {
        if (Main.dedServ)
            return;
        _adInterface?.SetState(null);
    }
}

/// <summary>
/// 「今日推剑」广告弹窗界面（自动关闭，无关闭按钮）。
/// </summary>
public class AdUIState : UIState
{
    private int _timer = 0;
    private const int DISPLAY_TIME = 180; // 3秒
    private UIPanel _mainPanel;
    private UIText _adText;

    /// <inheritdoc />
    public override void OnActivate()
    {
        base.OnActivate();
        _timer = DISPLAY_TIME;
        RefreshAdTitle();
    }

    /// <summary>刷新广告标题文本（弹窗显示前调用，确保取得已加载的本地化翻译值）。</summary>
    public void RefreshAdTitle()
    {
        _adText?.SetText(Language.GetTextValue("Mods.MatterRecord.Items.TodaySword.AdUITitle"));
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (_timer > 0)
        {
            _timer--;
            if (_timer <= 0)
            {
                ModContent.GetInstance<AdUISystem>().HideAdUI();
            }
        }
    }

    /// <inheritdoc />
    public override void OnInitialize()
    {
        _mainPanel = new UIPanel();
        _mainPanel.Width.Set(336f, 0f);
        _mainPanel.Height.Set(160f, 0f);
        _mainPanel.HAlign = 0.5f;
        _mainPanel.VAlign = 0.5f;
        _mainPanel.BackgroundColor = new Color(30, 30, 100) * 0.8f;
        _mainPanel.BorderColor = Color.Gold;

        _adText = new UIText("");
        _adText.HAlign = 0.5f;
        _adText.VAlign = 0.5f;
        _adText.TextColor = Color.White;
        _mainPanel.Append(_adText);

        Append(_mainPanel);
    }
}

/// <summary>
/// 「今日推剑」每日选剑系统（纯本地，不保存数据，无网络同步）。
/// 每个端本地跟踪昼夜，进入新的一天（凌晨 4:30 从夜晚翻转为白天的首次检测）时，
/// 从近战剑池中随机一把作为「今日剑」并把本地免费次数补满。
/// <para>
/// 跨天检测挂在 <see cref="PostUpdateTime"/> 而不是 <see cref="PostUpdateWorld"/>：
/// 原版只在 <c>Main.netMode != NetmodeID.MultiplayerClient</c> 时才调用 <c>WorldGen.UpdateWorld()</c>，
/// 而 <c>PostUpdateWorld</c> 是由它内部触发的，所以多人客户端上这个钩子根本不会执行，
/// 跨天检测与补满次数会一起失效。PostUpdateTime 是无条件调用的，客户端同样会跑。
/// </para>
/// <para>
/// 不做服务器广播，各端用各自的 Main.rand 独立抽取，因此多人下每个人的「今日剑」可以不同；
/// 体验结算也只在本地端进行（<see cref="TodaySword.UseItem"/> 里有本地玩家判定），互不冲突。
/// </para>
/// </summary>
public class DailySwordSystem : ModSystem
{
    /// <summary>本端当前的今日剑物品类型（ItemID.None 表示尚未选定）。</summary>
    public static int CurrentSwordType = ItemID.None;

    /// <summary>可被选中的近战剑池（Swing 类近战武器）。</summary>
    public static List<int> SwordPool = new List<int>();

    /// <summary>今日是否已刷新过（本地跨天标记）。</summary>
    private static bool _hasRefreshedToday = false;

    /// <summary>上一帧是否处于白天（用于跨天检测）。</summary>
    private static bool _wasDayTime = false;

    /// <summary>
    /// 本地剩余免费体验次数（纯本地数据，每端各自持有，不参与网络同步）。
    /// 本地跨天刷新出新的今日剑时补满。
    /// </summary>
    public static int FreeCharges = 0;

    /// <summary>每日免费次数的上限。</summary>
    public const int MaxFreeCharges = 3;

    /// <inheritdoc />
    public override void PostSetupContent()
    {
        SwordPool.Clear();
        for (int i = 0; i < ItemLoader.ItemCount; i++)
        {
            Item item = ContentSamples.ItemsByType[i];
            if (IsSword(item))
            {
                SwordPool.Add(item.type);
            }
        }
    }

    /// <summary>
    /// 判断一件物品是否算作「剑」（今日推剑的选剑判定，万剑归宗的飞剑判定也复用这一套）：
    /// 有伤害、近战伤害类型、Swing 挥砍使用方式、本体造成近战伤害，且不是镐 / 斧 / 锤。
    /// </summary>
    public static bool IsSword(Item item)
    {
        return item.damage > 0 &&
               item.DamageType == DamageClass.Melee &&
               item.useStyle == ItemUseStyleID.Swing &&
               item.noMelee == false &&
               item.pick == 0 &&
               item.axe == 0 &&
               item.hammer == 0;
    }

    /// <inheritdoc />
    public override void PostUpdateTime()
    {
        // PostUpdateTime 在主菜单也会被调用（原版对它的调用早于 gameMenu 的提前返回），
        // 而主菜单里的时间同样在流逝；不挡掉的话，在标题界面跨天会白占掉一次当日刷新，
        // 进世界后当天就再也选不出剑了。
        if (Main.gameMenu)
            return;

        bool isDayTime = Main.dayTime;

        if (isDayTime && !_wasDayTime)
            _hasRefreshedToday = false;

        if (isDayTime && !_hasRefreshedToday)
        {
            _hasRefreshedToday = true;
            RandomizeSword();
        }

        _wasDayTime = isDayTime;
    }

    /// <summary>
    /// 从剑池中随机选一把作为今日剑，并把本地免费次数补满（纯本地，不广播）。
    /// </summary>
    public static void RandomizeSword()
    {
        if (SwordPool.Count == 0)
            return;

        int newSword = SwordPool[Main.rand.Next(SwordPool.Count)];
        CurrentSwordType = newSword;
        FreeCharges = MaxFreeCharges;
    }
}

/// <summary>
/// 「今日推剑」物品：左键挥动今日剑本体 / 消耗本地免费次数切换为今日剑进行体验；
/// 右键打开广告面板获取 1 次免费次数（本地数据）。所有体验结算只在玩家本地运行。
/// </summary>
public class TodaySword : ModItem
{
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
        Item.damage = 6;
        Item.knockBack = 5f;
        Item.crit = 0;

        Item.value = 100;
        Item.rare = ItemRarityID.White;
        Item.UseSound = SoundID.Item1;
    }

    /// <inheritdoc />
    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    /// <inheritdoc />
    public override void HoldItem(Player player)
    {
        if (!player.GetModPlayer<TodaySwordPlayer>().HasBuff)
            Item.noUseGraphic = false;
    }

    /// <inheritdoc />
    public override bool? UseItem(Player player)
    {
        bool isLocalUser = Main.netMode == NetmodeID.SinglePlayer || player.whoAmI == Main.myPlayer;
        if (!isLocalUser)
            return true;

        // ============ 右键：广告点击（看广告得 1 次免费体验，本地数据，不同步） ============
        if (player.altFunctionUse == 2)
        {
            var modPlayer = player.GetModPlayer<TodaySwordPlayer>();
            if (modPlayer.LocalAdCooldown == 0)
            {
                modPlayer.LocalAdCooldown = TodaySwordPlayer.AdCooldownFrames;
                if (DailySwordSystem.FreeCharges < DailySwordSystem.MaxFreeCharges)
                    DailySwordSystem.FreeCharges++;
            }

            ModContent.GetInstance<AdUISystem>().ShowAdUI();
            return false;
        }

        // ============ 左键：今日剑体验 ============
        if (player.selectedItem < 0 || player.selectedItem > 9)
            return false;

        var modPlayer2 = player.GetModPlayer<TodaySwordPlayer>();

        int swordType = DailySwordSystem.CurrentSwordType;
        if (swordType <= 0 || swordType == ItemID.None)
        {
            DailySwordSystem.RandomizeSword();
            swordType = DailySwordSystem.CurrentSwordType;
        }

        if (modPlayer2.HasBuff)
        {
            modPlayer2.OnAttackWhileBuffed();
            return null;
        }

        // 尝试消耗本地免费次数
        if (DailySwordSystem.FreeCharges > 0)
        {
            DailySwordSystem.FreeCharges--;
        }
        else
        {
            // 没有免费次数，挥今日推剑本体
            return true;
        }

        // 激活体验
        Item.noUseGraphic = true;
        Item targetSword = new Item();
        targetSword.SetDefaults(swordType);
        // 伤害平衡：取今日剑原始伤害与背包最高伤害的较小值
        int originalDamage = targetSword.damage;
        int maxDamage = 0;
        for (int i = 0; i < player.inventory.Length; i++)
        {
            if (i == player.selectedItem) continue;
            Item invItem = player.inventory[i];
            if (invItem != null && !invItem.IsAir && invItem.damage > maxDamage)
            {
                maxDamage = invItem.damage;
            }
        }
        if (maxDamage > 0)
        {
            targetSword.damage = System.Math.Min(originalDamage, maxDamage);
        }

        modPlayer2.ActivateBuff(player.selectedItem, player.inventory[player.selectedItem], targetSword.Clone());

        return null;
    }

    /// <inheritdoc />
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        int swordType = DailySwordSystem.CurrentSwordType;
        if (swordType > 0 && swordType != ItemID.None)
        {
            Item template = new Item();
            template.SetDefaults(swordType);
            TooltipLine line = new TooltipLine(Mod, "CurrentSword", this.GetLocalization("CurrentSwordText").Format(template.Name));
            line.OverrideColor = Color.LightGreen;
            tooltips.Add(line);
        }
        else
        {
            TooltipLine line = new TooltipLine(Mod, "CurrentSword", this.GetLocalizedValue("SwordNotSelectedText"));
            line.OverrideColor = Color.Gray;
            tooltips.Add(line);
        }

        string chargesText = this.GetLocalization("FreeChargesText").Format(DailySwordSystem.FreeCharges, DailySwordSystem.MaxFreeCharges);

        if (Main.netMode != NetmodeID.Server)
        {
            var localPlayer = Main.LocalPlayer.GetModPlayer<TodaySwordPlayer>();
            if (localPlayer.LocalAdCooldown > 0)
                chargesText += this.GetLocalization("AdCooldownText").Format(localPlayer.LocalAdCooldown / 60);
        }

        tooltips.Add(new TooltipLine(Mod, "FreeCharges", chargesText));
        tooltips.Add(new TooltipLine(Mod, "RightClick", this.GetLocalizedValue("RightClickHint")));
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

/// <summary>
/// 「今日推剑」玩家扩展：体验剑临时替换（非持久 Buff）与本地广告冷却。
/// </summary>
public class TodaySwordPlayer : ModPlayer
{
    private int _buffSlot = -1;
    private Item _originalItem = null;
    private Item _targetSword = null;
    private int _buffTimer = 0;
    private const int BUFF_DURATION = 60;
    private const int IDLE_FRAMES = 1;
    private int _idleTimer = 0;

    public int LocalAdCooldown = 0;
    public const int AdCooldownFrames = 1800;

    public bool HasBuff => _buffTimer > 0 && _buffSlot != -1 && _originalItem != null && _targetSword != null;

    public void OnAttackWhileBuffed()
    {
        if (!HasBuff)
            return;
        _buffTimer = BUFF_DURATION;
        _idleTimer = IDLE_FRAMES;
    }

    public void ActivateBuff(int slot, Item originalItem, Item targetSword)
    {
        DeactivateBuff();

        _buffSlot = slot;
        _originalItem = originalItem;
        _targetSword = targetSword;
        _buffTimer = BUFF_DURATION;
        _idleTimer = IDLE_FRAMES;

        Player.inventory[slot] = targetSword;
    }

    private void DeactivateBuff()
    {
        if (_buffSlot != -1 && _originalItem != null)
        {
            Player.inventory[_buffSlot] = _originalItem;
        }

        _buffSlot = -1;
        _originalItem = null;
        _targetSword = null;
        _buffTimer = 0;
        _idleTimer = 0;
    }

    /// <inheritdoc />
    public override void PostUpdate()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            if (LocalAdCooldown > 0)
                LocalAdCooldown--;
        }

        if (!HasBuff)
            return;

        if (Player.itemAnimation > 0)
        {
            _idleTimer = IDLE_FRAMES;
        }
        else
        {
            _idleTimer--;
            if (_idleTimer <= 0)
            {
                DeactivateBuff();
                return;
            }
        }

        _buffTimer--;
        if (_buffTimer <= 0)
        {
            DeactivateBuff();
            return;
        }

        if (Player.selectedItem != _buffSlot)
        {
            DeactivateBuff();
        }
    }

    /// <inheritdoc />
    public override void OnRespawn()
    {
        DeactivateBuff();
    }
}