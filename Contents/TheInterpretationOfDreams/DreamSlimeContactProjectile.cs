using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

/// <summary>
/// 梦境史莱姆的战斗弹幕，用于造成接触伤害，替代碰撞箱检测。
/// </summary>
public class DreamSlimeContactProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.hide = true;
        Projectile.aiStyle = -1;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
        // 使用默认伤害类型，避免额外加成干扰
        Projectile.DamageType = DamageClass.Default;
    }

    public override void AI()
    {
        // 获取宿主史莱姆
        NPC owner = Main.npc[(int)Projectile.ai[0]];
        if (!owner.active || owner.type != ModContent.NPCType<DreamSlime>() || owner.aiStyle != -1)
        {
            Projectile.Kill();
            return;
        }

        Projectile.Center = owner.Center;
        Projectile.timeLeft = 2;

        // 动态更新伤害（基于玩家防御）
        int damage = GetDamageFromOwner();
        if (Projectile.damage != damage)
            Projectile.damage = damage;
    }

    private int GetDamageFromOwner()
    {
        int damage = 12;
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            var player = Main.LocalPlayer;
            if (player != null && player.active)
            {
                // 计算玩家三件套防御总和
                int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;
                damage = (int)MathHelper.Max(defense * 0.85f, 12);
            }
        }
        else
        {
            foreach (var player in Main.player)
            {
                if (player != null && player.active)
                {
                    int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;
                    int curDamage = (int)MathHelper.Max(defense * 0.85f, 12);
                    if (curDamage > damage)
                        damage = curDamage;
                }
            }
        }
        return (int)MathHelper.Max(damage, 1);
    }

    /// <summary>
    /// 无视敌人的防御，使伤害全额生效。
    /// </summary>
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        // 将防御减免系数设为 0，彻底无视防御
        modifiers.Defense *= 0f;
    }
}