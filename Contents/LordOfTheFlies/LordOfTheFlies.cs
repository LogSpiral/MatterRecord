using MatterRecord.Contents.Rarities;
using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;


namespace MatterRecord.Contents.LordOfTheFlies
{
    public class LordOfTheFlies : ModItem, IRecordBookItem
    {
        ItemRecords IRecordBookItem.RecordType => ItemRecords.LordOfTheFlies;

        /// <summary>
        /// 占位基础伤害值。
        /// <para>原版 <c>Item.Prefix(int)</c> 在应用前缀前会校验该前缀是否真实改变了数值：
        /// 若 <c>Math.Round(Item.damage * 伤害倍率) == Item.damage</c>，该前缀会被判为无效并重新掷骰。
        /// 基础伤害为 1 时，虚幻(1.15)、恶魔(1.15)、瞄准(1.1) 等一切带伤害倍率的前缀取整后都与原值相同，
        /// 因此永远洗不出来。这里改用足够大的占位值（21 可覆盖 0.7~1.18 的全部倍率），
        /// 真实的基础伤害在 <see cref="ModifyWeaponDamage"/> 中还原为 1。</para>
        /// </summary>
        private const float PlaceholderDamage = 21f;

        public override void SetDefaults()
        {
            // 占位值而非真实伤害：仅为让原版前缀校验通过（详见 PlaceholderDamage 说明）
            Item.damage = (int)PlaceholderDamage;
            Item.knockBack = 1f;
            Item.useTime = Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.value = Item.buyPrice(copper: 5);
            Item.rare = ModContent.RarityType<ImmutableQuest>();
            Item.holdStyle = ItemHoldStyleID.HoldHeavy;
            Item.noMelee = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
            base.SetDefaults();
        }

        public override void AddRecipes()
        {
            // 火枪 / 送葬者 已合并为同一合成组，因此只生成一条配方
            this.RegisterBookRecipe(RecipeGroupSystem.MusketGroup);
        }

        public override bool CanUseItem(Player player)
        {
            ItemID.Sets.gunProj[Item.type] = true;
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            if (player.controlUseTile)
                return true;
            if (player.controlUseItem)
            {
                if (mplr.IsInTrialMode)
                    return mplr.ChargingEnergy >= 3; // 审判模式左键可点击发射普通审判弹（长按蓄力湮灭弹在右键）
                else
                    return true;
            }
            return true;
        }

        /// <summary>
        /// 背包内右键装填不消耗物品本身（防止右键后蝇王消失，参考浮士德写法）。
        /// </summary>
        public override bool ConsumeItem(Player player) => false;

        /// <summary>
        /// 背包内右键装填湮灭弹的可用性：已解锁湮灭弹（项2进度锁）、源质满且存量未满（上限 6 发）时才允许背包内右键。
        /// 参考浮士德的背包内右键判定写法（CanRightClick + RightClick）。
        /// </summary>
        public override bool CanRightClick()
        {
            var mplr = Main.LocalPlayer.GetModPlayer<LordOfTheFliesPlayer>();
            return LordOfTheFliesProgression.Tier2_AnnihilationBullet && mplr.ChargingEnergy == 120 && mplr.StoredAmmoCount < 6;
        }

        /// <summary>
        /// 背包内右键装填湮灭弹：消耗全部源质（清零）装填 1 发，并触发 30 帧右键冷却防止连点。
        /// </summary>
        public override void RightClick(Player player)
        {
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            if (LordOfTheFliesProgression.Tier2_AnnihilationBullet && mplr.ChargingEnergy == 120 && mplr.StoredAmmoCount < 6)
            {
                mplr.StoredAmmoCount++;
                mplr.ChargingEnergy = 0;
                SoundEngine.PlaySound(SoundID.Item45, player.Center);
                mplr.RightCooldown = 30;
                if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                    mplr.SyncPlayer(-1, player.whoAmI, false);
            }
            base.RightClick(player);
        }

