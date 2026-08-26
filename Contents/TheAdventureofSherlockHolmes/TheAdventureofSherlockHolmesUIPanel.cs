using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes
{
    public class MapSearchPanel : UIState
    {
        private UIPanel _mainPanel;
        private UISearchBar _searchBar;
        private UIList _suggestionList;
        private UIPanel _suggestionPanel;
        private bool _wasMouseLeft;
        private string _lastQuery = "";

        private const int BASE_HEIGHT = 74;
        private const int MAX_EXTEND_HEIGHT = 300;

        public bool IsTyping => _searchBar?.IsWritingText ?? false;

        public override void OnInitialize()
        {
            _mainPanel = new UIPanel();
            _mainPanel.Left.Set(20f, 0f);
            _mainPanel.Top.Set(20f, 0f);
            _mainPanel.Width.Set(300f, 0f);
            _mainPanel.Height.Set(BASE_HEIGHT, 0f);
            _mainPanel.BackgroundColor = new Color(26, 32, 56) * 0.96f;
            _mainPanel.BorderColor = new Color(95, 115, 190);
            _mainPanel.SetPadding(0f);
            Append(_mainPanel);

            var innerPanel = new UIPanel();
            innerPanel.Left.Set(10f, 0f);
            innerPanel.Top.Set(17f, 0f);
            innerPanel.Width.Set(-20f, 1f);
            innerPanel.Height.Set(40f, 0f);
            innerPanel.BackgroundColor = new Color(18, 22, 38) * 0.98f;
            innerPanel.BorderColor = new Color(89, 116, 213);
            innerPanel.SetPadding(0f);
            innerPanel.IgnoresMouseInteraction = true;
            _mainPanel.Append(innerPanel);

            LocalizedText hint = new LocalizedText("Mods.MatterRecord.UI.SearchHint", "输入显示的图格名进行查找");
            _searchBar = new UISearchBar(hint, 1f);
            _searchBar.Left.Set(6f, 0f);
            _searchBar.Top.Set(2f, 0f);
            _searchBar.Width.Set(-12f, 1f);
            _searchBar.Height.Set(-4f, 1f);
            _searchBar.OnContentsChanged += OnSearchTextChanged;
            innerPanel.Append(_searchBar);

            _suggestionPanel = new UIPanel();
            _suggestionPanel.Left.Set(10f, 0f);
            _suggestionPanel.Top.Set(57f, 0f);
            _suggestionPanel.Width.Set(-20f, 1f);
            _suggestionPanel.MaxHeight.Set(MAX_EXTEND_HEIGHT - BASE_HEIGHT, 0f);
            _suggestionPanel.SetPadding(0f);
            _suggestionPanel.IgnoresMouseInteraction = false;
            _suggestionPanel.BackgroundColor = Color.Transparent;
            _suggestionPanel.BorderColor = Color.Transparent;
            _suggestionPanel.Height.Set(0f, 0f);
            _mainPanel.Append(_suggestionPanel);

            _suggestionList = new UIList();
            _suggestionList.Width.Set(0f, 1f);
            _suggestionList.Height.Set(0f, 1f);
            _suggestionList.ListPadding = 2f;
            _suggestionPanel.Append(_suggestionList);

            UIScrollbar scrollbar = new UIScrollbar();
            scrollbar.Height.Set(0f, 1f);
            scrollbar.Left.Set(-20f, 1f);
            scrollbar.SetView(100f, 1000f);
            _suggestionPanel.Append(scrollbar);
            _suggestionList.SetScrollbar(scrollbar);
        }

        private void OnSearchTextChanged(string text)
        {
            if (text == _lastQuery) return;
            _lastQuery = text;

            SearchCore.UserInputText = text;

            if (string.IsNullOrWhiteSpace(text))
            {
                SearchCore.ClearSearch();
                Main.LocalPlayer?.GetModPlayer<LocatorPlayer>().ClearSavedState();
            }
            else
            {
                SearchCore.QueueSearch(text);
            }

            UpdateSuggestionList(text);
        }

        private void UpdateSuggestionList(string query)
        {
            _suggestionList.Clear();
            if (string.IsNullOrWhiteSpace(query))
            {
                _suggestionPanel.BackgroundColor = Color.Transparent;
                _suggestionPanel.BorderColor = Color.Transparent;
                _suggestionPanel.Height.Set(0f, 0f);
                _mainPanel.Height.Set(BASE_HEIGHT, 0f);
                _suggestionPanel.Recalculate();
                _mainPanel.Recalculate();
                return;
            }

            var matches = SearchCore.GetMatchingTileNames(query, 20);
            if (matches.Count == 0)
            {
                _suggestionPanel.BackgroundColor = Color.Transparent;
                _suggestionPanel.BorderColor = Color.Transparent;
                _suggestionPanel.Height.Set(0f, 0f);
                _mainPanel.Height.Set(BASE_HEIGHT, 0f);
                _suggestionPanel.Recalculate();
                _mainPanel.Recalculate();
                return;
            }

            _suggestionPanel.BackgroundColor = new Color(26, 32, 56) * 0.96f;
            _suggestionPanel.BorderColor = new Color(95, 115, 190);

            foreach (string name in matches)
            {
                var entry = new SuggestionEntry(name, OnSuggestionClicked, CancelCurrentSearch);
                _suggestionList.Add(entry);
            }

            float suggestionHeight = matches.Count * 30 + 6;
            suggestionHeight = MathHelper.Clamp(suggestionHeight, 0, MAX_EXTEND_HEIGHT - BASE_HEIGHT);
            _suggestionPanel.Height.Set(suggestionHeight, 0f);
            _suggestionPanel.Recalculate();

            _mainPanel.Height.Set(BASE_HEIGHT + suggestionHeight, 0f);
            _mainPanel.Recalculate();
        }

        // ===== 修改点：点击候选时先失焦 =====
        private void OnSuggestionClicked(string tileName)
        {
            // 失焦（如果输入框处于输入状态）
            if (_searchBar != null && _searchBar.IsWritingText)
                _searchBar.ToggleTakingText();

            // 如果点击的是当前搜索项，不执行任何搜索
            if (string.Equals(tileName, SearchCore.CurrentQuery, System.StringComparison.OrdinalIgnoreCase))
                return;

            SearchCore.ForceSearchByExactName(tileName);
            UpdateSuggestionList(SearchCore.UserInputText);
        }

        private void CancelCurrentSearch()
        {
            SearchCore.ClearSearch();
            Main.LocalPlayer?.GetModPlayer<LocatorPlayer>().ClearSavedState();
            _searchBar.SetContents(string.Empty, false);
            if (_searchBar.IsWritingText)
                _searchBar.ToggleTakingText();
            UpdateSuggestionList(string.Empty);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (_searchBar != null && _searchBar.ContainsPoint(Main.MouseScreen))
            {
                if (!_searchBar.IsWritingText && Main.mouseLeft && !_wasMouseLeft)
                {
                    _searchBar.ToggleTakingText();
                }
            }
            _wasMouseLeft = Main.mouseLeft;

            if (_mainPanel.ContainsPoint(Main.MouseScreen) || IsTyping)
            {
                Main.LocalPlayer.mouseInterface = true;
                Main.blockMouse = true;
            }
        }

        public void ResetText()
        {
            if (_searchBar != null)
            {
                _searchBar.SetContents(string.Empty, false);
                if (IsTyping) _searchBar.ToggleTakingText();
            }
            _suggestionList.Clear();
            _suggestionPanel.BackgroundColor = Color.Transparent;
            _suggestionPanel.BorderColor = Color.Transparent;
            _suggestionPanel.Height.Set(0f, 0f);
            _mainPanel.Height.Set(BASE_HEIGHT, 0f);
            _suggestionPanel.Recalculate();
            _mainPanel.Recalculate();
        }

        public void RestoreFromCore()
        {
            string userInput = SearchCore.UserInputText;
            _searchBar.SetContents(userInput, false);
            UpdateSuggestionList(userInput);
        }
    }

    // ========== SuggestionEntry 类（包含您指定的叉号样式） ==========
    public class SuggestionEntry : UIPanel
    {
        private UIText _text;
        private UIText _cancelText;
        private string _tileName;
        private System.Action<string> _onClick;
        private System.Action _onCancel;

        public SuggestionEntry(string tileName, System.Action<string> onClick, System.Action onCancel)
        {
            _tileName = tileName;
            _onClick = onClick;
            _onCancel = onCancel;

            Width.Set(0f, 1f);
            Height.Set(30f, 0f);
            BackgroundColor = new Color(40, 50, 70) * 0.9f;
            BorderColor = Color.Transparent;
            SetPadding(0f);

            _text = new UIText(tileName, 0.8f);
            _text.Left.Set(8f, 0f);
            _text.Top.Set(5f, 0f);
            Append(_text);

            // 叉号文本（您指定的位置和大小）
            _cancelText = new UIText("×", 2.5f);
            _cancelText.Left.Set(-37f, 1f);
            _cancelText.Top.Set(-6f, 0f);
            _cancelText.TextColor = Color.Transparent;
            _cancelText.SetPadding(0f);
            _cancelText.IgnoresMouseInteraction = true;
            _cancelText.OnLeftMouseDown += (evt, element) =>
            {
                _onCancel?.Invoke();
                SoundEngine.PlaySound(SoundID.MenuTick);
            };
            Append(_cancelText);
        }

        public override void LeftMouseDown(UIMouseEvent evt)
        {
            // 如果点击在叉号区域，交由叉号处理，不触发条目点击
            if (_cancelText != null && _cancelText.ContainsPoint(evt.MousePosition))
                return;

            base.LeftMouseDown(evt);
            _onClick?.Invoke(_tileName);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            bool isActive = string.Equals(_tileName, SearchCore.CurrentQuery, System.StringComparison.OrdinalIgnoreCase);
            if (isActive)
            {
                _cancelText.TextColor = _cancelText.IsMouseHovering ? Color.OrangeRed : Color.Red;
                _cancelText.IgnoresMouseInteraction = false;
            }
            else
            {
                _cancelText.TextColor = Color.Transparent;
                _cancelText.IgnoresMouseInteraction = true;
            }

            // 背景悬停（避开叉号）
            if (IsMouseHovering && !_cancelText.IsMouseHovering)
                BackgroundColor = new Color(70, 80, 100) * 0.9f;
            else
                BackgroundColor = new Color(40, 50, 70) * 0.9f;
        }
    }
}