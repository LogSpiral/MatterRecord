using Terraria.Audio;

namespace MatterRecord.Contents.ImperfectPage
{
    public class ImperfectPageGlobalItem : GlobalItem
    {
        public override bool CanRightClick(Item item)
        {
            if (!ImperfectPage.Active) return false;
            if (item.ammo != AmmoID.Bullet) return false;
            if (item.type == ModContent.ItemType<ImperfectPage>()) return false;
            return true;
        }

        public override void RightClick(Item item, Player player)
        {
            if (!ImperfectPage.Active) return;
            if (item.ammo != AmmoID.Bullet) return;
            if (item.type == ModContent.ItemType<ImperfectPage>()) return;

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                var system = ModContent.GetInstance<ImperfectPageSystem>();
                system.FavoriteAmmoType = item.type;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ImperfectPageSync.Get(item.type).Send();
                SoundEngine.PlaySound(SoundID.MenuTick);
            }

            ImperfectPage.Active = false;
        }
    }
}