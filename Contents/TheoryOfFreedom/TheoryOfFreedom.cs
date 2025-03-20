
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;

namespace MatterRecord.Contents.TheoryOfFreedom;
public class TheoryOfFreedom : ModItem
{
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<FreedomPlayer>().EquippedTOF = true;
        base.UpdateAccessory(player, hideVisual);
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
        //On_Player.GrappleMovement += On_Player_GrappleMovement;
        On_Player.RefreshMovementAbilities += On_Player_RefreshMovementAbilities;
        On_Player.RefreshDoubleJumps += On_Player_RefreshDoubleJumps;
        base.Load();
    }

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
                var tile = Main.tile[coord];
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
                var tile = Main.tile[coord];
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
            var tile = Main.tile[coord];
            if (tile.HasTile && Main.tileSolid[tile.TileType])
            {
                antiReset = false;
                break;
            }
        }
        if (antiReset)
            self.wingTime = cache;
    }
    public override void Unload()
    {
        On_Player.RefreshMovementAbilities -= On_Player_RefreshMovementAbilities;
        On_Player.RefreshDoubleJumps -= On_Player_RefreshDoubleJumps;

        base.Unload();
    }
}
public class FreedomPlayer : ModPlayer
{
    public bool EquippedTOF = false;
    public override void ResetEffects()
    {
        EquippedTOF = false;
        var v = Player.Center.ToTileCoordinates();
        base.ResetEffects();
    }
    public override void PreUpdate()
    {
        flyTimeCache = Player.wingTime;
        base.PreUpdate();
    }
    public override void PostUpdate()
    {

        base.PostUpdate();
    }
    public float flyTimeCache;
    public List<Point> targetTileCoords = [];
}
public class TOFGlobalProjectile : GlobalProjectile
{
    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        var mplr = player.GetModPlayer<FreedomPlayer>();
        if (mplr.EquippedTOF && mplr.targetTileCoords.Contains(new Point(x, y)) && Vector2.Distance(player.Center, new Vector2(x, y) * 16) > new Vector2(projectile.width, projectile.height).Length() * 1.5f)
            return true;
        return null;
    }
    public override bool? CanUseGrapple(int type, Player player)
    {

        var mplr = player.GetModPlayer<FreedomPlayer>();
        if (!mplr.EquippedTOF) return null;
        if ((type < ProjectileID.LunarHookSolar || type > ProjectileID.LunarHookStardust) && type != ProjectileID.Web)
            for (int num7 = 0; num7 < 1000; num7++)
            {
                if (Main.projectile[num7].active && Main.projectile[num7].owner == Main.myPlayer && Main.projectile[num7].type == type && Main.projectile[num7].ai[0] != 2f)
                    return null;
            }
        else if (type == ProjectileID.Web)
        {
            int c = 0;
            for (int num7 = 0; num7 < 1000; num7++)
            {
                if (Main.projectile[num7].active && Main.projectile[num7].owner == Main.myPlayer && Main.projectile[num7].type == type && Main.projectile[num7].ai[0] != 2f)
                    c++;
            }
            if (c >= 9)
                return null;
        }
        else
        {
            int c = 0;
            for (int num7 = 0; num7 < 1000; num7++)
            {
                if (Main.projectile[num7].active && Main.projectile[num7].owner == Main.myPlayer && Main.projectile[num7].ai[0] != 2f)// && Main.projectile[num7].type == type
                    c++;
            }
            if (c >= 4)
                return null;
        }

        int num17 = 3;
        if (type == 165)
            num17 = 8;

        if (type == 256)
            num17 = 2;

        if (type == 372)
            num17 = 2;

        if (type == 652)
            num17 = 1;

        if (type >= 646 && type <= 649)
            num17 = 4;

        Projectile proj = new Projectile();
        proj.SetDefaults(type);
        ProjectileLoader.NumGrappleHooks(proj, player, ref num17);
        mplr.targetTileCoords.Add(Main.MouseWorld.ToTileCoordinates());
        while (mplr.targetTileCoords.Count > num17)
            mplr.targetTileCoords.RemoveAt(0);
        for (int n = 0; n < 32; n++)
        {
            //Dust.NewDust(mplr.targetTileCoord.ToVector2() * 16, 16, 16, DustID.Frost);
            var d = Dust.NewDustPerfect(Main.MouseWorld, DustID.Firework_Green, Main.rand.NextVector2Unit() * 4);
            d.noGravity = true;
        }
        player.statLife -= player.statLifeMax2 / 100;
        CombatText.NewText(player.Hitbox, CombatText.DamagedFriendly, player.statLifeMax2 / 100);
        if (player.statLife <= 0)
            player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name}获得了「自由」"), 0, 0);

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            var packet = Mod.GetPacket();
            packet.Write((byte)PacketType.HookPointSync);
            packet.Write((byte)player.whoAmI);
            packet.Write((byte)mplr.targetTileCoords.Count);
            foreach (var p in mplr.targetTileCoords)
            {
                packet.Write(p.X);
                packet.Write(p.Y);
            }
            packet.Send(-1, player.whoAmI);
        }
        return base.CanUseGrapple(type, player);
    }
}