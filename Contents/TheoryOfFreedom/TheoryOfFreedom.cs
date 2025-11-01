using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.TheoryOfFreedom;

[AutoloadEquip(EquipType.Wings)]
public class TheoryOfFreedom : ModItem,IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheoryOfFreedom;
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var mplr = player.GetModPlayer<FreedomPlayer>();
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

    //public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
    //{
    //    if (equippedItem.ModItem is TheoryOfFreedom)
    //        return incomingItem.wingSlot == -1;
    //    if(incomingItem.ModItem is TheoryOfFreedom)
    //        return equippedItem.wingSlot == -1;
    //    return true;
    //}

    public override void Load()
    {
        //On_Player.GrappleMovement += On_Player_GrappleMovement;
        // On_Player.RefreshMovementAbilities += On_Player_RefreshMovementAbilities;
        // On_Player.RefreshDoubleJumps += On_Player_RefreshDoubleJumps;
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

    public override void Unload()
    {
        // On_Player.RefreshMovementAbilities -= On_Player_RefreshMovementAbilities;
        // On_Player.RefreshDoubleJumps -= On_Player_RefreshDoubleJumps;

        base.Unload();
    }
}

public class FreedomPlayer : ModPlayer
{
    public bool EquippedTOF = false;
    public bool CanHookPlatform;

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(CanHookPlatform), CanHookPlatform);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet<bool>(nameof(CanHookPlatform), out bool value))
            CanHookPlatform = value;
        base.LoadData(tag);
    }

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

    private static ModKeybind CanHookPlatformSwitch { get; set; }

    public override void Load()
    {
        CanHookPlatformSwitch = KeybindLoader.RegisterKeybind(Mod, "CanHookPlatform", Keys.L);
        base.Load();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (CanHookPlatformSwitch.JustPressed)
        {
            CanHookPlatform = !CanHookPlatform;
            Main.NewText(Language.GetTextValue($"Mods.{nameof(MatterRecord)}.Items.{nameof(TheoryOfFreedom)}.{(CanHookPlatform ? "CanHookOnPlatform" : "CantHookOnPlatform")}"), CanHookPlatform ? Color.Lime : Color.Green);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                SyncPlayer(-1, Player.whoAmI, false);
        }
        base.ProcessTriggers(triggersSet);
    }

    public void ReceivePlayerSync(BinaryReader reader)
    {
        CanHookPlatform = reader.ReadBoolean();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        FreedomPlayer clone = (FreedomPlayer)targetCopy;
        clone.CanHookPlatform = CanHookPlatform;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        FreedomPlayer clone = (FreedomPlayer)clientPlayer;

        if (CanHookPlatform != clone.CanHookPlatform)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)PacketType.TheoryOfFreedomHookPlatformAbility);
        packet.Write((byte)Player.whoAmI);
        packet.Write(CanHookPlatform);
        packet.Send(toWho, fromWho);
        base.SyncPlayer(toWho, fromWho, newPlayer);
    }
}

public class TOFGlobalProjectile : GlobalProjectile
{
    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        var mplr = player.GetModPlayer<FreedomPlayer>();
        if (mplr.EquippedTOF && !mplr.CanHookPlatform && Main.tileSolidTop[Framing.GetTileSafely(x, y).TileType])
            return false;
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
                if (Main.projectile[num7].active 
                    && Main.projectile[num7].owner == Main.myPlayer 
                    && Main.projectile[num7].type == type 
                    && Main.projectile[num7].ai[0] != 2f)
                    return null;
            }
        else if (type == ProjectileID.Web)
        {
            int c = 0;
            for (int num7 = 0; num7 < 1000; num7++)
            {
                if (Main.projectile[num7].active
                    && Main.projectile[num7].owner == Main.myPlayer 
                    && Main.projectile[num7].type == type 
                    && Main.projectile[num7].ai[0] != 2f)
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
                if (Main.projectile[num7].active 
                    && Main.projectile[num7].owner == Main.myPlayer 
                    && Main.projectile[num7].type is >= ProjectileID.LunarHookSolar and <= ProjectileID.LunarHookStardust 
                    && Main.projectile[num7].ai[0] != 2f)// && Main.projectile[num7].type == type
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
            player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromKey($"Mods.{nameof(MatterRecord)}.Items.{nameof(TheoryOfFreedom)}.GotFreedom", player.name)), 0, 0);

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

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (player.GetModPlayer<FreedomPlayer>().EquippedTOF)
            speed *= 1.5f;
        base.GrapplePullSpeed(projectile, player, ref speed);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (player.GetModPlayer<FreedomPlayer>().EquippedTOF)
        {
            speed *= 3f;
            speed = MathHelper.Min(speed, 100);
        }

        base.GrappleRetreatSpeed(projectile, player, ref speed);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (projectile.aiStyle == 7 && Main.player[projectile.owner].GetModPlayer<FreedomPlayer>().EquippedTOF)
        {
            projectile.velocity *= 2;
        }
        base.OnSpawn(projectile, source);
    }
}