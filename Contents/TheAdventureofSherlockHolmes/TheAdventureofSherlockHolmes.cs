using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes
{
    public class TASHSystem : ModSystem
    {
        public override void Load()
        {
            Main.OnPostFullscreenMapDraw += Main_OnPostFullscreenMapDraw;
            base.Load();
        }

        public override void Unload()
        {
            Main.OnPostFullscreenMapDraw -= Main_OnPostFullscreenMapDraw;
            base.Unload();
        }

        public static int coolDown;
        public static Point? cachePoint;
        public static bool readyToShow;

        private static void LightOnMap(Point worldCoord)
        {
            for (int i = worldCoord.X - 3; i < worldCoord.X + 4; i++)
            {
                for (int j = worldCoord.Y - 3; j < worldCoord.Y + 4; j++)
                {
                    if (WorldGen.InWorld(i, j))
                    {
                        Main.Map.Update(i, j, byte.MaxValue);
                    }
                }
            }
            Main.refreshMap = true;
        }

        private void Main_OnPostFullscreenMapDraw(Vector2 arg1, float arg2)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && readyToShow && cachePoint.HasValue && Main.LocalPlayer.BuyItem(Item.buyPrice(0, 0, Main.LocalPlayer.discountEquipped ? 8 : 10, 0)))
            {
                LightOnMap(cachePoint.Value);
                cachePoint = null;
                readyToShow = false;
                return;
            }
            if (Main.gameMenu || !Main.mouseRight || Main.LocalPlayer.HeldItem.type != ModContent.ItemType<TheAdventureofSherlockHolmes>() || coolDown >= 0 || !Main.LocalPlayer.CanAfford(Item.buyPrice(0, 0, Main.LocalPlayer.discountEquipped ? 8 : 10, 0)))
            {
                coolDown--;
                return;
            }

            Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight) * Main.UIScale;
            Vector2 target = ((Main.MouseScreen - screenSize / 2f) / 16f * (16f / Main.mapFullscreenScale) + Main.mapFullscreenPos) * 16f;
            Point worldCoord = target.ToTileCoordinates();
            if (WorldGen.InWorld(worldCoord.X, worldCoord.Y))
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                {
                    if (Main.LocalPlayer.BuyItem(Item.buyPrice(0, 0, Main.LocalPlayer.discountEquipped ? 8 : 10, 0)))
                        LightOnMap(worldCoord);
                    coolDown = 30;
                }
                else
                {
                    cachePoint = worldCoord;
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)PacketType.TASHTileSync);
                    packet.Write(worldCoord.X);
                    packet.Write(worldCoord.Y);
                    packet.Send();
                    coolDown = 30;
                }
            }
        }
    }

    public class TheAdventureofSherlockHolmes : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = Item.height = 48;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;

            base.SetDefaults();
        }
    }
}