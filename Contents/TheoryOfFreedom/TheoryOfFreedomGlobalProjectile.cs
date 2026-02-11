using Microsoft.Xna.Framework;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Localization;

namespace MatterRecord.Contents.TheoryOfFreedom;

public class TheoryOfFreedomGlobalProjectile:GlobalProjectile
{
    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        var mplr = player.GetModPlayer<TheTheoryOfFreedomPlayer>();
        if (mplr.EquippedTOF && !mplr.CanHookPlatform && Main.tileSolidTop[Framing.GetTileSafely(x, y).TileType])
            return false;
        if (mplr.EquippedTOF && mplr.TargetTileCoords.Contains(new Point(x, y)) && Vector2.Distance(player.Center, new Vector2(x, y) * 16) > new Vector2(projectile.width, projectile.height).Length() * 1.5f)
            return true;
        return null;
    }

    public override bool? CanUseGrapple(int type, Player player)
    {
        var mplr = player.GetModPlayer<TheTheoryOfFreedomPlayer>();
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
        mplr.TargetTileCoords.Add(Main.MouseWorld.ToTileCoordinates());
        while (mplr.TargetTileCoords.Count > num17)
            mplr.TargetTileCoords.RemoveAt(0);
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
            HookPointSync.Get(player.whoAmI, [.. mplr.TargetTileCoords]).Send(-1, player.whoAmI);

        return base.CanUseGrapple(type, player);
    }

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (player.GetModPlayer<TheTheoryOfFreedomPlayer>().EquippedTOF)
            speed *= 1.5f;
        base.GrapplePullSpeed(projectile, player, ref speed);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (player.GetModPlayer<TheTheoryOfFreedomPlayer>().EquippedTOF)
        {
            speed *= 3f;
            speed = MathHelper.Min(speed, 100);
        }

        base.GrappleRetreatSpeed(projectile, player, ref speed);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (projectile.aiStyle == ProjAIStyleID.Hook && Main.player[projectile.owner].GetModPlayer<TheTheoryOfFreedomPlayer>().EquippedTOF)
        {
            projectile.velocity *= 2;
        }
        base.OnSpawn(projectile, source);
    }
}
