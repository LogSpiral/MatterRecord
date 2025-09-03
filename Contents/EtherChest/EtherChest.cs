using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Generation;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.EtherChest
{
    public class EtherChest : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<EtherChest_Tile>());
            // Item.placeStyle = 1; // Use this to place the chest in its locked style
            Item.width = 26;
            Item.height = 22;
            Item.value = 500;
            Item.rare = ItemRarityID.Green;
        }
    }

    public class EtherChest_Tile : ModTile
    {
        public override void SetStaticDefaults()
        {
            // Properties
            Main.tileSpelunker[Type] = true;
            Main.tileContainer[Type] = true;
            Main.tileShine2[Type] = true;
            Main.tileShine[Type] = 1200;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileOreFinderPriority[Type] = 500;
            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.BasicChest[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.AvoidedByNPCs[Type] = true;
            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileID.Sets.IsAContainer[Type] = true;
            TileID.Sets.FriendlyFairyCanLureTo[Type] = true;
            TileID.Sets.GeneralPlacementTiles[Type] = false;

            DustType = DustID.ShimmerSpark;
            AdjTiles = [TileID.Containers];

            // Other tiles with just one map entry use CreateMapEntryName() to use the default translationkey, "MapEntry"
            // Since ExampleChest needs multiple, we register our own MapEntry keys
            AddMapEntry(new Color(200, 200, 200), this.GetLocalization("MapEntry"), MapChestName);

            // Style 1 is ExampleChest when locked. We want that tile style to drop the ExampleChest item as well. Use the Chest Lock item to lock this chest.
            // No item places ExampleChest in the locked style, so the automatically determined item drop is unknown, this is why RegisterItemDrop is necessary in this situation.
            //RegisterItemDrop(ModContent.ItemType<EtherChest>(), 1);
            // Sometimes mods remove content, such as tile styles, or tiles accidentally get corrupted. We can, if desired, register a fallback item for any tile style that doesn't have an automatically determined item drop. This is done by omitting the tileStyles parameter.
            //RegisterItemDrop(ItemID.Chest);

            // Placement
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(Chest.FindEmptyChest, -1, 0, true);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(Chest.AfterPlacement_Hook, -1, 0, false);
            TileObjectData.newTile.AnchorInvalidTiles = [
                TileID.MagicalIceBlock,
                TileID.Boulder,
                TileID.BouncyBoulder,
                TileID.LifeCrystalBoulder,
                TileID.RollingCactus
            ];
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);
        }

        public override ushort GetMapOption(int i, int j)
        {
            return (ushort)(Framing.GetTileSafely(i, j).TileFrameX / 36);
        }

        public override LocalizedText DefaultContainerName(int frameX, int frameY)
        {
            return this.GetLocalization("MapEntry");
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
        {
            return true;
        }

        public static string MapChestName(string name, int i, int j)
        {
            int left = i;
            int top = j;
            Tile tile = Framing.GetTileSafely(i, j);
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }

            if (tile.TileFrameY != 0)
            {
                top--;
            }

            int chest = Chest.FindChest(left, top);
            if (chest < 0)
            {
                return Language.GetTextValue("LegacyChestType.0");
            }

            if (Main.chest[chest].name == "")
            {
                return name;
            }

            return name + ": " + Main.chest[chest].name;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = 1;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // We override KillMultiTile to handle additional logic other than the item drop. In this case, unregistering the Chest from the world
            Chest.DestroyChest(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Framing.GetTileSafely(i, j);
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }

            if (tile.TileFrameY != 0)
            {
                top--;
            }

            player.CloseSign();
            player.SetTalkNPC(-1);
            Main.npcChatCornerItem = 0;
            Main.npcChatText = "";
            if (Main.editChest)
            {
                SoundEngine.PlaySound(SoundID.MenuTick);
                Main.editChest = false;
                Main.npcChatText = string.Empty;
            }

            if (player.editedChestName)
            {
                NetMessage.SendData(MessageID.SyncPlayerChest, -1, -1, NetworkText.FromLiteral(Main.chest[player.chest].name), player.chest, 1f);
                player.editedChestName = false;
            }

            bool isLocked = Chest.IsLocked(left, top);
            if (Main.netMode == NetmodeID.MultiplayerClient && !isLocked)
            {
                if (left == player.chestX && top == player.chestY && player.chest != -1)
                {
                    player.chest = -1;
                    Recipe.FindRecipes();
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else
                {
                    NetMessage.SendData(MessageID.RequestChestOpen, -1, -1, null, left, top);
                    Main.stackSplit = 600;
                }
            }
            else
            {
                int chest = Chest.FindChest(left, top);
                if (chest != -1)
                {
                    Main.stackSplit = 600;
                    if (chest == player.chest)
                    {
                        player.chest = -1;
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                    else
                    {
                        SoundEngine.PlaySound(player.chest < 0 ? SoundID.MenuOpen : SoundID.MenuTick);
                        player.OpenChest(left, top, chest);
                    }

                    Recipe.FindRecipes();
                }
            }

            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            Tile tile = Framing.GetTileSafely(i, j);
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }

            if (tile.TileFrameY != 0)
            {
                top--;
            }

            int chest = Chest.FindChest(left, top);
            player.cursorItemIconID = -1;
            if (chest < 0)
            {
                player.cursorItemIconText = Language.GetTextValue("LegacyChestType.0");
            }
            else
            {
                string defaultName = TileLoader.DefaultContainerName(tile.TileType, tile.TileFrameX, tile.TileFrameY); // This gets the ContainerName text for the currently selected language
                player.cursorItemIconText = Main.chest[chest].name.Length > 0 ? Main.chest[chest].name : defaultName;
                if (player.cursorItemIconText == defaultName)
                {
                    player.cursorItemIconID = ModContent.ItemType<EtherChest>();
                    player.cursorItemIconText = "";
                }
            }

            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
        }

        public override void MouseOverFar(int i, int j)
        {
            MouseOver(i, j);
            Player player = Main.LocalPlayer;
            if (player.cursorItemIconText == "")
            {
                player.cursorItemIconEnabled = false;
                player.cursorItemIconID = 0;
            }
        }
    }

    public class EtherChestPass : ILoadable
    {
        private void Detour_Shimmer(WorldGen.orig_GenPassDetour orig, object self, GenerationProgress progress, GameConfiguration configuration)
        {
            orig(self, progress, configuration);
            List<Rectangle> structArea = typeof(StructureMap).GetField("_structures", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenVars.structures) as List<Rectangle>;
            List<Rectangle> protectArea = typeof(StructureMap).GetField("_protectedStructures", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GenVars.structures) as List<Rectangle>;
            Rectangle s = structArea.Last();
            Rectangle p = protectArea.Last();
            structArea.Remove(s);
            protectArea.Remove(p);
            var point = GenVars.shimmerPosition.ToPoint();
            for (int i = -1; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    WorldGen.KillTile(point.X + i, point.Y + 30 - j, false, false, true);

            for (int i = 0; i < 2; i++)
                WorldGen.PlaceTile(point.X + i * 3 - 1, point.Y + 28, TileID.Torches, false, true, -1, 23);

            int id = WorldGen.PlaceChest(point.X, point.Y + 30, (ushort)ModContent.TileType<EtherChest_Tile>());
            var chest = Main.chest[id];
            chest.item[0] = new Item(ModContent.ItemType<EmeraldTablet.EmeraldTablet>());

            structArea.Add(s);
            protectArea.Add(p);
        }

        public void Load(Mod mod)
        {
            WorldGen.DetourPass((PassLegacy)WorldGen.VanillaGenPasses["Shimmer"], Detour_Shimmer);
        }

        public void Unload()
        { }
    }
}