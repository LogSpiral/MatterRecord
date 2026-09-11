using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TheManInTheHighCastle;

public class TheManInTheHighCastlePlayer : ModPlayer
{
    public bool HasHookWeapon { get; set; }
    private bool HasHookWeaponOld { get; set; }
    public int GrapCountCache { get; set; }
    public bool UseRotation { get; set; }
    public float Rotation { get; set; }
    private Vector2 OldVelocity { get; set; }
    public override void ResetEffects()
    {
        HasHookWeaponOld = HasHookWeapon;
        HasHookWeapon = false;
        if (Rotation > 0.01f || Rotation < -0.01f)
            Rotation *= 0.9f;
        else 
        {
            Rotation = 0;
            UseRotation = false;
        }
        // GrapCountCache = 0;
    }
    public override bool CanUseItem(Item item)
    {
        return Player.ownedProjectileCounts[ModContent.ProjectileType<TheManInTheHighCastleProj>()] == 0;
    }
    public override void PostUpdateBuffs()
    {
        if (Player.whoAmI == Main.myPlayer
            && HasHookWeaponOld
            && Player.grapCount > 0
            && Player.velocity != OldVelocity
            && Player.velocity.LengthSquared() > 1
            && Player.ownedProjectileCounts[ModContent.ProjectileType<TheManInTheHighCastleProj>()] == 0)
        {
            OldVelocity = Player.velocity;
            Projectile.NewProjectile(Player.GetSource_FromThis(),
                Player.Center,
                Vector2.Zero,
                ModContent.ProjectileType<TheManInTheHighCastleProj>(),
                Player.GetBestPickaxe()?.pick ?? 1, 0, Main.myPlayer);
        }
        GrapCountCache = Player.grapCount;
    }
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (UseRotation)
        {
            drawInfo.rotation = Rotation;
            drawInfo.rotationOrigin = new(10, 28);
        }

    }
}