        private static SoundStyle SwitchModeSoundEffect { get; } = new SoundStyle("MatterRecord/Assets/Sounds/shotgun_reload_clip3");

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            ref int _chargeTimer = ref mplr.ChargeTimer;
            // 非审判模式且右键已松开时，清除「无备弹退出」防抖哨兵，允许下次右键重新切换进审判模式
            if (_chargeTimer >= 130 && !mplr.IsInTrialMode && !player.controlUseTile)
                _chargeTimer = 0;
            player.itemLocation -= player.itemRotation.ToRotationVector2() * 8 * player.direction;
            if (player.itemAnimation == 1)
            {
                if (player.altFunctionUse == 2 || mplr.IsChargingAnnihilation)
                {
                    if (mplr.IsInTrialMode)
                    {
                        // ==== 审判模式：短按右键切换退出 / 长按右键蓄力发射（无备弹时直接退出，不蓄力瞄准）====
                        if (player.controlUseTile && mplr.StoredAmmoCount > 0)
                        {
                            // 有备弹：长按右键蓄力发射（保持动画循环、计时递增，参考原蓄力机制区分长短按）
                            player.itemAnimation = 2;
                            _chargeTimer++;
                            if (_chargeTimer == 40)
                                SoundEngine.PlaySound(SoundID.MaxMana, player.Center);
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
                            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation * player.gravDir - (player.direction < 0 ? MathHelper.Pi : 0) - MathHelper.PiOver2);
                            // 同步蓄力状态给其他玩家
                            if (!mplr.IsChargingAnnihilation)
                            {
                                mplr.IsChargingAnnihilation = true;
                                if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
                                    mplr.SyncAnniCharging(-1, Main.myPlayer);
                            }
                        }
                        else
                        {
                            // 松开右键（有备弹蓄力完成则发射）；无备弹长按右键直接退出审判模式（不蓄力瞄准）
                            if (!player.controlUseTile && mplr.StoredAmmoCount > 0 && _chargeTimer >= 10)
                            {
                                // 有备弹长按蓄力完成且松开：发射（ManualShoot 内部按 ChargeTimer>=40 判定湮灭弹/普通审判弹）
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

                                    Vector2 center = player.RotatedRelativePoint(player.MountedCenter) + new Vector2(0, -6);
                                    ManualShoot(
                                        player,
                                        (EntitySource_ItemUse_WithAmmo)player.GetSource_ItemUse_WithPotentialAmmo(Item, usedAmmoItemId),
                                        center,
                                        (Main.MouseWorld - center).SafeNormalize(default) * speed,
                                        projToShoot,
                                        Damage,
                                        KnockBack
                                        );
                                }
                                _chargeTimer = 0;
                                if (mplr.IsChargingAnnihilation)
                                {
                                    mplr.IsChargingAnnihilation = false;
                                    if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
                                        mplr.SyncAnniCharging(-1, Main.myPlayer);
                                }
                            }
                            else
                            {
                                // 无备弹长按右键 / 短按松开右键：退出审判模式
                                mplr.IsInTrialMode = false;
                                if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                                    mplr.SyncPlayer(-1, player.whoAmI, false);
                                SoundEngine.PlaySound(SwitchModeSoundEffect, player.Center);
                                // 若右键仍按住（无备弹长按），置防抖哨兵避免下一帧又切回审判模式；松开右键由 UseStyle 开头清零
                                _chargeTimer = player.controlUseTile ? 130 : 0;
                                if (mplr.IsChargingAnnihilation)
                                {
                                    mplr.IsChargingAnnihilation = false;
                                    if (player.whoAmI == Main.myPlayer && Main.netMode == NetmodeID.MultiplayerClient)
                                        mplr.SyncAnniCharging(-1, Main.myPlayer);
                                }
                            }
                        }
                    }
                    else
                    {
                        // ==== 非审判模式：右键切换进审判模式（长按蓄力装填动作已删除，右键仅用于切换）====
                        // 防抖期（_chargeTimer>=130，无备弹刚退出）不切回；正常情况才切换进审判模式
                        if (_chargeTimer < 130)
                        {
                            mplr.IsInTrialMode = true;
                            if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == player.whoAmI)
                                mplr.SyncPlayer(-1, player.whoAmI, false);
                            SoundEngine.PlaySound(SwitchModeSoundEffect, player.Center);
                            var box = player.Hitbox;
                            CombatText.NewText(box, Color.Green, this.GetLocalizedValue("Clatter"));
                            _chargeTimer = 0;
                        }
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
                // 左键：非审判模式普通射击；审判模式点击发射普通审判弹（长按右键蓄力发射湮灭弹）
                if (player.itemTime == player.itemTimeMax - 1)
                {
                    // 立即发射
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

                        Vector2 center = player.RotatedRelativePoint(player.MountedCenter) + new Vector2(0, -6);
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
                else
                {
                    // 瞄准动作
                    if (player.whoAmI == Main.myPlayer)
                    {
                        Vector2 target = Main.MouseWorld - player.RotatedRelativePoint(player.MountedCenter);
                        float rotation = target.ToRotation();
                        player.direction = Math.Sign(target.X);
                        player.itemRotation = rotation;
                        if (player.direction < 0)
                            player.itemRotation += MathHelper.Pi;
                        if (player.itemTime % 4 == 0)
                        {
                            NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, Main.myPlayer);
                            NetMessage.SendData(MessageID.ShotAnimationAndSound, -1, -1, null, Main.myPlayer);
                        }
                    }
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.itemRotation * player.gravDir - (player.direction < 0 ? MathHelper.Pi : 0) - MathHelper.PiOver2);
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
                player.itemRotation += MathHelper.PiOver4 * player.direction * player.gravDir;
                player.itemLocation -= player.itemRotation.ToRotationVector2() * 8 * player.direction;
                Item.holdStyle = ItemHoldStyleID.HoldHeavy;
            }
            else
            {
                Item.holdStyle = ItemHoldStyleID.None;
            }
            base.HoldStyle(player, heldItemFrame);
        }

        private static void GetFirstTwoBullets(Player player, out int primaryType, out int secondaryType)
        {
            primaryType = ProjectileID.Bullet;
            secondaryType = 0;
            bool foundFirst = false;

            for (int i = 54; i <= 57 && i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item != null && !item.IsAir && item.ammo == AmmoID.Bullet)
                {
                    if (!foundFirst)
                    {
                        primaryType = item.shoot;
                        foundFirst = true;
                    }
                    else
                    {
                        secondaryType = item.shoot;
                        return;
                    }
                }
            }

            for (int i = 0; i < 54 && i < player.inventory.Length; i++)
            {
                Item item = player.inventory[i];
                if (item != null && !item.IsAir && item.ammo == AmmoID.Bullet)
                {
                    if (!foundFirst)
                    {
                        primaryType = item.shoot;
                        foundFirst = true;
                    }
                    else
                    {
                        secondaryType = item.shoot;
                        return;
                    }
                }
            }
        }

        private static void ManualShoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            GetFirstTwoBullets(player, out int primaryAmmoType, out int secondaryAmmoType);

            if (mplr.IsInTrialMode)
            {
                if (LordOfTheFliesProgression.Tier2_AnnihilationBullet && mplr.StoredAmmoCount > 0 && mplr.ChargeTimer >= 40)
                {
                    // ---- 湮灭弹 ----
                    if (!LordOfTheFliesProgression.Tier14_NoConsume || Main.rand.Next(6) != 0)
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
                    var proj = Projectile.NewProjectileDirect(source, position, velocity * 2, ModContent.ProjectileType<AnnihilationBullet>(), damage * 2, knockback, player.whoAmI);
                    proj.MaxUpdates *= 2;
                    var gProj = proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>();
                    gProj.IsFromTrialMode = true;
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
                    player.GetModPlayer<MatterRecordPlayer>().strengthOfShake += 5.0f;
                }
                else
                {
                    // ---- 审判模式普通子弹（强化） ----
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
                    // 穿透 2 + 无视护甲（项3 进度锁）
                    if (LordOfTheFliesProgression.Tier3_Penetration)
                    {
                        proj.penetrate = 2;
                        proj.ArmorPenetration = 40; // 护甲穿透 40
                    }
                    proj.usesIDStaticNPCImmunity = true;
                    proj.idStaticNPCHitCooldown = 4;
                    var gProj = proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>();
                    gProj.IsFromTrialMode = true;
                    // 融合子弹（项13 进度锁）
                    if (LordOfTheFliesProgression.Tier13_Fusion && secondaryAmmoType > 0 && secondaryAmmoType != ProjectileID.None && Main.rand.NextFloat() < 0.1f)
                        gProj.SecondaryAmmoType = secondaryAmmoType;
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
                }
            }
            else
            {
                // ---- 非审判模式 ----
                if (LordOfTheFliesProgression.Tier12_SourceFull && mplr.ChargingEnergy == 120) // 源质满，发射强化子弹（项12 进度锁）
                {
                    mplr.ChargingEnergy -= 5;
                    if (mplr.ChargingEnergy < 0) mplr.ChargingEnergy = 0;
                    mplr.SourceRecoveryPauseTimer = 30; // 使用源质强化后暂停源质恢复 30 帧

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
                    // 穿透 2 + 无视护甲（项3 进度锁）
                    if (LordOfTheFliesProgression.Tier3_Penetration)
                    {
                        proj.penetrate = 2;
                        proj.ArmorPenetration = 40;
                    }
                    proj.usesIDStaticNPCImmunity = true;
                    proj.idStaticNPCHitCooldown = 4;
                    var gProj = proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>();
                    gProj.IsFromTrialMode = true;
                    // 融合子弹（项13 进度锁）
                    if (LordOfTheFliesProgression.Tier13_Fusion && secondaryAmmoType > 0 && secondaryAmmoType != ProjectileID.None && Main.rand.NextFloat() < 0.1f)
                        gProj.SecondaryAmmoType = secondaryAmmoType;
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
                }
                else
                {
                    // 普通模式普通射击
                    var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
                    SoundEngine.PlaySound(SoundID.Item11, player.Center);
                    proj.usesIDStaticNPCImmunity = true;
                    proj.idStaticNPCHitCooldown = 4;
                }
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return false;
        }

       

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            // 反查当前前缀的伤害倍率（无前缀时 prefix 为 0，反查结果为 1）
            float prefixMult = 1f;
            if (Item.TryGetPrefixStatMultipliersForItem(Item.prefix, out float dmg, out _, out _, out _, out _, out _, out _))
                prefixMult = dmg;

            // 把占位基础值还原为设计值 1，同时保留前缀倍率。
            // StatModifier.ApplyTo 的公式为 (baseValue + Base) * Additive * Multiplicative + Flat，
            // 因此只要令 Item.damage + Base == prefixMult，最终基础伤害就等于「1 × 前缀倍率」：
            // 既不会因占位值放大伤害，也让前缀的 +15% 等加成真实生效。
            damage.Base = prefixMult - Item.damage;

            float rangeFactor = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
            float genericFactor = player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);
            rangeFactor -= genericFactor;
            genericFactor += rangeFactor - 1;

            var critFactor = player.GetTotalCritChance(DamageClass.Ranged) * .01f;
            critFactor += .04f;

            int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;

            // 蝇王强化1：命中叠层提供的防御视为护甲防御，一并计入伤害公式
            defense += player.GetModPlayer<LordOfTheFliesPlayer>().GetDefenseBonus();

            // 平衡补偿：护甲部分额外 ÷1.15，抵消强化1满层(5层×3%=15%)带来的护甲提升，
            // 使玩家只获得防御容错而不会因护甲堆叠被增强输出
            damage.Multiplicative *= Math.Max((defense / 1.15f) * (0.75f + rangeFactor + critFactor) / (1 + genericFactor * .5f), 1);
        }

        public override float UseTimeMultiplier(Player player)
        {
            // 攻速随护甲提升（项6 进度锁）
            if (!LordOfTheFliesProgression.Tier6_AttackSpeed)
                return 1f;
            float rangeMultiplier = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
            float rangeBonus = rangeMultiplier - 1f;
            float targetFrames = 20f / (1f + rangeBonus * 1.5f);
            targetFrames = Math.Max(6f, Math.Min(20f, targetFrames));
            float baseFrames = 15f;
            return targetFrames / baseFrames;
        }

        public override void UpdateInventory(Player player)
        {
            Item.useAmmo = this.IsRecordUnlocked ? AmmoID.Bullet : AmmoID.None;

            if (Main.myPlayer != player.whoAmI)
                return;

            // 别西卜协助作战（项4 进度锁）：未解锁则不生成
            if (!LordOfTheFliesProgression.Tier4_Beelzebub)
                return;

            int summonType = ModContent.ProjectileType<BeelzebubSummon>();

            // ---- 不手持蝇王：如果没有别西卜则生成 ----
            if (player.HeldItem.type != Type)
            {
                bool hasSummon = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == player.whoAmI && p.type == summonType)
                    {
                        hasSummon = true;
                        break;
                    }
                }
                if (!hasSummon)
                {
                    int proj = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item, null),
                        player.MountedCenter,
                        Vector2.Zero,
                        summonType,
                        // 占位 Item.damage 仅供前缀校验使用，不可外传；
                        // 别西卜的伤害由 BeelzebubSummon.ComputeWeaponDamage 自行计算，此处固定传 1
                        1,
                        Item.knockBack,
                        player.whoAmI
                    );
                    if (proj >= 0 && proj < Main.maxProjectiles)
                    {
                        NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
                    }
                }
            }
            // ★ 手持蝇王时：不做任何操作，由 BeelzebubSummon 自身的 AI 检测并移除
        }



        public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
        {
            if (!this.IsRecordUnlocked)
                return true;

            bool shiftHeld = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);

            var extraLines = new List<TooltipLine>();

            if (!shiftHeld)
            {
                // 未按住 Shift：只添加提示行，不显示强化列表
                extraLines.Add(new TooltipLine(Mod, "ShiftHint", this.GetLocalizedValue("ShiftHint"))
                {
                    OverrideColor = Color.Gray
                });
                MiscMethods.DrawTagTooltips(lines, extraLines, x, y);
                return true;
            }

            // --- 按住 Shift：显示强化进度 ---

            // Boss → 面具 ID（组合 Boss 单独处理）
            var bossIconMap = new Dictionary<string, int>
    {
        { "史莱姆王", 2493 },
        { "克眼", 2112 },
        { "蜂后", 2108 },
        { "骷髅王", 1281 },
        { "肉山", 2105 },
        { "史莱姆皇后", 4959 },
        { "世花", 2109 },
        { "石巨人", 2110 },
        { "猪鲨", 2588 },
        { "光女", 4784 },
        { "教徒", 3372 },
        { "月总", 3373 },
    };

            var progression = new (string name, bool unlocked, string bossName)[]
            {
        ("Tier1_DefenseOnHit", LordOfTheFliesProgression.Tier1_DefenseOnHit, "史莱姆王"),
        ("Tier2_AnnihilationBullet", LordOfTheFliesProgression.Tier2_AnnihilationBullet, "克眼"),
        ("Tier3_Penetration", LordOfTheFliesProgression.Tier3_Penetration, "世界吞噬者/克苏鲁之脑"),
        ("Tier4_Beelzebub", LordOfTheFliesProgression.Tier4_Beelzebub, "蜂后"),
        ("Tier5_CritLifePercent", LordOfTheFliesProgression.Tier5_CritLifePercent, "骷髅王"),
        ("Tier6_AttackSpeed", LordOfTheFliesProgression.Tier6_AttackSpeed, "肉山"),
        ("Tier7_Teleport", LordOfTheFliesProgression.Tier7_Teleport, "史莱姆皇后"),
        ("Tier8_JudgmentBlade", LordOfTheFliesProgression.Tier8_JudgmentBlade, "毁灭者/机械骷髅王/双子魔眼"),
        ("Tier9_AnnihilationLifePercent", LordOfTheFliesProgression.Tier9_AnnihilationLifePercent, "世花"),
        ("Tier10_Explosion", LordOfTheFliesProgression.Tier10_Explosion, "石巨人"),
        ("Tier11_Homing", LordOfTheFliesProgression.Tier11_Homing, "猪鲨"),
        ("Tier12_SourceFull", LordOfTheFliesProgression.Tier12_SourceFull, "光女"),
        ("Tier13_Fusion", LordOfTheFliesProgression.Tier13_Fusion, "教徒"),
        ("Tier14_NoConsume", LordOfTheFliesProgression.Tier14_NoConsume, "月总"),
            };

            int unlockedCount = 0;
            foreach (var item in progression)
                if (item.unlocked) unlockedCount++;

            // 标题
            extraLines.Add(new TooltipLine(Mod, "ProgressionTitle", this.GetLocalization("ProgressionTitle").Format(unlockedCount))
            {
                OverrideColor = Color.White
            });

            // 每一项
            for (int i = 0; i < progression.Length; i++)
            {
                var (nameKey, unlocked, bossName) = progression[i];
                string iconStr = "";

                if (bossName == "世界吞噬者/克苏鲁之脑")
                {
                    iconStr = "[i:2111] [i:2104] ";
                }
                else if (bossName == "毁灭者/机械骷髅王/双子魔眼")
                {
                    iconStr = "[i:2113] [i:2107] [i:2106] ";
                }
                else if (bossIconMap.TryGetValue(bossName, out int itemId))
                {
                    iconStr = $"[i:{itemId}] ";
                }

                string displayText = iconStr + this.GetLocalizedValue(nameKey);
                extraLines.Add(new TooltipLine(Mod, "Progression" + (i + 1), displayText)
                {
                    OverrideColor = unlocked ? Color.Yellow : Color.Gray
                });
            }

            MiscMethods.DrawTagTooltips(lines, extraLines, x, y);
            return true;
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
            var mplr = player.GetModPlayer<LordOfTheFliesPlayer>();
            // 蓄力发射已改为审判模式下长按右键（controlUseTile），蓄力特效仅在审判模式长按右键时绘制
            if (!mplr.IsInTrialMode || !player.controlUseTile) return;
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
            center -= (rotation + MathHelper.PiOver2 * player.direction).ToRotationVector2() * 6 * player.gravDir;
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
                    Main.MouseWorld - Main.screenPosition,
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
                        Main.MouseWorld - Main.screenPosition,
                        null,
                        color * .25f * factor3,
                        Main.GlobalTimeWrappedHourly * 4,
                        new Vector2(32),
                        1f + 3 * factor2, 0, 0));
            }
        }
    }
}
