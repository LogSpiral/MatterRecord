using Microsoft.Xna.Framework;
using System.Reflection;
using Terraria.Audio;
using Terraria.DataStructures;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFlies : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 50;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useAmmo = AmmoID.Bullet;
        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 16;
        Item.DamageType = DamageClass.Ranged;
        Item.rare = ItemRarityID.Lime;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;
        base.SetDefaults();
    }
    public override bool CanUseItem(Player player)
    {
        if (player.controlUseTile)
            return true;
        if (player.controlUseItem)
        {
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            if (mplr.IsInTrialMode)
                return player.CheckMana(mplr.StoredAmmoCount > 0 ? 100 : 5) && !player.HasBuff(BuffID.ManaSickness);
            else
                return true;
        }
        return true;
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        ref int _chargeTimer = ref mplr.ChargeTimer;
        player.itemLocation -= player.itemRotation.ToRotationVector2() * 8 * player.direction;
        if (player.altFunctionUse == 2)
            Item.UseSound = null;
        if (player.itemAnimation == 1 && player.altFunctionUse == 2)
        {
            if (player.controlUseTile && (_chargeTimer < 10 || (player.CheckMana(200) && mplr.StoredAmmoCount < 6)))
            {
                player.itemAnimation = 2;
                _chargeTimer++;
                if (_chargeTimer > 10)
                {
                    float factor = Utils.GetLerpValue(10, 120, _chargeTimer, true);
                    if (_chargeTimer % 3 == 0)
                        for (int k = 0; k < 4; k++)
                        {
                            for (float f = 0; f < MathHelper.PiOver2 * factor; f += 0.1f)
                            {
                                var unit = (f + k * MathHelper.PiOver2).ToRotationVector2();
                                var dust = Dust.NewDustPerfect(
                                    player.Center + unit * 64,
                                    DustID.GreenTorch,
                                    new Vector2(-unit.Y, unit.X) * 4,
                                    0,
                                    Color.White * .25f,
                                    Main.rand.NextFloat()
                                    * (factor == 1 ? 2f : 1));
                                dust.noGravity = true;
                            }
                        }
                    if (_chargeTimer == 120)
                        SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
                }
            }
            else
            {
                if (_chargeTimer <= 10)
                {
                    mplr.IsInTrialMode = !mplr.IsInTrialMode;
                    SoundEngine.PlaySound(SoundID.Item20);
                }
                else if (_chargeTimer > 120)
                {
                    mplr.StoredAmmoCount++;
                    player.CheckMana(200, true);
                    SoundEngine.PlaySound(SoundID.Item45);
                }
                _chargeTimer = 0;
            }
        }
        player.scope = false;
        base.UseStyle(player, heldItemFrame);
    }
    public override bool AltFunctionUse(Player player) => true;
    public override void HoldStyle(Player player, Rectangle heldItemFrame)
    {
        var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        player.scope = false;
        if (player.altFunctionUse == 2)
            Item.UseSound = null;
        if (mplr.IsInTrialMode)
        {
            player.itemRotation += MathHelper.PiOver4 * player.direction;
            player.itemLocation -= player.itemRotation.ToRotationVector2() * 8 * player.direction;
            Item.holdStyle = ItemHoldStyleID.HoldHeavy;
            // Item.DamageType = ModContent.GetInstance<RangedMagicDamage>();
        }
        else
        {
            Item.holdStyle = ItemHoldStyleID.None;
            // Item.DamageType = DamageClass.Ranged;
        }

        base.HoldStyle(player, heldItemFrame);
    }
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        if (player.altFunctionUse == 0)
        {
            if (mplr.IsInTrialMode)
            {
                if (mplr.StoredAmmoCount > 0)
                {
                    mplr.StoredAmmoCount--;
                    SoundEngine.PlaySound(SoundID.Item38, player.Center);
                    for (int n = 0; n < 20; n++)
                    {
                        var dust = Dust.NewDustPerfect(
                            position + Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 8,
                            DustID.GreenTorch,
                            velocity * Main.rand.NextFloat() * 4,
                            0,
                            Color.White * .75f,
                            Main.rand.NextFloat() * 4);
                        dust.noGravity = true;
                    }
                    player.CheckMana(100, true);
                    var proj = Projectile.NewProjectileDirect(source, position, velocity * 2, ModContent.ProjectileType<AnnihilationBullet>(), damage, knockback, player.whoAmI);
                    proj.MaxUpdates *= 2;
                }
                else
                {
                    player.CheckMana(5, true);
                    SoundEngine.PlaySound(SoundID.Item36, player.Center);
                    for (int n = 0; n < 10; n++)
                    {
                        var dust = Dust.NewDustPerfect(
                            position + Main.rand.NextVector2Unit() * Main.rand.NextFloat() * 4,
                            DustID.GreenTorch,
                            velocity * Main.rand.NextFloat() * 2,
                            0,
                            Color.White * .5f,
                            Main.rand.NextFloat() * 1.5f);
                        dust.noGravity = true;
                    }

                    var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
                    proj.MaxUpdates *= 2;
                    proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>().IsFromTrialMode = true;
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item11, player.Center);
                return true;
            }
        }
        return false;

    }
}
