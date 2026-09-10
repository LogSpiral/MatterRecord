using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheOldManAndTheSea;

public class TheOldManAndTheSeaBobber : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.BobberWooden);
        Projectile.friendly = true;
        Projectile.penetrate = -1;                    // 无限穿透
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 40;          // 无敌帧 40
        DrawOriginOffsetY = 0;
        Projectile.bobber = true;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Player player = Main.player[Projectile.owner];
        var mp = player.GetModPlayer<TheOldManAndTheSeaPlayer>();

        if (mp.IsActivated)
        {
            Projectile.friendly = true;

            // 1. 找出背包中基础伤害最高的武器
            int maxDamage = 0;
            foreach (Item item in player.inventory)
            {
                if (!item.IsAir && item.damage > maxDamage)
                    maxDamage = item.damage;
            }
            if (maxDamage == 0) maxDamage = 10;       // 无武器时保底

            // 2. 获取玩家当前总渔力（包含饰品、药水、鱼饵等所有加成）
            int fishingLevel = player.fishingSkill;

            // 3. 基础伤害 = 武器伤害 × (1 + 渔力 / 100)
            //    例如：渔力 75 时，伤害倍率 = 1.75 倍；渔力 200 时 = 3.0 倍
            Projectile.damage = (int)(maxDamage * (1 + fishingLevel / 100.0));
        }
        else
        {
            Projectile.friendly = false;
            Projectile.damage = 0;
        }
    }

    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Projectile.friendly)
        {
            // 额外增加敌人最大生命值的 1%（向下取整）
            int extra = (int)(target.lifeMax * 0.01f);
            modifiers.FlatBonusDamage += extra;
        }
    }
}