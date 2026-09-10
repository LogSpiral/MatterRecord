using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

public class DonQuijoteDeLaManchaPlayer : ModPlayer
{
    // ---- 状态字段 ----
    public bool NextHitImmune;
    public bool Dashing;
    public int DashCoolDown;
    public int DashCoolDownMax;
    public int StabTimeLeft;
    public Vector2 startPoint;
    public ItemDefinition itemDefinition = new();
    public int pendingEndurance;
    public int WindmillCount = 0;
    public int TauntTimer = 0;
    public bool HoldingDonQuijote = false;

    // ---- 连击系统 ----
    public int ComboCount = 0;
    public int ComboTimer = 0;        // 最大300帧（5秒）
    public int ComboCooldown = 0;

    // ---- 复活系统 ----
    public int ReviveCooldownTimer = 0;

    public float ComboSizeMultiplier => Math.Min(2f, 1f + ComboCount * 0.1f);

    // ---- UI 纹理 ----
    private static Texture2D _numberTex;
    private static Texture2D _hitTex;

    // ---- 赌博机滚动动画 ----
    private int _comboDisplayValue = 0;
    private int _comboOldDisplayValue = 0;
    private int _comboAnimTimer = 0;
    private const int ComboAnimDuration = 15;
    private bool _comboAnimating = false;

    // ---- 跳动动画 ----
    private int _comboBounceTimer = 0;
    private const int ComboBounceDuration = 10;   // 10帧

    // ---- Debuff 基准时间 ----
    private int[] _lastBuffTime;

    // ===== 以下为修改后的绘制常量 =====
    // 原：digitWidth=5, digitHeight=7, hitWidth=16, hitHeight=7
    // 新图尺寸：数字216x30（每数字21宽），Hit 66x30
    private const int digitWidth = 22;      // 216 / 10 ≈ 21.6，取整21，剩余6px留白
    private const int digitHeight = 30;
    private const int hitWidth = 66;
    private const int hitHeight = 30;
    // ==================================

    // ---- 核心方法 ----
    public bool TryAddCombo()
    {
        // 连击只由 owner（本地玩家）累加：服务器/其它端弹幕命中不自加，避免多人双份计数；
        // 非 owner 端作为接收方，通过 AISync 同步包跟随 owner 的连击值
        if (Player.whoAmI != Main.myPlayer) return false;

        // 进度锁5：未解锁则无法获得连击
        if (!DonQuijoteProgression.Tier5_ComboSystem) return false;

        ComboTimer = 300;

        if (_comboBounceTimer == 0)
            _comboBounceTimer = ComboBounceDuration;

        if (ComboCooldown > 0)
            return false;

        int maxCombo = DonQuijoteProgression.Tier9_ComboDecayAndCap20 ? 20 : 10;
        if (ComboCount >= maxCombo)
            return false;

        ComboCount++;
        ComboCooldown = 90;

        _comboOldDisplayValue = _comboDisplayValue;
        _comboDisplayValue = ComboCount;
        _comboAnimTimer = ComboAnimDuration;
        _comboAnimating = true;

        // 连击变化即上报，保证服务器/队友端的挥砍尺寸判定与本端一致
        SendSyncAI();

        return true;
    }

