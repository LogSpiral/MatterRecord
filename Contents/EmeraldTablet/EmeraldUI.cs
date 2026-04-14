using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace MatterRecord.Contents.EmeraldTablet
{
    // 可拖拽面板（内部类）
    public class DraggablePanel : UIPanel
    {
        public UIElement ControlTarget { get; set; }
        public bool Dragging { get; private set; }
        private Vector2 Offset;

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            if (evt.Target == this)
            {
                Dragging = true;
                var target = ControlTarget ?? this;
                Offset = new Vector2(evt.MousePosition.X - target.Left.Pixels, evt.MousePosition.Y - target.Top.Pixels);
            }
            base.LeftMouseDown(evt);
        }

        public override void LeftMouseUp(UIMouseEvent evt)
        {
            Dragging = false;
            base.LeftMouseUp(evt);
        }

        public override void Update(GameTime gameTime)
        {
            if (Dragging)
            {
                var target = ControlTarget ?? this;
                target.Left.Set(Main.mouseX - Offset.X, 0f);
                target.Top.Set(Main.mouseY - Offset.Y, 0f);
                target.Recalculate();
            }
            base.Update(gameTime);
        }
    }

    // UI 状态类
    public class EmeraldUI : UIState
    {
        // 本地化文本静态字段（键前缀 EmeraldTablet）
        public static LocalizedText HintDefault { get; private set; }
        public static LocalizedText HintBanner { get; private set; }
        public static LocalizedText HintBossBag { get; private set; }
        public static LocalizedText HintCrate { get; private set; }
        public static LocalizedText HintInvalidBag { get; private set; }
        public static LocalizedText HintInvalid { get; private set; }
        public static LocalizedText BannerUnit { get; private set; }
        public static LocalizedText BagUnit { get; private set; }
        public static LocalizedText FormatBannersPerItem { get; private set; }
        public static LocalizedText FormatItemsPerBanner { get; private set; }

        private UIElement Mask;
        private DraggablePanel _mainPanel;
        private UIItemSlot _bannerSlot;
        private UIPanel _dropsPanel;
        private UIList _dropList;
        private UIScrollbar _scrollbar;
        private UIText _hintText;

        private int _currentNPC = -1;
        private string _currentNPCName = "";
        private int _currentKillsNeeded = 50;
        private bool _isBagMode = false;
        private Item _currentBagItem;
        private double _currentBagTotalRate;
        private bool _bossBagAlreadyExchanged = false;

        private static Dictionary<int, (int npcId, string npcName, int killsNeeded)> _bannerToNPC;
        private static readonly HashSet<int> _prohibitedPreHardmodeNPCs = new HashSet<int> { 85, 480, 170, 171, 180, 86, 82 };

        public static UserInterface UserInterface { get; private set; }
        private static EmeraldUI _instance;

        public static EmeraldUI Instance => _instance;
        public Item CurrentBannerItem => _bannerSlot?.Item;

        // 动画相关
        private static int _timer = 0;
        private static bool _active = false;
        public static bool Visible { get; private set; } = false;

        private const int TargetHeight = 250;
        private const int AnimationSteps = 15;

        // 构建旗帜字典
        public static void BuildBannerDictionary()
        {
            if (_bannerToNPC != null)
                return;

            _bannerToNPC = new Dictionary<int, (int, string, int)>();

            for (int npcId = -10; npcId < NPCLoader.NPCCount; npcId++)
            {
                if (npcId == 0) continue;

                int bannerId = Item.NPCtoBanner(npcId);
                if (bannerId > 0)
                {
                    int bannerItemId = Item.BannerToItem(bannerId);
                    string npcName = Lang.GetNPCNameValue(npcId);
                    int killsNeeded = ItemID.Sets.KillsToBanner[bannerItemId];
                    if (!_bannerToNPC.ContainsKey(bannerItemId))
                    {
                        _bannerToNPC.Add(bannerItemId, (npcId, npcName, killsNeeded));
                    }
                }
            }
        }

        private static void EnsureDictionaryBuilt()
        {
            if (_bannerToNPC == null)
                BuildBannerDictionary();
        }

        public static bool TryGetNPCInfo(int bannerItemId, out int npcId, out string npcName, out int kills)
        {
            EnsureDictionaryBuilt();
            if (_bannerToNPC.TryGetValue(bannerItemId, out var info))
            {
                npcId = info.npcId;
                npcName = info.npcName;
                kills = info.killsNeeded;
                return true;
            }
            npcId = -1;
            npcName = "";
            kills = 0;
            return false;
        }

        private static bool IsBagItem(Item item)
        {
            if (item == null || item.IsAir) return false;
            return ItemLoader.CanRightClick(item) && Main.ItemDropsDB.GetRulesForItemID(item.type).Count > 0;
        }

        private static bool IsAllowedBagItem(Item item)
        {
            if (!IsBagItem(item)) return false;
            if (ItemID.Sets.IsFishingCrate[item.type]) return true;
            if (ItemID.Sets.BossBag[item.type]) return true;
            return false;
        }

        private bool IsNPCForbidden()
        {
            return !Main.hardMode && _prohibitedPreHardmodeNPCs.Contains(_currentNPC);
        }

        // 加载本地化文本（由 ModSystem 调用），键前缀改为 EmeraldTablet
        public static void LoadLocalization(Mod mod)
        {
            HintDefault = mod.GetLocalization("EmeraldTablet.HintDefault");
            HintBanner = mod.GetLocalization("EmeraldTablet.HintBanner");
            HintBossBag = mod.GetLocalization("EmeraldTablet.HintBossBag");
            HintCrate = mod.GetLocalization("EmeraldTablet.HintCrate");
            HintInvalidBag = mod.GetLocalization("EmeraldTablet.HintInvalidBag");
            HintInvalid = mod.GetLocalization("EmeraldTablet.HintInvalid");
            BannerUnit = mod.GetLocalization("EmeraldTablet.BannerUnit");
            BagUnit = mod.GetLocalization("EmeraldTablet.BagUnit");
            FormatBannersPerItem = mod.GetLocalization("EmeraldTablet.FormatBannersPerItem");
            FormatItemsPerBanner = mod.GetLocalization("EmeraldTablet.FormatItemsPerBanner");
        }

        public static void Initialize()
        {
            if (UserInterface == null)
                UserInterface = new UserInterface();
            if (_instance == null)
            {
                _instance = new EmeraldUI();
                _instance.Activate();
                UserInterface.SetState(_instance);
            }
        }

        public static void Show()
        {
            Initialize();
            if (Visible) return;

            Visible = true;
            _active = true;
            _timer = 0;
            SoundEngine.PlaySound(SoundID.MenuOpen);

            var vec = Main.MouseScreen;
            vec -= Main.ScreenSize.ToVector2() * 0.5f;
            vec *= Main.GameViewMatrix.Zoom.X;
            vec += Main.ScreenSize.ToVector2() * 0.5f;
            vec /= Main.UIScale;
            _instance.Mask.Left.Set(vec.X, 0f);
            _instance.Mask.Top.Set(vec.Y, 0f);
            _instance.Mask.Recalculate();

            // 强制刷新当前物品槽的显示，确保本地化文本正确加载
            _instance.RefreshCurrentSlot();
        }

        public static void Hide()
        {
            if (!Visible) return;
            _active = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        public static void Toggle()
        {
            if (Visible) Hide();
            else Show();
        }

        public override void OnInitialize()
        {
            Mask = new UIElement
            {
                Width = new StyleDimension(400, 0f),
                Height = new StyleDimension(0, 0f),
                OverflowHidden = true
            };
            Append(Mask);

            _mainPanel = new DraggablePanel
            {
                ControlTarget = Mask,
                MinWidth = new StyleDimension(400, 0f),
                MinHeight = new StyleDimension(TargetHeight, 0f),
                BackgroundColor = new Color(30, 40, 70),
                PaddingTop = 12,
                PaddingBottom = 12,
                PaddingLeft = 12,
                PaddingRight = 12,
                Width = new StyleDimension(0, 1f),
                Height = new StyleDimension(0, 1f),
            };
            _mainPanel.OnUpdate += elem => { if (_mainPanel.IsMouseHovering) Main.LocalPlayer.mouseInterface = true; };
            Mask.Append(_mainPanel);

            _bannerSlot = new UIItemSlot(ItemSlot.Context.EquipAccessory, 0.8f);
            _bannerSlot.Width.Set(52, 0);
            _bannerSlot.Height.Set(52, 0);
            _bannerSlot.Left.Set(2, 0f);
            _bannerSlot.Top.Set(3, 0);
            _bannerSlot.OnItemChanged += OnBannerSlotChanged;
            _mainPanel.Append(_bannerSlot);

            _hintText = new UIText(HintDefault.Value, 0.7f);
            _hintText.TextColor = Color.LightGray;
            _hintText.Left.Set(70, 0f);
            _hintText.Top.Set(10, 0f);
            _mainPanel.Append(_hintText);

            _dropsPanel = new UIPanel();
            _dropsPanel.SetPadding(6);
            _dropsPanel.Width.Set(0, 0.95f);
            _dropsPanel.Height.Set(160, 0);
            _dropsPanel.Top.Set(60, 0);
            _dropsPanel.BackgroundColor = new Color(20, 30, 50);
            _mainPanel.Append(_dropsPanel);

            _dropList = new UIList();
            _dropList.Width.Set(0, 1);
            _dropList.Height.Set(0, 1);
            _dropList.ListPadding = 4;
            _dropsPanel.Append(_dropList);

            _scrollbar = new UIScrollbar();
            _scrollbar.SetView(100, 1000);
            _scrollbar.Height.Set(0, 1);
            _scrollbar.Left.Set(10, 1);
            _dropsPanel.Append(_scrollbar);
            _dropList.SetScrollbar(_scrollbar);

            // 刷新一次当前槽位，确保提示文本正确显示本地化内容
            RefreshCurrentSlot();
        }

        private void RefreshCurrentSlot()
        {
            OnBannerSlotChanged(CurrentBannerItem);
        }

        private void OnBannerSlotChanged(Item item)
        {
            EnsureDictionaryBuilt();

            if (item != null && !item.IsAir)
            {
                if (_bannerToNPC.TryGetValue(item.type, out var info))
                {
                    _isBagMode = false;
                    _currentNPC = info.npcId;
                    _currentNPCName = info.npcName;
                    _currentKillsNeeded = info.killsNeeded;
                    _currentBagItem = null;
                    _hintText.SetText(string.Format(HintBanner.Value, item.Name));
                    RefreshDropsForBanner();
                }
                else if (IsAllowedBagItem(item))
                {
                    _isBagMode = true;
                    _currentNPC = -1;
                    _currentNPCName = "";
                    _currentKillsNeeded = 0;
                    _currentBagItem = item.Clone();

                    bool isBossBag = ItemID.Sets.BossBag[item.type];
                    var recordPlayer = Main.LocalPlayer.GetModPlayer<BossBagRecordPlayer>();
                    _bossBagAlreadyExchanged = isBossBag && recordPlayer.HasExchanged(item.type);

                    var rules = Main.ItemDropsDB.GetRulesForItemID(item.type);
                    var dropRateInfos = new List<DropRateInfo>();
                    var chain = new DropRateInfoChainFeed(1f);
                    foreach (var rule in rules)
                        rule.ReportDroprates(dropRateInfos, chain);
                    _currentBagTotalRate = dropRateInfos.Sum(info => info.dropRate);

                    if (isBossBag)
                        _hintText.SetText(string.Format(HintBossBag.Value, item.Name));
                    else
                        _hintText.SetText(string.Format(HintCrate.Value, item.Name));

                    RefreshDropsForBag();
                }
                else if (IsBagItem(item))
                {
                    _currentNPC = -1;
                    _currentNPCName = "";
                    _dropList.Clear();
                    _hintText.SetText(HintInvalidBag.Value);
                }
                else
                {
                    _currentNPC = -1;
                    _currentNPCName = "";
                    _dropList.Clear();
                    _hintText.SetText(HintInvalid.Value);
                }
            }
            else
            {
                _currentNPC = -1;
                _currentNPCName = "";
                _dropList.Clear();
                _hintText.SetText(HintDefault.Value);
            }
        }

        private void RefreshDropsForBanner()
        {
            _dropList.Clear();
            if (_currentNPC == -1) return;

            var drops = LootDatabase.GetDropsForNPCRealTime(_currentNPC, Main.LocalPlayer);
            drops = drops.OrderBy(d => !(d.DropRate > 0 && d.IsAvailable))
                         .ThenByDescending(d => d.DropRate)
                         .ToList();

            bool forceUnavailable = IsNPCForbidden();
            foreach (var drop in drops)
            {
                var entry = new DropEntry(drop, false, _currentKillsNeeded, forceUnavailable, 0, PerformExchange);
                _dropList.Add(entry);
            }
        }

        private void RefreshDropsForBag()
        {
            _dropList.Clear();
            if (_currentBagItem == null || _currentBagItem.IsAir) return;

            var rules = Main.ItemDropsDB.GetRulesForItemID(_currentBagItem.type);
            var dropRateInfos = new List<DropRateInfo>();
            var chain = new DropRateInfoChainFeed(1f);
            foreach (var rule in rules)
                rule.ReportDroprates(dropRateInfos, chain);

            var resultMap = new Dictionary<int, (double rate, int min, int max, bool available)>();
            foreach (var info in dropRateInfos)
            {
                if (info.itemId <= 0) continue;
                bool canDrop = true;
                if (info.conditions != null && info.conditions.Count > 0)
                {
                    var dropAttemptInfo = new DropAttemptInfo
                    {
                        player = Main.LocalPlayer,
                        IsExpertMode = Main.expertMode,
                        IsMasterMode = Main.masterMode,
                        IsInSimulation = true
                    };
                    foreach (var condition in info.conditions)
                    {
                        if (!condition.CanDrop(dropAttemptInfo))
                        {
                            canDrop = false;
                            break;
                        }
                    }
                }
                double effectiveRate = canDrop ? info.dropRate : 0;
                if (resultMap.TryGetValue(info.itemId, out var existing))
                {
                    resultMap[info.itemId] = (
                        existing.rate + effectiveRate,
                        Math.Min(existing.min, info.stackMin),
                        Math.Max(existing.max, info.stackMax),
                        existing.available || canDrop
                    );
                }
                else
                {
                    resultMap[info.itemId] = (effectiveRate, info.stackMin, info.stackMax, canDrop);
                }
            }

            var drops = new List<DropInfo>();
            foreach (var kv in resultMap)
            {
                drops.Add(new DropInfo
                {
                    ItemID = kv.Key,
                    DropRate = kv.Value.rate,
                    StackMin = kv.Value.min,
                    StackMax = kv.Value.max,
                    IsAvailable = kv.Value.available
                });
            }

            drops = drops.OrderBy(d => !(d.DropRate > 0 && d.IsAvailable))
                         .ThenByDescending(d => d.DropRate)
                         .ToList();

            foreach (var drop in drops)
            {
                bool forceUnavailable = _bossBagAlreadyExchanged || !(drop.DropRate > 0 && drop.IsAvailable);
                var entry = new DropEntry(drop, true, 1, forceUnavailable, _currentBagTotalRate, PerformExchange);
                _dropList.Add(entry);
            }
        }

        public void PerformExchange(DropInfo dropInfo, int requiredBanners, int giveStack)
        {
            if (_isBagMode)
            {
                if (_bossBagAlreadyExchanged) return;
                Item bagItem = CurrentBannerItem;
                if (bagItem == null || bagItem.IsAir || !IsAllowedBagItem(bagItem)) return;
                if (bagItem.stack < requiredBanners) return;

                bool isBossBag = ItemID.Sets.BossBag[bagItem.type];
                if (isBossBag)
                {
                    var recordPlayer = Main.LocalPlayer.GetModPlayer<BossBagRecordPlayer>();
                    recordPlayer.RecordExchange(bagItem.type);
                    _bossBagAlreadyExchanged = true;
                }

                Player player = Main.LocalPlayer;
                int newItem = Item.NewItem(player.GetSource_GiftOrReward(), player.Center, dropInfo.ItemID, giveStack);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);

                bagItem.stack -= requiredBanners;
                bool wasCleared = bagItem.stack <= 0;
                if (wasCleared)
                    bagItem.TurnToAir();
                _bannerSlot.OnItemChanged?.Invoke(bagItem);
                if (wasCleared) return;
            }
            else
            {
                if (IsNPCForbidden()) return;
                Item bannerItem = CurrentBannerItem;
                if (bannerItem == null || bannerItem.IsAir) return;
                if (bannerItem.stack < requiredBanners) return;

                Player player = Main.LocalPlayer;
                int newItem = Item.NewItem(player.GetSource_GiftOrReward(), player.Center, dropInfo.ItemID, giveStack);
                if (Main.netMode == NetmodeID.MultiplayerClient)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newItem, 1f);

                bannerItem.stack -= requiredBanners;
                bool wasCleared = bannerItem.stack <= 0;
                if (wasCleared)
                    bannerItem.TurnToAir();
                _bannerSlot.OnItemChanged?.Invoke(bannerItem);
                if (wasCleared) return;
            }

            if (_isBagMode)
                RefreshDropsForBag();
            else
                RefreshDropsForBanner();
        }

        public override void Update(GameTime gameTime)
        {
            int oldTimer = _timer;
            _timer += _active ? 1 : -1;
            if (_timer > AnimationSteps) _timer = AnimationSteps;
            if (_timer < 0) _timer = 0;

            if (_timer == 0 && !_active && Visible)
                Visible = false;

            if (_timer != oldTimer)
            {
                float t = _timer / (float)AnimationSteps;
                float height = MathHelper.SmoothStep(0, TargetHeight, t);
                Mask.Height.Set(height, 0f);
                Mask.Recalculate();
            }

            base.Update(gameTime);
        }
    }

    // ModSystem 负责加载本地化和 UI
    public class EmeraldUILoader : ModSystem
    {
        private static GameTime _cachedGameTime;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                // 加载本地化文本（键前缀 EmeraldTablet）
                EmeraldUI.LoadLocalization(Mod);
                EmeraldUI.Initialize();
            }
        }

        public override void PostSetupContent()
        {
            if (!Main.dedServ)
                EmeraldUI.BuildBannerDictionary();
        }

        public override void Unload()
        {
            EmeraldUI.Hide();
            LootDatabase.Clear();
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _cachedGameTime = gameTime;
            EmeraldUI.UserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "MatterRecord: EmeraldTablet UI",
                    delegate
                    {
                        if (EmeraldUI.Visible && EmeraldUI.UserInterface?.CurrentState != null)
                            EmeraldUI.UserInterface.Draw(Main.spriteBatch, _cachedGameTime);
                        return true;
                    },
                    InterfaceScaleType.UI));
            }
        }
    }
}