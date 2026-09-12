using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace MatterRecord.Contents.TheManInTheHighCastle;

public class TheManInTheHighCastlePlayer : ModPlayer
{
    public bool HasHookWeapon { get; set; }
    private bool HasHookWeaponOld { get; set; }
    public int GrapCountCache { get; set; }
    public bool UseRotation { get; set; }
    public float Rotation { get; set; }
    private Vector2 OldVelocity { get; set; }

    // 钩爪自动取消的距离阈值
    private const float HookCancelDistance = 48f;
    // 牵引速度缓存（默认值，实际会在拉回时更新）
    private float GrapplePullSpeedCache = 9f;
    // 当前被追踪的钩爪索引（-1 表示无）
    private int ActiveHookIndex = -1;

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
    }

    public override bool CanUseItem(Item item)
    {
        return Player.ownedProjectileCounts[ModContent.ProjectileType<TheManInTheHighCastleProj>()] == 0;
    }

    public override void PostUpdateBuffs()
    {
        // ---- 更新牵引速度缓存：在钩爪拉回状态记录玩家速度大小 ----
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile hook = Main.projectile[i];
            if (!hook.active || hook.owner != Player.whoAmI || hook.aiStyle != ProjAIStyleID.Hook)
                continue;

            if (hook.ai[0] == 1f)
            {
                float speed = Player.velocity.Length();
                if (speed > 0.1f)
                    GrapplePullSpeedCache = speed;
            }
        }

        // ---- 原有：钩爪移动时生成弹幕 ----
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

        // ---- 验证活动钩爪是否仍然有效 ----
        if (ActiveHookIndex >= 0)
        {
            if (ActiveHookIndex >= Main.maxProjectiles)
            {
                ActiveHookIndex = -1;
            }
            else
            {
                Projectile ah = Main.projectile[ActiveHookIndex];
                if (!ah.active || ah.owner != Player.whoAmI || ah.aiStyle != ProjAIStyleID.Hook
                    || ah.ai[0] == 0f || ah.ai[0] == 1f)
                {
                    ActiveHookIndex = -1;
                }
            }
        }

        // ---- 若没有活动钩爪，寻找第一个锁定的钩爪 ----
        if ((HasHookWeapon || HasHookWeaponOld) && ActiveHookIndex < 0)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile hook = Main.projectile[i];
                if (!hook.active || hook.owner != Player.whoAmI || hook.aiStyle != ProjAIStyleID.Hook)
                    continue;
                if (hook.ai[0] == 0f || hook.ai[0] == 1f)
                    continue;

                ActiveHookIndex = i;
                break;
            }
        }

        // ---- 处理活动钩爪：到达钩爪点后自动取消 + 反弹 ----
        if ((HasHookWeapon || HasHookWeaponOld) && ActiveHookIndex >= 0)
        {
            Projectile hook = Main.projectile[ActiveHookIndex];
            if (hook.active)
            {
                float distance = Vector2.Distance(Player.Center, hook.Center);
                if (distance < HookCancelDistance)
                {
                    // 判断钩爪点是否为实心块（排除平台）
                    bool isSolidBlock = false;
                    Point tilePos = hook.Center.ToTileCoordinates();
                    if (tilePos.X >= 0 && tilePos.Y >= 0
                        && tilePos.X < Main.maxTilesX && tilePos.Y < Main.maxTilesY)
                    {
                        Tile tile = Framing.GetTileSafely(tilePos.X, tilePos.Y);
                        if (tile.HasTile
                            && Main.tileSolid[tile.TileType]
                            && !Main.tileSolidTop[tile.TileType])
                        {
                            isSolidBlock = true;
                        }
                    }

                    // 取消钩爪
                    hook.Kill();
                    hook.netUpdate = true;

                    // ---- 压缩 Player.grappling 数组，移除被取消的钩爪 ----
                    int write = 0;
                    for (int j = 0; j < Player.grappling.Length; j++)
                    {
                        if (Player.grappling[j] != -1 && Player.grappling[j] != ActiveHookIndex)
                            Player.grappling[write++] = Player.grappling[j];
                    }
                    for (int j = write; j < Player.grappling.Length; j++)
                        Player.grappling[j] = -1;
                    Player.grapCount = write;

                    // 若为实心块，向反方向弹开，速度 = 牵引速度
                    if (isSolidBlock)
                    {
                        Vector2 bounceDir = Player.Center - hook.Center;
                        if (bounceDir.LengthSquared() < 0.01f)
                            bounceDir = -Vector2.UnitY;
                        bounceDir.Normalize();

                        Player.velocity = bounceDir * GrapplePullSpeedCache;

                        SoundEngine.PlaySound(SoundID.Item56, Player.Center);
                    }

                    // 处理完毕后重置，下一帧会寻找新的活动钩爪
                    ActiveHookIndex = -1;
                }
            }
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