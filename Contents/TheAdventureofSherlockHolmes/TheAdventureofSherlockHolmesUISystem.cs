using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using MatterRecord.Contents.Recorder;

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
                _searchPanel.RestoreFromCore();
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
            if (!Main.mapFullscreen || Main.gameMenu)
                return false;

            if (!RecorderSystem.CheckUnlock(ItemRecords.TheAdventureofSherlockHolmes))
                return false;

            int itemType = ModContent.ItemType<TheAdventureofSherlockHolmes>();
            return Main.LocalPlayer.inventory.Union(Main.LocalPlayer.bank.item)
                .Union(Main.LocalPlayer.bank2.item)
                .Union(Main.LocalPlayer.bank3.item)
                .Union(Main.LocalPlayer.bank4.item)
                .Any(item => item.type == itemType);
        }
    }
}