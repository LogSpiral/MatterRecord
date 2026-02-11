using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TheoryOfFreedom;

[AutoloadEquip(EquipType.Wings)]
public class TheoryOfFreedom : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheoryOfFreedom;
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var mplr = player.GetModPlayer<TheTheoryOfFreedomPlayer>();
        mplr.EquippedTOF = true;
        if (player.grapCount > 0)
            player.endurance += 0.1f;
        player.noFallDmg = true;
        if (mplr.CanHookPlatform) return;

        Tile thisTile = Framing.GetTileSafely(player.Bottom);
        Tile bottomTile = Framing.GetTileSafely(player.Bottom + Vector2.UnitY * 8f);
        if (!Collision.SolidCollision(player.BottomLeft, player.width, 16))
        {
            if (player.velocity.Y >= 0f && (IsPlatform(thisTile.TileType) || IsPlatform(bottomTile.TileType)))
            {
                player.position.Y += 2f;
            }
            if (player.velocity.Y == 0f)
            {
                player.position.Y += 16f;
            }
        }
        static bool IsPlatform(int tileType)
        {
            if (tileType != 19)
            {
                return tileType == 380;
            }
            return true;
        }
        base.UpdateAccessory(player, hideVisual);
    }
    public override void SetStaticDefaults()
    {
        ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(-300);
    }
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.ChlorophyteBar);
    }
    public override void SetDefaults()
    {
        Item.width = Item.height = 32;
        Item.accessory = true;
        Item.value = Item.sellPrice(0, 4, 0, 0);
        Item.rare = ItemRarityID.Yellow;
        base.SetDefaults();
    }

    public override void Load()
    {
        IL_WorldGen.CheckJunglePlant += SpawnRecordFreedom;
        base.Load();
    }

    private static void SpawnRecordFreedom(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchCall(typeof(NPC).GetMethod(nameof(NPC.SpawnOnPlayer), BindingFlags.Static | BindingFlags.Public)))) return;
        cursor.Index++;
        cursor.EmitLdloc(18);
        cursor.EmitLdarg0();
        cursor.EmitLdarg1();
        cursor.EmitDelegate<Action<int, int, int>>((index, i, j) =>
        {
            if (RecorderSystem.ShouldSpawnRecordItem<TheoryOfFreedom>())
                Main.player[index].QuickSpawnItem(WorldGen.GetItemSource_FromTileBreak(i, j), ModContent.ItemType<TheoryOfFreedom>());
        });
    }
#if false
    private void On_Player_RefreshDoubleJumps(On_Player.orig_RefreshDoubleJumps orig, Player self)
    {
        if (self.grappling[0] < 0)
        {
            orig.Invoke(self);
            return;
        }
        else
        {
            for (int i = 0; i < self.grapCount; i++)
            {
                Point coord = Main.projectile[self.grappling[i]].Center.ToTileCoordinates();
                var tile = Framing.GetTileSafely(coord);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    orig.Invoke(self);
                    break;
                }
            }
        }
    }

    private void On_Player_RefreshMovementAbilities(On_Player.orig_RefreshMovementAbilities orig, Player self, bool doubleJumps)
    {
        if (self.grappling[0] < 0)
        {
            orig.Invoke(self, doubleJumps);
            return;
        }
        else
        {
            for (int i = 0; i < self.grapCount; i++)
            {
                Point coord = Main.projectile[self.grappling[i]].Center.ToTileCoordinates();
                var tile = Framing.GetTileSafely(coord);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    orig.Invoke(self, doubleJumps);
                    break;
                }
            }
        }
    }

    private static void On_Player_GrappleMovement(On_Player.orig_GrappleMovement orig, Player self)
    {
        float cache = self.wingTime;
        orig.Invoke(self);
        if (self.grappling[0] < 0)
            return;

        bool antiReset = true;
        for (int i = 0; i < self.grapCount; i++)
        {
            Point coord = Main.projectile[self.grappling[i]].Center.ToTileCoordinates();
            var tile = Framing.GetTileSafely(coord);
            if (tile.HasTile && Main.tileSolid[tile.TileType])
            {
                antiReset = false;
                break;
            }
        }
        if (antiReset)
            self.wingTime = cache;
    }
#endif
}