    // ---- 格挡逻辑 ----
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Player.HeldItem.type == ModContent.ItemType<DonQuijoteDeLaMancha>())
        {
            if (itemDefinition.Type > 0)
            {
                var item = new Item(itemDefinition.Type);
                int baseDamage = item.damage;
                if (baseDamage > 0 && DonQuijoteProgression.Tier1_Block)
                {
                    float blockChance = 0.1f;
                    if (DonQuijoteProgression.Tier14_BlockBoostWithWindmill && WindmillCount > 0)
                        blockChance += 0.5f;

                    if (Main.rand.NextFloat() < blockChance)
                    {
                        float reduction = baseDamage * 0.35f;
                        modifiers.FinalDamage.Flat -= reduction;
                    }
                }
            }
        }
    }

    public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
    {
        if (Player.HeldItem.type == ModContent.ItemType<DonQuijoteDeLaMancha>())
        {
            if (itemDefinition.Type > 0)
            {
                var item = new Item(itemDefinition.Type);
                int baseDamage = item.damage;
                if (baseDamage > 0 && DonQuijoteProgression.Tier1_Block)
                {
                    float blockChance = 0.1f;
                    if (DonQuijoteProgression.Tier14_BlockBoostWithWindmill && WindmillCount > 0)
                        blockChance += 0.3f;

                    if (Main.rand.NextFloat() < blockChance)
                    {
                        float reduction = baseDamage * 0.20f;
                        modifiers.FinalDamage.Flat -= reduction;
                    }
                }
            }
        }
    }

    // ---- 复活逻辑 ----
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
    {
        if (DonQuijoteProgression.Tier13_Revive && ComboCount > 15 && ReviveCooldownTimer <= 0)
        {
            int healAmount = (int)(ComboCount * 0.05f * Player.statLifeMax2);
            Player.statLife += healAmount;
            if (Player.statLife > Player.statLifeMax2)
                Player.statLife = Player.statLifeMax2;

            ComboTimer = 300;
            ReviveCooldownTimer = 36000;

            CombatText.NewText(Player.Hitbox, Color.Cyan, Language.GetTextValue("Mods.MatterRecord.Items.DonQuijoteDeLaMancha.ReviveText", healAmount));
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
            for (int i = 0; i < 30; i++)
                Dust.NewDust(Player.Center, 10, 10, DustID.Shadowflame, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f));

            return false;
        }
        else
        {
            ReviveCooldownTimer = 0;
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }
    }

    // ---- 每帧重置 ----
    public override void ResetEffects()
    {
        // 连击超时衰减（只有在连击系统解锁时才有效）
        if (DonQuijoteProgression.Tier5_ComboSystem)
        {
            // 计时/衰减只在 owner（本地玩家）端运行：其它端连击值由 AISync 同步包驱动，不做本地衰减
            //（否则队友/服务器端会自行把同步来的连击值衰减/清零，造成挥砍尺寸不同步）
            if (Player.whoAmI == Main.myPlayer)
            {
                if (ComboTimer > 0)
                {
                    ComboTimer--;
                    if (ComboTimer == 0)
                    {
                        int newCount;
                        if (DonQuijoteProgression.Tier9_ComboDecayAndCap20)
                            newCount = Math.Max(0, ComboCount - 5);
                        else
                            newCount = 0;

                        if (newCount != ComboCount)
                        {
                            _comboOldDisplayValue = _comboDisplayValue;
                            _comboDisplayValue = newCount;
                            _comboAnimTimer = ComboAnimDuration;
                            _comboAnimating = true;
                            if (_comboBounceTimer == 0)
                                _comboBounceTimer = ComboBounceDuration;
                            ComboCount = newCount;
                            if (ComboCount > 0)
                                ComboTimer = 300;
                        }
                        else
                        {
                            ComboCount = newCount;
                        }

                        // 连击衰减后广播新值，让服务器/队友端的挥砍尺寸同步回退
                        SendSyncAI();
                    }
                }

                if (ComboCooldown > 0)
                    ComboCooldown--;
            }
        }
        else
        {
            // 如果连击系统未解锁，强制清零（所有端一致，本就不应有连击）
            ComboCount = 0;
            ComboTimer = 0;
            ComboCooldown = 0;
            _comboDisplayValue = 0;
            _comboAnimating = false;
        }

        if (ReviveCooldownTimer > 0)
            ReviveCooldownTimer--;

        if (StabTimeLeft > 0)
            StabTimeLeft--;

        if (pendingEndurance > 0)
        {
            Player.endurance += 0.2f;
            pendingEndurance--;
        }

        if (DashCoolDown > 0)
            DashCoolDown--;

        if (Dashing)
            Player.noKnockback = true;

        if (TauntTimer > 0)
        {
            TauntTimer--;
            if (WindmillCount > 0)
                Player.aggro += 1000;
        }

        // ---- 连击增益（阈值奖励） ----
        if (DonQuijoteProgression.Tier5_ComboSystem)
        {
            // 5级：Debuff缩短在PreUpdateBuffs中
            // 10级：暴击率增加 连击数 * 1%
            if (ComboCount >= 10)
                Player.GetCritChance(DamageClass.Melee) += ComboCount * 1f;

            // 15级：生命恢复修正（在UpdateLifeRegen中）
            // 20级：移速增伤 + 加速度提升（在ModifyWeaponDamage中计算增伤，这里提升加速度）
            if (ComboCount >= 20)
            {
                Player.runAcceleration += 0.1f;
                Player.maxRunSpeed += 0.75f;
            }

            // 每连击提供 1% 攻速（平滑）
            Player.GetAttackSpeed(DamageClass.Melee) += ComboCount * 0.01f;
        }

        // 动画递减
        if (_comboAnimTimer > 0)
        {
            _comboAnimTimer--;
            if (_comboAnimTimer == 0)
                _comboAnimating = false;
        }

        if (_comboBounceTimer > 0)
            _comboBounceTimer--;

        HoldingDonQuijote = false;
        base.ResetEffects();
    }

    // ---- 减少 Debuff 持续时间（连击 ≥5 时生效，跳过旗帜） ----
    public override void PreUpdateBuffs()
    {
        base.PreUpdateBuffs();

        if (!DonQuijoteProgression.Tier5_ComboSystem || ComboCount < 5)
            return;

        if (_lastBuffTime == null || _lastBuffTime.Length != Player.buffType.Length)
            _lastBuffTime = new int[Player.buffType.Length];

        float reduction = ComboCount / 100f;
        for (int i = 0; i < Player.buffType.Length; i++)
        {
            int buffType = Player.buffType[i];

            // 旗帜（ID 147）是正面效果，但被错误标记为负面，直接跳过
            if (buffType == 147)
            {
                _lastBuffTime[i] = 0;
                continue;
            }

            if (buffType > 0 && Main.debuff[buffType])
            {
                int currentTime = Player.buffTime[i];
                if (currentTime == 0)
                {
                    _lastBuffTime[i] = 0;
                    continue;
                }

                if (currentTime > _lastBuffTime[i])
                {
                    int newTime = (int)(currentTime * (1f - reduction));
                    if (newTime < 1) newTime = 1;
                    Player.buffTime[i] = newTime;
                    _lastBuffTime[i] = newTime;
                }
                else
                {
                    _lastBuffTime[i] = currentTime;
                }
            }
            else
            {
                _lastBuffTime[i] = 0;
            }
        }
    }

    // ---- 生命回复修正：连击 ≥15 且 lifeRegen < 0 时，加上连击数，最高为0 ----
    public override void UpdateLifeRegen()
    {
        base.UpdateLifeRegen();

        if (DonQuijoteProgression.Tier5_ComboSystem && ComboCount >= 15 && Player.lifeRegen < 0)
        {
            Player.lifeRegen += ComboCount;
            if (Player.lifeRegen > 0)
                Player.lifeRegen = 0;
        }
    }

    // ---- 伤害加成：冲刺距离 + 移速增伤（连击20） ----
    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        // 冲刺伤害加成（原有）
        if (Dashing)
            damage *= (1 + 0.5f * MathF.Log((Player.Center - startPoint).Length() / 16f + 1));

        // ---- 连击20：根据移速增伤 ----
        if (DonQuijoteProgression.Tier5_ComboSystem && ComboCount >= 20)
        {
            float speed = Player.velocity.Length(); // 像素/嘀嗒
            float baseSpeed = 6f; // 基准速度（约步行速度）
            float speedBonus = 1f + Math.Min(0.3f, (speed / baseSpeed) * 0.1f); // 最高30%
            damage *= speedBonus;
        }

        base.ModifyWeaponDamage(item, ref damage);
    }

    // ---- 其他重写 ----
    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (NextHitImmune && Dashing)
        {
            NextHitImmune = false;
            Player.immune = true;
            return true;
        }
        return false;
    }

    public override void UpdateDead()
    {
        Dashing = false;
        base.UpdateDead();
    }

    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        if (DonQuijoteProgression.Tier3_TauntOnHit && WindmillCount > 0)
        {
            TauntTimer = 180;
            SendSyncAI();
        }
        base.OnHitByNPC(npc, hurtInfo);
    }

    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        if (DonQuijoteProgression.Tier3_TauntOnHit && WindmillCount > 0)
        {
            TauntTimer = 180;
            SendSyncAI();
        }

        if (Dashing)
        {
            DashCoolDown += 30;
            DashCoolDownMax += 30;
        }

        base.OnHitByProjectile(proj, hurtInfo);
    }

    // ---- 绘制 ----
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        // 冲刺冷却（原有）
        if (DashCoolDown > 0 && Main.myPlayer == Player.whoAmI && !Player.dead)
        {
            Vector2 cen = Player.Center + Player.gfxOffY * Vector2.UnitY - Main.screenPosition - new Vector2(16, Player.gravDir < 0 ? -128 : 160);
            var direction = Player.gravDir < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            drawInfo.DrawDataCache.Add(new DrawData(ModAsset.DashCooldown_Recover.Value, cen, null, Color.White, 0, new Vector2(), 1f, direction));

            drawInfo.DrawDataCache.Add(
                new DrawData(
                    ModAsset.DashCooldown.Value,
                    cen + (Player.gravDir < 0 ? Vector2.UnitY * (int)(32 - 32f * DashCoolDown / DashCoolDownMax) : Vector2.Zero),
                    new Rectangle(0, 0, 32, (int)(32f * DashCoolDown / DashCoolDownMax)),
                    Color.White,
                    0,
                    new Vector2(),
                    1f,
                    direction));
            string text = Language.GetTextValue("Mods.MatterRecord.Items.DonQuijoteDeLaMancha.DashCooldown") + $"{DashCoolDown / 60f:0.0}/{DashCoolDownMax / 60f:0.0}";
            var state = Main.graphics.GraphicsDevice.RasterizerState;
            Main.graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text, cen + new Vector2(16, Player.gravDir < 0 ? -16 : 48),
                Color.White,
                Color.Black,
                0,
                FontAssets.MouseText.Value.MeasureString(text) * .5f,
                new Vector2(1, Player.gravDir));
            Main.graphics.GraphicsDevice.RasterizerState = state;
        }

        // ---- 连击显示 UI（使用新尺寸常量） ----
        if (DonQuijoteProgression.Tier5_ComboSystem && Main.myPlayer == Player.whoAmI && _comboDisplayValue > 0 && _numberTex != null && _hitTex != null)
        {
            const float baseUIScale = 2f;

            float bounceScale = 1f;
            if (_comboBounceTimer > 0)
            {
                float bounceProgress = 1f - _comboBounceTimer / (float)ComboBounceDuration;
                bounceScale = 0.8f + 0.2f * bounceProgress;
            }
            float uiScale = baseUIScale * bounceScale;

            // 使用修改后的常量
            const int spacing = 2;
            const int padding = 2;

            int totalContentWidth = digitWidth + spacing + digitWidth + spacing + hitWidth;
            int totalHeight = Math.Max(digitHeight, hitHeight);
            int bgWidth = totalContentWidth + padding * 2;
            int bgHeight = totalHeight + padding * 2;

            float scaledBgWidth = bgWidth * uiScale;
            float scaledBgHeight = bgHeight * uiScale;
            float scaledPadding = padding * uiScale;
            float scaledSpacing = spacing * uiScale;
            float scaledDigitWidth = digitWidth * uiScale;

            Vector2 playerScreenPos = Player.Center - Main.screenPosition;
            Vector2 offset = new Vector2(-200, -150);
            Vector2 bgPos = playerScreenPos + offset - new Vector2(scaledBgWidth / 2, scaledBgHeight / 2);

            int GetNumberIndex(int digit)
            {
                if (digit == 0) return 9;
                return digit - 1;
            }

            void DrawSingleDigit(int digit, Vector2 position, float alpha, float verticalOffset, IList<DrawData> drawDataCache)
            {
                int index = GetNumberIndex(digit);
                Rectangle rect = new Rectangle(index * digitWidth, 0, digitWidth, digitHeight);
                Vector2 pos = new Vector2(position.X, position.Y + verticalOffset);
                drawDataCache.Add(new DrawData(
                    _numberTex,
                    pos,
                    rect,
                    Color.White * alpha,
                    0f,
                    Vector2.Zero,
                    uiScale,
                    SpriteEffects.None,
                    0f
                ));
            }

            int oldTens = _comboOldDisplayValue / 10;
            int oldOnes = _comboOldDisplayValue % 10;
            int newTens = _comboDisplayValue / 10;
            int newOnes = _comboDisplayValue % 10;

            bool tensChanged = oldTens != newTens;
            bool onesChanged = oldOnes != newOnes;

            float animProgress = 0f;
            if (_comboAnimating && _comboAnimTimer > 0)
                animProgress = 1f - _comboAnimTimer / (float)ComboAnimDuration;

            Vector2 tensPos = new Vector2(bgPos.X + scaledPadding, bgPos.Y + scaledPadding);

            if (_comboAnimating && tensChanged)
            {
                float oldAlpha = 1f - animProgress;
                float oldOffset = -animProgress * 10f;
                DrawSingleDigit(oldTens, tensPos, oldAlpha, oldOffset, drawInfo.DrawDataCache);

                float newAlpha = animProgress;
                float newOffset = (1f - animProgress) * 10f;
                DrawSingleDigit(newTens, tensPos, newAlpha, newOffset, drawInfo.DrawDataCache);
            }
            else
            {
                DrawSingleDigit(newTens, tensPos, 1f, 0f, drawInfo.DrawDataCache);
            }

            Vector2 onesPos = new Vector2(tensPos.X + scaledDigitWidth + scaledSpacing, tensPos.Y);

            if (_comboAnimating && onesChanged)
            {
                float oldAlpha = 1f - animProgress;
                float oldOffset = -animProgress * 10f;
                DrawSingleDigit(oldOnes, onesPos, oldAlpha, oldOffset, drawInfo.DrawDataCache);

                float newAlpha = animProgress;
                float newOffset = (1f - animProgress) * 10f;
                DrawSingleDigit(newOnes, onesPos, newAlpha, newOffset, drawInfo.DrawDataCache);
            }
            else
            {
                DrawSingleDigit(newOnes, onesPos, 1f, 0f, drawInfo.DrawDataCache);
            }

            Rectangle hitRect = new Rectangle(0, 0, hitWidth, hitHeight);
            Vector2 hitPos = new Vector2(
                onesPos.X + scaledDigitWidth + scaledSpacing,
                tensPos.Y
            );
            drawInfo.DrawDataCache.Add(new DrawData(
                _hitTex,
                hitPos,
                hitRect,
                Color.White,
                0f,
                Vector2.Zero,
                uiScale,
                SpriteEffects.None,
                0f
            ));

            float timeProgress = ComboTimer / 300f;
            int barHeight = 4;
            float barWidth = bgWidth * uiScale * timeProgress;
            float barHeightScaled = barHeight * uiScale;
            Vector2 barPos = new Vector2(bgPos.X, bgPos.Y + scaledBgHeight + 2);
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            drawInfo.DrawDataCache.Add(new DrawData(
                magicPixel,
                barPos,
                new Rectangle(0, 0, 1, 1),
                Color.Red,
                0f,
                Vector2.Zero,
                new Vector2(barWidth, barHeightScaled),
                SpriteEffects.None,
                0f
            ));
        }

        base.ModifyDrawInfo(ref drawInfo);
    }

    // ---- 网络同步 ----
    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        DonQuijoteDeLaManchaItemDefinitionSync.Get(Player.whoAmI, itemDefinition.ToString()).Send(toWho, fromWho);
    }

    public void ReceivePlayerSyncItemDefinition(BinaryReader reader)
    {
        itemDefinition = new ItemDefinition(reader.ReadString());
    }

    public void ReceivePlayerSyncAI(BinaryReader reader)
    {
        DashCoolDown = reader.ReadUInt16();
        DashCoolDownMax = reader.ReadUInt16();
        Dashing = reader.ReadBoolean();
        NextHitImmune = reader.ReadBoolean();
        StabTimeLeft = reader.ReadUInt16();
        TauntTimer = reader.ReadUInt16();
        ComboCount = reader.ReadUInt16();
    }

    public void SendSyncAI()
    {
        if (Main.netMode == NetmodeID.SinglePlayer) return;
        DonQuijoteDeLaManchaAISync.Get(
            Player.whoAmI,
            DashCoolDown,
            DashCoolDownMax,
            Dashing,
            NextHitImmune,
            StabTimeLeft,
            TauntTimer,
            ComboCount)
            .Send();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        DonQuijoteDeLaManchaPlayer clone = (DonQuijoteDeLaManchaPlayer)targetCopy;
        clone.itemDefinition = itemDefinition;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        DonQuijoteDeLaManchaPlayer clone = (DonQuijoteDeLaManchaPlayer)clientPlayer;
        if (itemDefinition.ToString() != clone.itemDefinition.ToString())
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    // ---- 数据持久化 ----
    public override void SaveData(TagCompound tag)
    {
        tag.Add("targetItem", itemDefinition);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        itemDefinition = tag.Get<ItemDefinition>("targetItem");
        base.LoadData(tag);
    }

    // ---- 加载纹理 ----
    public override void Load()
    {
        if (!Main.dedServ)
        {
            _numberTex = ModContent.Request<Texture2D>("MatterRecord/Contents/DonQuijoteDeLaMancha/Number", AssetRequestMode.ImmediateLoad).Value;
            _hitTex = ModContent.Request<Texture2D>("MatterRecord/Contents/DonQuijoteDeLaMancha/Hit", AssetRequestMode.ImmediateLoad).Value;
        }
        base.Load();
    }
}