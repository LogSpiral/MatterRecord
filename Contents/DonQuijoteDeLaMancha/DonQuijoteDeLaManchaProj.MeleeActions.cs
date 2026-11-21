using MatterRecord.Contents.DonQuijoteDeLaMancha.Core;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.MeleeCore;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.StandardMelee;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using MatterRecord.Contents.TortoiseShell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using MatterRecord.Contents.TheManInTheHighCastle;
using MatterRecord.Contents.EternalWine;
namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

partial class DonQuijoteDeLaManchaProj
{
    private class CustomSwooshInfo : SwooshInfo
    {
        private bool _wingTimeRestored;
        private bool _comboAdded;

        public override void OnStartSingle()
        {
            _wingTimeRestored = false;
            _comboAdded = false;
            base.OnStartSingle();
        }

        public void HandleHit(Entity victim, int damageDone)
        {
            var player = Owner as Player;
            if (player == null) return;
            var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();

            if (DonQuijoteProgression.Tier7_HitRestoreWing && !_wingTimeRestored && player.wingTimeMax > 0)
            {
                player.wingTime += 1f;
                if (player.wingTime > player.wingTimeMax)
                    player.wingTime = player.wingTimeMax;
                _wingTimeRestored = true;
            }

            if (!_comboAdded && mplr.TryAddCombo())
                _comboAdded = true;
        }
    }

    private class DonQuijoteDeLaManchaDash : MeleeAction
    {
        public override bool Attacktive => Factor < .65f;

        private Vector2 originVelocity;
        private WindMill windMill;
        private bool _noMovement;
        private bool _wingTimeRestored;
        private bool _comboAdded;

        public override void OnEndAttack()
        {
            if (Projectile.owner != Main.myPlayer) return;

            Player plr = Owner as Player;

            if (!_noMovement)
            {
                Owner.velocity *= MathHelper.Clamp(.05f * (1 + MathF.Sqrt(originVelocity.Length())), 0, 1);
                Owner.velocity += originVelocity * MathF.Pow(0.9f, originVelocity.Length());
                plr?.fallStart = (int)(plr.position.Y / 16f);
            }

            if (plr != null)
            {
                var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (DonQuijoteProgression.Tier2_StabAfterDash)
                {
                    var maxT = 60;
                    if (mplr.itemDefinition != null && mplr.itemDefinition.Type > 0)
                        maxT = new Item(mplr.itemDefinition.Type).useAnimation;
                    mplr.StabTimeLeft = Math.Max((maxT < 30 ? 6 * maxT + 300 : 12 * maxT + 120) * 3 / 4, 300);
                }
                else
                {
                    mplr.StabTimeLeft = 0;
                }
                mplr.NextHitImmune = false;
                mplr.Dashing = false;
                mplr.SendSyncAI();

                if (!_noMovement && Main.netMode is NetmodeID.MultiplayerClient)
                    VelocitySync.Get(plr.whoAmI, plr.velocity).Send();
            }
            base.OnEndAttack();
        }

        public override void OnAttack()
        {
            if (Owner is Player plr)
            {
                plr.GetModPlayer<MatterRecordPlayer>().strengthOfShake += 2f / (1f + TimerMax / 30f);
                plr.GetModPlayer<MatterRecordPlayer>().ultraFallEnable = true;
                plr.noKnockback = true;
            }
            var rand = Main.rand.NextFloat(0.25f, 0.5f);
            for (int k = 0; k < 15; k++)
            {
                var vec = ((k * MathHelper.TwoPi / 15f).ToRotationVector2() * 6 * new Vector2(rand, 1)).RotatedBy(Rotation);
            }
            if (current != null)
            {
                current.timeLeft = (byte)MathHelper.Lerp(current.timeLeftMax, 0, Utils.GetLerpValue(0.25f, 1, Factor, true));
                current.xScaler = 2 + Owner.velocity.Length() / 32;
                current.scaler = StandardInfo.VertexStandard.scaler * ModifyData.Size * OffsetSize / 3 * 4f * current.xScaler;
                current.center = Owner.Center - Rotation.ToRotationVector2() * current.scaler * .5f;
                current.rotation = Rotation;
                current.negativeDir = Flip;
            }

            // 冲锋碰撞弹幕摧毁 + 恢复生命 + 连击
            EraseProjectilesAndHeal();

            base.OnAttack();
        }

