using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes;

public class TASHSystem : ModSystem
{
    public static int TASHPrice => Main.LocalPlayer.discountEquipped ? 80 : 100;
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
        for (int i = -18; i < 19; i++)
        {
            for (int j = -18; j < 19; j++)
            {
                int x = worldCoord.X + i;
                int y = worldCoord.Y + j;
                if (WorldGen.InWorld(x, y))
                {
                    var oldLight = Main.Map[x, y].Light;
                    var newLight = (byte)(255
                        * Utils.GetLerpValue(18, 13, MathF.Abs(i), true)
                        * Utils.GetLerpValue(18, 13, MathF.Abs(j), true));
                    if (newLight > oldLight)
                        Main.Map.Update(x, y, newLight);
                }
            }
        }
        Main.refreshMap = true;
    }

    private void Main_OnPostFullscreenMapDraw(Vector2 arg1, float arg2)
    {
        if (!RecorderSystem.CheckUnlock(ItemRecords.TheAdventureofSherlockHolmes)) return;
        if (Main.netMode == NetmodeID.MultiplayerClient && readyToShow && cachePoint.HasValue && Main.LocalPlayer.BuyItem(TASHPrice))
        {
            LightOnMap(cachePoint.Value);
            cachePoint = null;
            readyToShow = false;
            return;
        }
        if (Main.gameMenu || !Main.mouseRight || Main.LocalPlayer.HeldItem.type != ModContent.ItemType<TheAdventureofSherlockHolmes>() || coolDown >= 0 || !Main.LocalPlayer.CanAfford(TASHPrice))
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
                if (Main.LocalPlayer.BuyItem(TASHPrice))
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

public class TheAdventureofSherlockHolmes : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheAdventureofSherlockHolmes;
    public override void SetDefaults()
    {
        Item.width = Item.height = 48;
        Item.value = Item.sellPrice(0, 1, 0, 0);
        Item.rare = ItemRarityID.Green;

        base.SetDefaults();
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.ShinePotion);
    }
}