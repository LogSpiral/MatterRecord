using MatterRecord.Contents.Recorder;
using System;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes;

public class TheAdventureofSherlockHolmesSystem : ModSystem
{
    private static int TASHPrice => Main.LocalPlayer.discountEquipped ? 80 : 100;
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

    private static int _coolDown;
    private static Point? _cachedPoint;
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

                    Main.Map.Update(x, y, Math.Max(oldLight,newLight));
                }
            }
        }
        Main.refreshMap = true;
    }

    private void Main_OnPostFullscreenMapDraw(Vector2 arg1, float arg2)
    {
        if (!RecorderSystem.CheckUnlock(ItemRecords.TheAdventureofSherlockHolmes)) return;
        if (Main.netMode == NetmodeID.MultiplayerClient && readyToShow && _cachedPoint.HasValue && Main.LocalPlayer.BuyItem(TASHPrice))
        {
            LightOnMap(_cachedPoint.Value);
            _cachedPoint = null;
            readyToShow = false;
            return;
        }
        if (Main.gameMenu || !Main.mouseRight || Main.LocalPlayer.HeldItem.type != ModContent.ItemType<TheAdventureofSherlockHolmes>() || _coolDown >= 0 || !Main.LocalPlayer.CanAfford(TASHPrice))
        {
            _coolDown--;
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
                _coolDown = 30;
            }
            else
            {
                TheAdventureofSherlockHolmesTileSync.Get(Main.myPlayer, worldCoord).Send();
                _cachedPoint = worldCoord;
                _coolDown = 30;
            }
        }
    }
}
