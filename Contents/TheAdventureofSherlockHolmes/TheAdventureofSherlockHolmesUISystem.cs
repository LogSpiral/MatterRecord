using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes
{
    public class TheAdventureofSherlockHolmesUISystem : ModSystem
    {
        private UserInterface _searchInterface;
        internal MapSearchPanel _searchPanel;

        public override void Load()
        {
            if (Main.dedServ) return;
            SearchCore.Load();
            _searchInterface = new UserInterface();
            _searchPanel = new MapSearchPanel();
            _searchPanel.Activate();
        }

        public override void Unload()
        {
            SearchCore.Unload();
            _searchPanel = null;
            _searchInterface = null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            SearchCore.Update();

            if (Main.dedServ || !ShouldShowSearchUI())
            {
                if (_searchInterface?.CurrentState != null)
                    _searchInterface.SetState(null);
                return;
            }

            if (_searchInterface.CurrentState != _searchPanel)
            {
                _searchPanel.ResetText();
                _searchInterface.SetState(_searchPanel);
            }

            if (_searchPanel.IsTyping)
                Main.LocalPlayer.mouseInterface = true;

            _searchInterface.Update(gameTime);
        }

        public override void PostDrawFullscreenMap(ref string mouseText)
        {
            if (Main.dedServ) return;
            SearchCore.DrawResults(Main.spriteBatch);
            if (ShouldShowSearchUI() && _searchInterface?.CurrentState != null)
                _searchInterface.Draw(Main.spriteBatch, new GameTime());
        }

        private bool ShouldShowSearchUI()
        {
            return Main.mapFullscreen && !Main.gameMenu &&
                   Main.LocalPlayer.HeldItem.type == ModContent.ItemType<TheAdventureofSherlockHolmes>();
        }
    }
}