        public override void OnStartSingle()
        {
            _noMovement = false;
            KValue = 1f;
            _wingTimeRestored = false;
            _comboAdded = false;

            if (Owner is Player plr)
            {
                var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (mplr.DashCoolDown > 0)
                {
                    Projectile.Kill();
                    return;
                }
                if (plr.whoAmI == Main.myPlayer)
                {
                    int windDamage = (int)(Projectile.damage * 0.2f);
                    if (windDamage < 1) windDamage = 1;
                    var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Main.MouseWorld, default, ModContent.ProjectileType<WindMill>(), windDamage, Projectile.knockBack, Projectile.owner);
                }
            }
            base.OnStartSingle();
        }

        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(SoundID.Item92, Owner.Center);
            originVelocity = Owner.velocity;

            if (windMill != null)
            {
                windMill.Projectile.ai[0] = 10;
                windMill.Projectile.netUpdate = true;
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, windMill.Projectile.whoAmI);
            }

            if (Owner is Player plr && plr.whoAmI == Main.myPlayer)
            {
                var mplr2 = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();

                _noMovement = plr.controlDown;

                if (!_noMovement)
                {
                    var adder = targetedVector.SafeNormalize(default) * (1200 / TimerMax + 45);
                    Owner.velocity += adder;
                    var scaler = ((Main.MouseWorld - plr.Center).Length()) / 1440 + 1 / 6f - .1f;
                    Owner.velocity *= scaler;
                    plr.fallStart = (int)(plr.position.Y / 16f);
                    if (Main.netMode is NetmodeID.MultiplayerClient)
                        VelocitySync.Get(plr.whoAmI, plr.velocity).Send();
                }

                mplr2.NextHitImmune = true;
                mplr2.Dashing = true;

                // ★ 设置全分组无敌
                foreach (int cooldownID in ImmunityHelper.GetAllImmunityCooldownIDs())
                {
                    plr.AddImmuneTime(cooldownID, TimerMax + 5);
                }
                // 保留通用免疫（可选）
                plr.immune = true;
                plr.immuneTime = TimerMax + 5;

                var maxT = 60;
                if (mplr2.itemDefinition != null && mplr2.itemDefinition.Type > 0)
                    maxT = new Item(mplr2.itemDefinition.Type).useAnimation;
                mplr2.DashCoolDown = maxT < 30 ? 6 * maxT + 300 : 12 * maxT + 120;
                mplr2.DashCoolDownMax = mplr2.DashCoolDown;
                mplr2.startPoint = plr.Center;
                mplr2.SendSyncAI();
            }
        }

        public void HandleHit(Entity victim, int damageDone)
        {
            var player = Owner as Player;
            if (player == null) return;
            var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();

            if (DonQuijoteProgression.Tier7_HitRestoreWing && !_wingTimeRestored && player.wingTimeMax > 0)
            {
                player.wingTime += 1f;
                if (player.wingTime > player.wingTimeMax)
                    player.wingTime = player.wingTimeMax;
                _wingTimeRestored = true;
            }

            if (!_comboAdded && mplr.TryAddCombo())
                _comboAdded = true;
        }

        private void EraseProjectilesAndHeal()
        {
            if (!DonQuijoteProgression.Tier11_DashEraseHealCombo)
                return;

            Player player = Owner as Player;
            if (player == null) return;
            var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (!p.active || p.friendly) continue;
                if (!p.Hitbox.Intersects(player.Hitbox)) continue;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    WindMillEraseSync.Get(i).Send();
                    p.Kill();
                }
                else
                {
                    WindMill.KillProjectile(p);
                    player.Heal(5);
                }
            }
        }

        private UltraStab current;
    }

    private class DonQuijoteDeLaManchaStab : RapidlyStabInfo
    {
        private int hitCount = 0;
        private bool _wingTimeRestored;
        private bool _comboAdded;

        public override void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
            base.OnHitEntity(victim, damageDone, context);
            if (Owner is Player player)
            {
                damageDone /= Math.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue);
                var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (victim is NPC npc && npc.CanBeChasedBy())
                {
                    hitCount++;
                    if (DonQuijoteProgression.Tier6_StabReduceCooldown)
                    {
                        mplr.DashCoolDown -= hitCount switch
                        {
                            1 => 10,
                            2 => 5,
                            3 => 3,
                            4 => 1,
                            5 => 1,
                            _ => 0
                        };
                        if (mplr.DashCoolDown < 0)
                            mplr.DashCoolDown = 0;
                    }
                }
            }
            float delta = Main.rand.NextFloat(0.85f, 1.15f) * damageDone;
            for (int n = 0; n < 30 * delta * (StandardInfo.dustAmount + .2f); n++)
                MiscMethods.FastDust(victim.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), StandardInfo.standardColor);

            if (Owner is Player player2 && DonQuijoteProgression.Tier7_HitRestoreWing && !_wingTimeRestored && player2.wingTimeMax > 0)
            {
                player2.wingTime += 1f;
                if (player2.wingTime > player2.wingTimeMax)
                    player2.wingTime = player2.wingTimeMax;
                _wingTimeRestored = true;
            }

            if (Owner is Player player3 && !_comboAdded)
            {
                var mplr = player3.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (mplr.TryAddCombo())
                    _comboAdded = true;
            }

            HandleHit(victim, damageDone);
        }

        public override void OnEndSingle()
        {
            if (Projectile.owner != Main.myPlayer) return;
            if (Owner is Player player && player.controlUp && DonQuijoteProgression.Tier4_StabDRAndMove)
            {
                Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(default);
                if (mouseDir != Vector2.Zero)
                {
                    player.velocity += mouseDir * 9;
                    if (player.velocity.Length() > 12)
                        player.velocity = player.velocity.SafeNormalize(default) * 12;
                    player.fallStart = (int)(player.position.Y / 16f);
                    if (Main.netMode is NetmodeID.MultiplayerClient)
                        VelocitySync.Get(player.whoAmI, player.velocity).Send();
                }
            }
            base.OnEndSingle();
        }

        public override void OnStartSingle()
        {
            hitCount = 0;
            _wingTimeRestored = false;
            _comboAdded = false;
            base.OnStartSingle();
        }

        public override void UpdateStatus(bool triggered)
        {
            base.UpdateStatus(triggered);
            if (Owner is Player plr && DonQuijoteProgression.Tier4_StabDRAndMove)
            {
                var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                mplr.pendingEndurance = 5;
            }
        }

        public override void OnEndAttack()
        {
            SoundEngine.PlaySound(SoundID.Item96 with { Volume = 0.5f * SoundID.Item96.Volume }, Owner.Center);
            base.OnEndAttack();
        }

        private void HandleHit(Entity victim, int damageDone) { }
    }

    private class Throwing : MeleeAction
    {
        public override void UpdateStatus(bool triggered)
        {
            if (Factor > .8f)
            {
                Owner.direction = MathF.Abs(Rotation) < MathHelper.PiOver2 ? 1 : -1;
                Flip = Owner.direction == -1;
            }

            int point = TimerMax * 5 / 6;
            if (Timer == point && !triggered)
            {
                _realPosition = (CompositeArmRotation + MathHelper.PiOver2).ToRotationVector2() * 16f + Owner.Center;
                // SoundEngine.PlaySound(SoundID.Zombie104);
                SoundEngine.PlaySound(SoundID.DD2_FlameburstTowerShot with { Volume = .5f }, Owner.Center);
                Owner.velocity += new Vector2(0, -8f);
                if (!Main.dedServ)
                {
                    _stab = UltraStab.NewUltraStabOnDefaultCanvas(15, 1f, _realPosition);
                    _stab.ApplyStdValueToVtxEffect(StandardInfo);
                }
            }
            if (Timer != point || !triggered)
            {
                Timer--;
            }

            if (Owner is Player plr)
            {
                plr.itemTime = 2;
                plr.itemAnimation = 2;
                plr.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, CompositeArmRotation);
                if (Factor > .83f)
                {
                    Rotation = (Main.MouseWorld - Owner.Center).ToRotation();
                    float factor = Utils.GetLerpValue(1, .83f, Factor);
                    plr.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi / 3f * -plr.direction * MathHelper.SmoothStep(0, 1, factor));
                }
                else
                {
                    float factor = Utils.GetLerpValue(.83f, .8f, Factor, true);
                    plr.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.Pi / 3f * -plr.direction * MathHelper.SmoothStep(1, -1.5f, factor));

                    if (Factor > .8f)
                    {
                        float factor2 = Utils.GetLerpValue(.83f, .8f, Factor, true);
                        var rotation = (MathHelper.Lerp(-0.05f, .65f, factor2) + .5f) % 1 - .5f;

                        plr.GetModPlayer<TheManInTheHighCastlePlayer>().Rotation = rotation * MathHelper.TwoPi * Owner.direction;
                        plr.GetModPlayer<TheManInTheHighCastlePlayer>().UseRotation = true;
                    }
                    if (Factor > 0.6f)
                        UpdateRealPosition_ThrowingProcess();
                    else
                        UpdateRealPosition_ReturningProcess();
                }
            }

            if (!_tileCollided && Factor < .83f && Factor > .6f)
            {
                Vector2 dir = (Rotation + OffsetRotation + MathHelper.PiOver2).ToRotationVector2();
                Vector2 ndir = new Vector2(-dir.Y, dir.X);
                for (int n = 0; n < 4; n++)
                {
                    Dust.NewDustPerfect(
                        _realPosition + ndir * n * 16 + dir * 4 * n,
                        DustID.TheDestroyer,
                        dir * 4).noGravity = true;

                    Dust.NewDustPerfect(
                        _realPosition + ndir * n * 16 - dir * 4 * n,
                        DustID.TheDestroyer,
                        dir * -4).noGravity = true;
                }

            }
        }
        public override void OnActive()
        {
            _tileCollided = false;
            _stab = null;
            _swoosh = null;
            base.OnActive();
        }
        public override void OnAttack()
        {
            base.OnAttack();
        }
        public override void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            base.Draw(spriteBatch, texture);
        }
        public override CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
        {
            return base.GetWeaponVertex(texture, alpha);
        }
        public override bool Attacktive => Factor < 0.83f && (!_tileCollided || Factor is < 0.3f and > 0.1f);
        public override float CompositeArmRotation
        {
            get
            {
                if (Factor < 0.3f)
                {
                    return MathHelper.Lerp(field, _targetRotation - MathHelper.PiOver2, 0.9f);
                }
                float offset;
                if (Factor > 0.83f)
                {
                    float factor = Utils.GetLerpValue(1, .83f, Factor);
                    offset = MathHelper.SmoothStep(0, -2 * Owner.direction, factor * factor);
                }
                else
                {
                    float factor = Utils.GetLerpValue(.83f, .8f, Factor, true);
                    offset = MathHelper.SmoothStep(-2 * Owner.direction, 0, factor * factor);
                }
                field = offset + Rotation - MathHelper.PiOver2;
                return field;
            }
        }

        private Vector2 _realPosition;
        private bool _tileCollided;
        private float _returningDirection = 0;
        private float _targetRotation;
        private static readonly float[] distanceSampler = new float[3];

        private UltraStab _stab;
        private UltraSwoosh _swoosh;
        private float _oldRotation;
        private void UpdateRealPosition_ThrowingProcess()
        {
            if (_tileCollided) return;



            const float x0 = 0.03f / 0.83f;
            const float y0 = x0 / (2.0f - x0);

            const float k0 = 2 * y0 / x0;
            const float p0 = k0 * .5f / x0;

            float factor = Utils.GetLerpValue(.83f, 0, Factor, true);
            float factor2 = factor < x0 ? p0 * factor * factor : k0 * factor - y0;

            float rfactor = Utils.GetLerpValue(.83f, 0, (fTimer + 1f) / TimerMax, true);
            float rfactor2 = rfactor < x0 ? p0 * rfactor * rfactor : k0 * rfactor - y0;

            float distanceMax = (factor2 - rfactor2) * 12000f;
            Vector2 unit = (Rotation + OffsetRotation).ToRotationVector2();
            Vector2 prevPos = _realPosition;
            Collision.LaserScan(prevPos, unit, 2, distanceMax, distanceSampler);
            float distance = (distanceSampler[0] + distanceSampler[1] + distanceSampler[2]) * .3333f;
            _realPosition = prevPos + unit * distance;
            if (distance / distanceMax < 0.98f)
            {
                _tileCollided = true;
                SoundEngine.PlaySound(SoundID.DD2_BetsysWrathImpact with { Volume = 2 }, _realPosition);
                var delta = _realPosition - prevPos;
                var uvelocity = unit * 100;
                for (int n = 0; n < 10; n++)
                {
                    Collision.HitTiles(_realPosition, delta, 4, 4);
                    for (int u = 0; u < 4; u++)
                        Dust.NewDustPerfect(
                            _realPosition,
                            DustID.TheDestroyer,
                            -uvelocity * (.25f + u * .2f) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(5 - u),
                            0,
                            default,
                            Main.rand.NextFloat(2)).noGravity = true;
                }
            }
            Vector2 dir = new Vector2(-unit.Y, unit.X);
            Vector2 ndir = unit;

            Vector2 v1 = ndir * distance * .5f;
            for (int n = 0; n < 2; n++)
            {
                Dust.NewDustPerfect(
                    _realPosition + (-v1 + dir * 4) * n,
                    DustID.TheDestroyer,
                    dir * 4, Scale: 0.5f).noGravity = true;

                Dust.NewDustPerfect(
                    _realPosition + (-v1 - dir * 4) * n,
                    DustID.TheDestroyer,
                    dir * -4, Scale: 0.5f).noGravity = true;

                Dust.NewDustPerfect(
                    _realPosition,
                    DustID.TheDestroyer,
                    -ndir * 4 * (n + 2), Scale: 0.5f).noGravity = true;
            }

            if (_stab is { } stab)
            {
                stab.timeLeft = 15;
                stab.center = _realPosition - unit * 240;
                stab.scaler = 600;
                stab.rotation = Rotation + OffsetRotation;
                stab.xScaler = 8;
            }
        }
        private void UpdateRealPosition_ReturningProcess()
        {
            if (Factor < 0.3f)
            {
                float factor = Utils.GetLerpValue(0.3f, 0.1f, Factor, true);
                _realPosition = Vector2.Lerp(_realPosition, Owner.Center, MathF.Pow(factor, 3));

                var target = Owner.Center - _realPosition;
                if (target != Vector2.Zero)
                    _targetRotation = MathHelper.Lerp(_targetRotation, target.ToRotation(), 0.2f);

                if (_returningDirection == 0)
                    _returningDirection = Math.Sign((Owner.Center - _realPosition).X);

                // if (factor < 0.7f)
                _oldRotation = Rotation;
                Rotation = MathHelper.Lerp(Rotation, _targetRotation + MathHelper.TwoPi * 2 * _returningDirection, MathF.Pow(factor, 3));

                if (Timer == 0)
                {
                    Rotation %= MathHelper.TwoPi;
                    _oldRotation = Rotation;
                }
                if (!Main.dedServ)
                {
                    if (_swoosh == null)
                    {
                        _swoosh = UltraSwoosh.NewUltraSwooshOnDefaultCanvas(15, 128, _realPosition, (-_returningDirection, 0));
                        _swoosh.ApplyStdValueToVtxEffect(StandardInfo);
                    }
                    _swoosh.timeLeft = 15;
                    _swoosh.center = _realPosition;
                    _swoosh.rotation = Rotation;
                    _swoosh.angleRange = ((_oldRotation - Rotation) * 2, 0);
                }
            }
            else
            {
                float factor = Utils.GetLerpValue(0.6f, 0.3f, Factor, true);

                _realPosition -= (OffsetRotation + Rotation).ToRotationVector2() * 8 * (1 - factor) * factor;
            }
        }

        public override Vector2 OffsetCenter
        {
            get
            {
                if (Factor > 0.83f)
                {
                    return (CompositeArmRotation + MathHelper.PiOver2).ToRotationVector2() * 16f;
                }
                else if (Factor > .1f)
                {
                    return _realPosition - Owner.Center;
                }
                else return default;
            }
        }
        public override float OffsetRotation
        {
            get
            {
                if (Factor > 0.83f)
                {
                    float factor = Utils.GetLerpValue(1, .83f, Factor);
                    return MathHelper.SmoothStep(MathHelper.PiOver2, MathHelper.Pi / 12f, factor * factor) * -Owner.direction;
                }
                else
                {
                    return base.OffsetRotation;
                }
            }
        }
        public override Vector2 OffsetOrigin => new Vector2(0.2f, -0.2f);
        public override bool OwnerHitCheek => false;

        public override void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
            base.OnHitEntity(victim, damageDone, context);
        }
        public override bool Collide(Rectangle rectangle)
        {
            switch (Factor)
            {
                case < .83f and > 0.6f:
                    float point = 0;
                    Vector2 unit = (OffsetRotation + Rotation).ToRotationVector2();
                    return Collision.CheckAABBvLineCollision(
                        rectangle.TopLeft(),
                        rectangle.Size(),
                        _realPosition + unit * 80,
                        _realPosition - unit * 80,
                        32,
                        ref point);
                case < 0.3f and > 0.1f:
                    return Vector2.Distance(_realPosition, rectangle.Center()) < 90 + rectangle.Size().Length() * .5f;
            }
            return false;
        }
    }
}
