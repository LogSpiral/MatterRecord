using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;

namespace MatterRecord.Contents.LordOfTheFlies;
public class LordOfTheFlies : ModItem
{
    public override void SetDefaults()
    {
        Item.damage = 1;
        Item.useTime = Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useAmmo = AmmoID.Bullet;
        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 16;
        Item.DamageType = DamageClass.Ranged;
        Item.rare = ItemRarityID.Lime;
        Item.holdStyle = ItemHoldStyleID.HoldHeavy;
        Item.noMelee = true;
        ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        base.SetDefaults();
    }

    public override bool CanUseItem(Player player)
    {
        ItemID.Sets.gunProj[Item.type] = true;
        if (player.controlUseTile)
            return true;
        if (player.controlUseItem)
        {
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            if (mplr.IsInTrialMode)
                return mplr.ChargingEnergy >= 3;
            else
                return true;
        }
        return true;
    }
    private static SoundStyle SwitchModeSoundEffect { get; } = new SoundStyle("MatterRecord/Assets/Sounds/shotgun_reload_clip3");
    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        ref int _chargeTimer = ref mplr.ChargeTimer;
        player.itemLocation -= player.itemRotation.ToRotationVector2() * 8 * player.direction;
        if (player.itemAnimation == 1)
        {
            if (player.altFunctionUse == 2 || mplr.IsChargingAnnihilation)
            {
                if (player.controlUseTile && (_chargeTimer < 10 || (mplr.ChargingEnergy == 120 && mplr.StoredAmmoCount < 6)) && _chargeTimer <= 130)
                {
                    player.itemAnimation = 2;
                    _chargeTimer++;
                    if (mplr.ChargingEnergy != 120 && mplr.StoredAmmoCount < 6 && _chargeTimer == 10 && !mplr.IsInTrialMode)
                        _chargeTimer--;
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
                                        new Vector2(-unit.Y, unit.X) * 4 + player.velocity,
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
                    if (!mplr.IsChargingAnnihilation)
                    {
                        mplr.IsChargingAnnihilation = true;
                        if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
                            mplr.SyncAnniCharging(-1, Main.myPlayer);
                    }
                }
                else
                {
                    if (_chargeTimer < 9 || (_chargeTimer <= 10 && mplr.IsInTrialMode))
                    {
                        mplr.IsInTrialMode = !mplr.IsInTrialMode;
                        if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                            mplr.SyncPlayer(-1, player.whoAmI, false);
                        SoundEngine.PlaySound(SwitchModeSoundEffect, player.Center);
                        if (mplr.IsInTrialMode)
                        {
                            var box = player.Hitbox;
                            //box.Offset(0, -32);
                            CombatText.NewText(box, Color.Green, this.GetLocalizedValue("Clatter"));
                        }
                    }

                    else if (_chargeTimer > 120)
                    {
                        mplr.StoredAmmoCount++;
                        mplr.ChargingEnergy = 0;
                        if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                            mplr.SyncPlayer(-1, player.whoAmI, false);
                        SoundEngine.PlaySound(SoundID.Item45, player.Center);
                        mplr.RightCooldown = 30;
                    }
                    _chargeTimer = 0;
                    if (mplr.IsChargingAnnihilation)
                    {
                        mplr.IsChargingAnnihilation = false;
                        if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
                            mplr.SyncAnniCharging(-1, Main.myPlayer);
                    }
                }
            }
        }
        if (player.altFunctionUse == 0)
        {
            if (player.itemTime == player.itemTimeMax - 1)
            {
                if (player.controlUseItem && mplr.StoredAmmoCount > 0 && mplr.ChargingEnergy >= 20 && mplr.IsInTrialMode)
                {
                    player.itemAnimation = player.itemAnimationMax;
                    player.itemTime = player.itemTimeMax;
                    _chargeTimer++;
                    if (_chargeTimer > 3)
                    {
                        //float factor = Utils.GetLerpValue(3, 40, _chargeTimer, true);
                        //if (_chargeTimer % 3 == 0)
                        //    for (int k = 0; k < 4; k++)
                        //    {
                        //        for (float f = 0; f < MathHelper.PiOver2 * factor; f += 0.1f)
                        //        {
                        //            var unit = (f + k * MathHelper.PiOver2).ToRotationVector2();
                        //            var dust = Dust.NewDustPerfect(
                        //                player.Center + unit * 64,
                        //                DustID.GreenTorch,
                        //                new Vector2(-unit.Y, unit.X) * -4 + player.velocity,
                        //                0,
                        //                Color.White * .25f,
                        //                Main.rand.NextFloat()
                        //                * (factor == 1 ? 2f : 1));
                        //            dust.noGravity = true;
                        //        }
                        //    }
                        if (_chargeTimer == 40)
                            SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
                    }
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Vector2 target = Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter);
                        float rotation = target.ToRotation();
                        player.direction = Math.Sign(target.X);
                        player.itemRotation = rotation;
                        if (player.direction < 0)
                            player.itemRotation += MathHelper.Pi;
                        player.itemRotation += Main.rand.NextFloat(Utils.GetLerpValue(3, 40, _chargeTimer, true) * 0.05f);
                        if (_chargeTimer % 4 == 0)
                        {
                            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
                            NetMessage.SendData(MessageID.ShotAnimationAndSound, -1, -1, null, Main.myPlayer);
                        }
                    }
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - (player.direction < 0 ? MathHelper.Pi : 0) - MathHelper.PiOver2);
                }
                else
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        int Damage = player.GetWeaponDamage(Item);
                        float KnockBack = Item.knockBack;
                        int usedAmmoItemId = 0;
                        int projToShoot = Item.shoot;
                        float speed = Item.shootSpeed;
                        int damage = Item.damage;
                        bool canShoot = true;
                        player.PickAmmo(Item, ref projToShoot, ref speed, ref canShoot, ref Damage, ref KnockBack, out usedAmmoItemId);

                        Vector2 center = player.RotatedRelativePoint(player.MountedCenter);
                        ManualShoot(
                            player,
                            (EntitySource_ItemUse_WithAmmo)player.GetSource_ItemUse_WithPotentialAmmo(Item, usedAmmoItemId),
                            center,
                            (Main.MouseWorld - center).SafeNormalize(default) * speed,
                            projToShoot,
                            Damage,
                            KnockBack
                            );
                        _chargeTimer = 0;
                    }
                }
            }
            else
            {
                if (player.whoAmI == Main.myPlayer && mplr.IsInTrialMode)
                {
                    Vector2 target = Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter);
                    float rotation = target.ToRotation();
                    player.direction = Math.Sign(target.X);

                    rotation -= player.direction * (player.itemTime / (float)player.itemTimeMax) * .5f;

                    player.itemRotation = rotation;
                    if (player.direction < 0)
                        player.itemRotation += MathHelper.Pi;
                    if (player.itemTime % 4 == 0)
                    {
                        NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
                        NetMessage.SendData(MessageID.ShotAnimationAndSound, -1, -1, null, Main.myPlayer);
                    }
                }
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation - (player.direction < 0 ? MathHelper.Pi : 0) - MathHelper.PiOver2);
            }
        }
        player.scope = false;
        base.UseStyle(player, heldItemFrame);
    }

    public override bool AltFunctionUse(Player player) => player.GetModPlayer<LordOfTheFliesPlayer>().RightCooldown <= 0;

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

    private static void ManualShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        if (mplr.IsInTrialMode)
        {
            if (mplr.StoredAmmoCount > 0 && mplr.ChargeTimer >= 40)
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
                mplr.ChargingEnergy -= 20;
                var proj = Projectile.NewProjectileDirect(source, position, velocity * 2, ModContent.ProjectileType<AnnihilationBullet>(), damage, knockback, player.whoAmI);
                proj.MaxUpdates *= 2;
                proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>().IsFromTrialMode = true;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
                player.GetModPlayer<MatterRecordPlayer>().strengthOfShake += 5.0f;
            }
            else
            {
                mplr.ChargingEnergy -= 3;
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
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
            }
        }
        else
        {
            Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
            SoundEngine.PlaySound(SoundID.Item11, player.Center);
        }
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        /*
        var index = tooltips.FindIndex(0, line => line.Name.StartsWith("Prefix"));
        if (index == -1)
            index = tooltips.FindIndex(0, line => line.Name == "JourneyResearch");
        var mplr = Main.LocalPlayer.GetModPlayer<LordOfTheFliesPlayer>();
        var count = mplr.NPCKillCount;
        var factor = count / (count + 20f);
        var count2 = mplr.PlayerKillCount;
        var factor2 = count2 / (count2 + 20f);

        var timerFac = 0.5f + 0.5f * MathF.Cos(Main.GlobalTimeWrappedHourly);
        var line = new TooltipLine(Mod, "NPCKillCount", Language.GetTextValue(this.GetLocalizationKey("NPCKillCount"), count)) { OverrideColor = Color.Lerp(Color.DarkGray, Color.DarkGreen, factor * timerFac) };
        var line2 = new TooltipLine(Mod, "PlrKillCount", Language.GetTextValue(this.GetLocalizationKey("KillCount"), count2)) { OverrideColor = Color.Lerp(Color.DarkGray, Color.DarkRed, factor2 * timerFac) };
        if (index == -1)
        {
            tooltips.Add(line);
            if (count2 > 0)
                tooltips.Add(line2);
        }
        else
        {
            tooltips.Insert(index, line);
            if (count2 > 0)
                tooltips.Insert(index + 1, line2);
        }
        */
        base.ModifyTooltips(tooltips);
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        float rangeFactor = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
        float genericFactor = player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);

        rangeFactor -= genericFactor;
        genericFactor += rangeFactor - 1;

        var critFactor = player.GetTotalCritChance(DamageClass.Ranged) * .01f;
        critFactor += .04f;
        damage.Multiplicative *= Math.Max(player.statDefense * (0.75f + rangeFactor + critFactor) / (1 + genericFactor), 1);
        //var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        //var count = mplr.NPCKillCount;
        //var factor = count / (count + 200f);
        //var count2 = mplr.PlayerKillCount;
        //var factor2 = count / (count + 20f);
        //damage.Additive += factor * .5f;
        //damage.Additive += factor2;
        //base.ModifyWeaponDamage(player, ref damage);
    }
}

public class LordOfTheFliesAnnihilationBulletLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeldItem);

    public override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (Main.gameMenu) return;
        var player = drawInfo.drawPlayer;
        if (player.whoAmI != Main.myPlayer) return;
        if (player.HeldItem.type != ModContent.ItemType<LordOfTheFlies>()) return;
        if (!player.controlUseItem) return;
        var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
        float chargeTimer = mplr.ChargeTimer;
        if (chargeTimer < 3) return;
        float factor = Utils.GetLerpValue(3, 40, chargeTimer, true);
        float fac2 = MathHelper.SmoothStep(0, 1, factor);
        factor = factor == 1 ? 1 : factor * .5f;

        float factor2 = 0;
        float factor3 = 0;
        bool flag = chargeTimer > 40;

        if (flag)
        {
            factor2 = Utils.GetLerpValue(40, 60, chargeTimer, true);
            factor3 = 1 - MathF.Cos(MathF.Tau * MathF.Sqrt(factor2));
            factor3 *= .125f;
        }

        var center = player.RotatedRelativePoint(player.MountedCenter);
        float rotation = (Main.MouseWorld - center).ToRotation();
        center -= (rotation + MathHelper.PiOver2 * player.direction).ToRotationVector2() * 6;
        var color = Color.White with { A = 0 };
        drawInfo.DrawDataCache.Add(
            new DrawData(
                TextureAssets.MagicPixel.Value,
                center - Main.screenPosition,
                null,
                color * factor,
                rotation - MathHelper.PiOver2,
                new Vector2(.5f),
                new Vector2(2, fac2 * 1.5f), 0, 0));

        if (flag)
        {
            drawInfo.DrawDataCache.Add(
                new DrawData(
                    TextureAssets.MagicPixel.Value,
                    center - Main.screenPosition,
                    null,
                    color * factor3 * .25f,
                    rotation - MathHelper.PiOver2,
                    new Vector2(.5f),
                    new Vector2(2 + factor2 * 16, fac2 * 1.5f), 0, 0));
        }

        drawInfo.DrawDataCache.Add(
            new DrawData(
                ModAsset.crosshair.Value,
                Main.MouseScreen,
                null,
                color * .25f * factor,
                Main.GlobalTimeWrappedHourly * 4,
                new Vector2(32),
                1f, 0, 0));

        if (flag)
        {
            drawInfo.DrawDataCache.Add(
                new DrawData(
                    ModAsset.crosshair.Value,
                    Main.MouseScreen,
                    null,
                    color * .25f * factor3,
                    Main.GlobalTimeWrappedHourly * 4,
                    new Vector2(32),
                    1f + 3 * factor2, 0, 0));
        }
    }
}