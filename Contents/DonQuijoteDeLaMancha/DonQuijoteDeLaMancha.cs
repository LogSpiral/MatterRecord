using MatterRecord.Contents.DonQuijoteDeLaMancha.Core;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.BuiltInGroups;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.MeleeCore;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.StandardMelee;
using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using MatterRecord.Contents.Recorder;
using MatterRecord.Contents.TortoiseShell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;
using MatterRecord.Contents.EternalWine;
namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

using global::MatterRecord.Contents.EternalWine;
using System.Collections.ObjectModel;

public class DonQuijoteDeLaMancha : MeleeSequenceItem<DonQuijoteDeLaManchaProj>, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.DonQuijoteDeLaMancha;
    public static bool SlashActive => MatterRecordConfig.Instance.DonQuijoteSlashActive;

    public static Dictionary<int, Vector2> WindmillPositions = new Dictionary<int, Vector2>();

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = 66;
        Item.height = 66;
        Item.rare = ItemRarityID.Quest;
        Item.UseSound = SoundID.Item71;
        Item.damage = 21;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.knockBack = 4f;
        Item.value = 5;
        Item.useTurn = true;
        Item.noUseGraphic = false;
        Item.noMelee = false;
    }
    public override bool CanShoot(Player player)
    {
        if (SlashActive)
            return base.CanShoot(player);
        bool flag = (player.altFunctionUse == 2 || player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0) && player.ownedProjectileCounts[ModContent.ProjectileType<DonQuijoteDeLaManchaProj>()] == 0;
        if (flag)
            return true;
        Item.shoot = ProjectileID.None;
        Item.noUseGraphic = false;
        Item.noMelee = false;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.channel = false;
        return false;
    }
    public override bool EnableRightClick => true;
    public override bool? UseItem(Player player)
    {
        if (SlashActive)
        {
            Item.shoot = ModContent.ProjectileType<DonQuijoteDeLaManchaProj>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            return base.UseItem(player);
        }
        if (player.whoAmI == Main.myPlayer)
        {
            if (player.altFunctionUse != 2 && player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft <= 0)
            {
                Item.shoot = ProjectileID.None;
                Item.noUseGraphic = false;
                Item.noMelee = false;
                Item.useStyle = ItemUseStyleID.Swing;
                Item.channel = false;
            }
            else
            {
                Item.shoot = ModContent.ProjectileType<DonQuijoteDeLaManchaProj>();
                Item.noUseGraphic = true;
                Item.noMelee = true;
                Item.useStyle = ItemUseStyleID.Shoot;
                Item.channel = true;
            }
        }
        return base.UseItem(player);
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.EoCShield);
        base.AddRecipes();
    }
    public override bool AltFunctionUse(Player player)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        if (mplr.DashCoolDown <= 0)
            return true;

        if (mplr.StabTimeLeft > 0)
        {
            mplr.StabTimeLeft = 0;
            mplr.SendSyncAI();
            for (int n = 0; n < 30; n++)
            {
                Dust.NewDustPerfect(player.Center, DustID.Shadowflame, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16));
            }
            CombatText.NewText(player.Hitbox with { Y = player.Hitbox.Y + 64 }, Color.MediumPurple, this.GetLocalizedValue("CancelStabState"));
        }
        return false;
    }

    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (Item.TryGetPrefixStatMultipliersForItem(item.prefix, out var dmg, out _, out _, out _, out _, out _, out _, out _, out _, out _))
            damage.Base = (int)((Math.Clamp(item.damage, 1, int.MaxValue) - 21) * dmg);
        else
            damage.Base = Math.Clamp(item.damage, 1, int.MaxValue) - 21;
        base.ModifyWeaponDamage(player, ref damage);
    }

    public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (item.knockBack == 0) return;
        knockback.Base = item.knockBack;
        base.ModifyWeaponKnockback(player, ref knockback);
    }

    public override void ModifyWeaponCrit(Player player, ref float crit)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (item.crit == 0) return;
        crit += item.crit * .01f - .04f;
        base.ModifyWeaponCrit(player, ref crit);
    }

    public override float UseTimeMultiplier(Player player)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (item.shoot != ProjectileID.None && item.noMelee && item.useTime > 0)
            return item.useTime / 60f;
        if (item.useAnimation == 0) return 1f;
        return item.useAnimation / 60f;
    }

    public override float UseAnimationMultiplier(Player player)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (item.shoot != ProjectileID.None && item.noMelee && item.useTime > 0)
            return item.useTime / 60f;
        if (item.useAnimation == 0) return 1f;
        return item.useAnimation / 60f;
    }

    public override void HoldItem(Player player)
    {
        player.aggro += 400;
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();

        if (player.altFunctionUse != 2 && player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft <= 0 && !SlashActive)
        {
            Item.shoot = ProjectileID.None;
            Item.noUseGraphic = false;
            Item.noMelee = false;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.channel = false;
            if (player.itemAnimation == player.itemAnimationMax)
                player.lastVisualizedSelectedItem = Item.Clone();
        }
        else
        {
            Item.shoot = ModContent.ProjectileType<DonQuijoteDeLaManchaProj>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
        }

        mplr.HoldingDonQuijote = true;

        base.HoldItem(player);
    }

    public static bool Active;

    public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (Active)
        {
            float factor = Main.GlobalTimeWrappedHourly % 1;
            spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), 0, origin, scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);
        }
        base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
    }

    public override void PostDrawInWorld(WorldItem item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (Active)
        {
            float factor = Main.GlobalTimeWrappedHourly % 1;
            spriteBatch.Draw(TextureAssets.Item[Type].Value, item.Center - Main.screenPosition, null, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), rotation, new Vector2(33), scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);
        }
    }

    public override bool CanRightClick()
    {
        return RecorderSystem.CheckUnlock(ItemRecords.DonQuijoteDeLaMancha);
    }

    public override bool ConsumeItem(Player player)
    {
        return false;
    }

    public override void RightClick(Player player)
    {
        Active = !Active;
        SoundEngine.PlaySound(SoundID.Item4);
        base.RightClick(player);
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        // ---- 原有继承物品、速度、伤害等逻辑（保持不变） ----
        var index = tooltips.FindIndex(0, line => line.Name.StartsWith("Prefix"));
        if (index == -1)
            index = tooltips.FindIndex(0, line => line.Name == "JourneyResearch");
        var mplr = Main.LocalPlayer.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        if (mplr.itemDefinition.Type > 0)
        {
            var line = new TooltipLine(Mod, "targetItem", this.GetLocalizedValue("InheritedFrom") + $" {mplr.itemDefinition.DisplayName}[i:{mplr.itemDefinition.Type}]");
            if (index == -1)
                tooltips.Add(line);
            else
                tooltips.Insert(index, line);
            foreach (var tips in tooltips)
            {
                if (tips.Name == "Speed")
                {
                    int time = new Item(mplr.itemDefinition.Type).useAnimation;
                    string str = Lang.tip[time switch
                    {
                        <= 8 => 6,
                        <= 20 => 7,
                        <= 25 => 8,
                        <= 30 => 9,
                        <= 35 => 10,
                        <= 45 => 11,
                        <= 55 => 12,
                        _ => 13
                    }].Value;
                    tips.Text = str;
                    break;
                }
            }
        }
        else
        {
            var line = new TooltipLine(Mod, "FindItemPlz", this.GetLocalizedValue("FindItemHint"));
            if (index == -1)
                tooltips.Add(line);
            else
                tooltips.Insert(index, line);
        }
        float k = Main.mouseTextColor / 255f;
        k = .85f + .15f * k;
        var dmgTip = new TooltipLine(Mod, "Sheep", this.GetLocalizedValue("SheepDamage")) { Color = new Color(120, 190, 120, 255) * k };
        if (index == -1)
            tooltips.Add(dmgTip);
        else
            tooltips.Insert(index + 1, dmgTip);

        // ---- ★ 连击提示与连击加成列表：同属主 tip 区域 ----
        // 仅在连击系统解锁时显示
        if (DonQuijoteProgression.Tier5_ComboSystem)
        {
            bool shiftHeld = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);
            if (shiftHeld)
            {
                // 按住 Shift：显示连击加成列表
                // 标题
                tooltips.Add(new TooltipLine(Mod, "ComboTitle", this.GetLocalizedValue("ComboTitle"))
                {
                    Color = Color.Cyan
                });

                // 各阈值效果（固定描述，不依赖当前连击数）
                tooltips.Add(new TooltipLine(Mod, "Combo5", this.GetLocalizedValue("Combo5"))
                {
                    Color = Color.LightGray
                });
                tooltips.Add(new TooltipLine(Mod, "Combo10", this.GetLocalizedValue("Combo10"))
                {
                    Color = Color.LightGray
                });
                tooltips.Add(new TooltipLine(Mod, "Combo15", this.GetLocalizedValue("Combo15"))
                {
                    Color = Color.LightGray
                });
                tooltips.Add(new TooltipLine(Mod, "Combo20", this.GetLocalizedValue("Combo20"))
                {
                    Color = Color.LightGray
                });
            }
            else
            {
                // 未按住 Shift：连击查看提示放在主 tip（与连击加成列表同区）
                tooltips.Add(new TooltipLine(Mod, "ComboHint", this.GetLocalizedValue("ComboHint"))
                {
                    Color = Color.Gray
                });
            }
        }

        base.ModifyTooltips(tooltips);
    }

    // ---- 独立 Shift 提示（完全参照蝇王 PreDrawTooltip 实现） ----
    public override bool PreDrawTooltip(ReadOnlyCollection<TooltipLine> lines, ref int x, ref int y)
    {
        if (!this.IsRecordUnlocked)
            return true;

        bool shiftHeld = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);

        var extraLines = new List<TooltipLine>();

        if (!shiftHeld)
        {
            // 未按住 Shift：只添加强化进度查看提示（连击提示已移至 ModifyTooltips 主 tip）
            extraLines.Add(new TooltipLine(Mod, "ShiftHint", this.GetLocalizedValue("ShiftHint"))
            {
                Color = Color.Gray
            });

            MiscMethods.DrawTagTooltips(lines, extraLines, x, y);
            return true;
        }

        // 按住 Shift：显示强化进度列表
        var bossIconMap = new Dictionary<string, string>
        {
            { "史莱姆王", "[i:2493]" },
            { "克眼", "[i:2112]" },
            { "蜂后", "[i:2108]" },
            { "骷髅王", "[i:1281]" },
            { "肉山", "[i:2105]" },
            { "史莱姆皇后", "[i:4959]" },
            { "世花", "[i:2109]" },
            { "石巨人", "[i:2110]" },
            { "猪鲨", "[i:2588]" },
            { "光女", "[i:4784]" },
            { "教徒", "[i:3372]" },
            { "月总", "[i:3373]" },
        };

        var progression = new (string name, bool unlocked, string bossName)[]
        {
            ("Tier1_Block", DonQuijoteProgression.Tier1_Block, "史莱姆王"),
            ("Tier2_StabAfterDash", DonQuijoteProgression.Tier2_StabAfterDash, "克眼"),
            ("Tier3_TauntOnHit", DonQuijoteProgression.Tier3_TauntOnHit, "世界吞噬者/克脑"),
            ("Tier4_StabDRAndMove", DonQuijoteProgression.Tier4_StabDRAndMove, "蜂后"),
            ("Tier5_ComboSystem", DonQuijoteProgression.Tier5_ComboSystem, "骷髅王"),
            ("Tier6_StabReduceCooldown", DonQuijoteProgression.Tier6_StabReduceCooldown, "肉山"),
            ("Tier7_HitRestoreWing", DonQuijoteProgression.Tier7_HitRestoreWing, "史莱姆皇后"),
            ("Tier8_SpearThrow", DonQuijoteProgression.Tier8_SpearThrow, "任意机械Boss"),
            ("Tier9_ComboDecayAndCap20", DonQuijoteProgression.Tier9_ComboDecayAndCap20, "世花"),
            ("Tier10_WindmillErase", DonQuijoteProgression.Tier10_WindmillErase, "石巨人"),
            ("Tier11_DashEraseHealCombo", DonQuijoteProgression.Tier11_DashEraseHealCombo, "猪鲨"),
            ("Tier12_SpearUpgrade", DonQuijoteProgression.Tier12_SpearUpgrade, "光女"),
            ("Tier13_Revive", DonQuijoteProgression.Tier13_Revive, "教徒"),
            ("Tier14_BlockBoostWithWindmill", DonQuijoteProgression.Tier14_BlockBoostWithWindmill, "月总"),
        };

        int unlockedCount = 0;
        foreach (var item in progression)
            if (item.unlocked) unlockedCount++;

        // 标题
        extraLines.Add(new TooltipLine(Mod, "ProgressionTitle", this.GetLocalization("ProgressionTitle").Format(unlockedCount))
        {
            Color = Color.White
        });

        // 每一项
        for (int i = 0; i < progression.Length; i++)
        {
            var (nameKey, unlocked, bossName) = progression[i];
            string iconStr = "";

            if (bossName == "世界吞噬者/克脑")
            {
                iconStr = "[i:2111] [i:2104] ";
            }
            else if (bossName == "任意机械Boss")
            {
                iconStr = "[i:2113] [i:2107] [i:2106] ";
            }
            else if (bossIconMap.TryGetValue(bossName, out string iconTag))
            {
                iconStr = iconTag + " ";
            }

            string displayText = iconStr + this.GetLocalizedValue(nameKey);
            extraLines.Add(new TooltipLine(Mod, "Progression" + (i + 1), displayText)
            {
                Color = unlocked ? Color.Yellow : Color.Gray
            });
        }

        MiscMethods.DrawTagTooltips(lines, extraLines, x, y);
        return true;
    }
}

public class DonQuijoteGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private static Vector2?[] _originalPositions = new Vector2?[Main.maxPlayers];

    public override bool PreAI(NPC npc)
    {
        int targetPlayer = npc.target;
        if (targetPlayer >= 0 && targetPlayer < Main.maxPlayers)
        {
            var player = Main.player[targetPlayer];
            if (player != null && player.active && !player.dead)
            {
                var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (mplr.TauntTimer > 0 && DonQuijoteDeLaMancha.WindmillPositions.TryGetValue(targetPlayer, out Vector2 windmillPos))
                {
                    if (!_originalPositions[targetPlayer].HasValue)
                        _originalPositions[targetPlayer] = player.position;
                    player.position = windmillPos - player.Size * 0.5f;
                    npc.TargetClosest();
                }
            }
        }
        return base.PreAI(npc);
    }

    public override void PostAI(NPC npc)
    {
        int targetPlayer = npc.target;
        if (targetPlayer >= 0 && targetPlayer < Main.maxPlayers)
        {
            if (_originalPositions[targetPlayer].HasValue)
            {
                Main.player[targetPlayer].position = _originalPositions[targetPlayer].Value;
                _originalPositions[targetPlayer] = null;
            }
        }
        base.PostAI(npc);
    }
}

public class DonQuijoteDeLaManchaProj : MeleeSequenceProj
{
    public override bool LabeledAsCompleted => true;

    public override void InitializeStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {
        standardInfo.itemType = ModContent.ItemType<DonQuijoteDeLaMancha>();
        vertexStandard.timeLeft = 10;
        vertexStandard.colorVec = new(0, 1, 0);
        vertexStandard.alphaFactor = 2f;

        Projectile.usesLocalNPCImmunity = true;
    }

    public override void UpdateStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {
        var mplr = Player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        int type = mplr.itemDefinition.Type;
        int timer = 60;
        if (type != 0)
        {
            var item = ContentSamples.ItemsByType[type];
            if (item.shoot != ProjectileID.None && item.noMelee && item.useTime > 0)
                timer = item.useTime;
            else if (item.useAnimation != 0)
                timer = item.useAnimation;
        }

        float attackSpeed = Player.GetAttackSpeed(DamageClass.Melee);
        int adjustedTimer = (int)Math.Round(timer / attackSpeed);
        standardInfo.standardTimer = Player.controlUseItem && !Player.controlUseTile ? Math.Clamp(adjustedTimer, 1, 30) : 10;

        standardInfo.standardColor = Color.DarkRed * (Player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0 ? 0.3f : 0.1f);
        standardInfo.standardOrigin = Player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0 ? new Vector2(.3f, .7f) : new Vector2(.1f, .9f);

        vertexStandard.scaler = DonQuijoteDeLaMancha.SlashActive || mplr.StabTimeLeft > 0 || mplr.Dashing ? 120 : 0;
    }

    private class CustomSwooshInfo : SwooshInfo
    {
        private float _initialSize;
        private bool _wingTimeRestored;
        private bool _comboAdded;

        public override void OnStartSingle()
        {
            if (_initialSize == 0f)
                _initialSize = ModifyData.Size;

            float scale = (Owner as Player)?.HeldItem.scale ?? 1f;
            var mplr = (Owner as Player)?.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
            float comboMult = mplr?.ComboSizeMultiplier ?? 1f;
            var data = ModifyData;
            data.Size = _initialSize * scale * comboMult;
            ModifyData = data;

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
        private float _initialSize;
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
                if (plr != null)
                {
                    plr.fallStart = (int)(plr.position.Y / 16f);
                }
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
            if (_initialSize == 0f)
                _initialSize = ModifyData.Size;

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
            float scale = (Owner as Player)?.HeldItem.scale ?? 1f;
            var mplr = (Owner as Player)?.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
            float comboMult = mplr?.ComboSizeMultiplier ?? 1f;
            var data = ModifyData;
            data.Size = _initialSize * scale * comboMult;
            ModifyData = data;

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
        private float _initialSize;
        private bool _wingTimeRestored;
        private bool _comboAdded;

        public override void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
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
            if (_initialSize == 0f)
                _initialSize = ModifyData.Size;

            float scale = (Owner as Player)?.HeldItem.scale ?? 1f;
            var mplr = (Owner as Player)?.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
            float comboMult = mplr?.ComboSizeMultiplier ?? 1f;
            var data = ModifyData;
            data.Size = _initialSize * scale * comboMult;
            ModifyData = data;

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

    public override void AI()
    {
        var mplr = Player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        if (Player.controlUseItem && !Player.controlUseTile && mplr.StabTimeLeft <= 0 && CurrentElement is not DonQuijoteDeLaManchaDash && !MatterRecordConfig.Instance.DonQuijoteSlashActive)
        {
            Projectile.Kill();
            return;
        }

        base.AI();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        var player = Player;
        float delta = Main.rand.NextFloat(0.85f, 1.15f) * (damageDone / MathHelper.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue));
        for (int n = 0; n < 30 * delta * (StandardInfo.dustAmount + .2f); n++)
            MiscMethods.FastDust(target.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), StandardInfo.standardColor);

        base.OnHitNPC(target, hit, damageDone);
        Projectile.localNPCHitCooldown = Math.Clamp(StandardInfo.standardTimer / 2, 1, 514);

        if (CurrentElement is CustomSwooshInfo swoosh)
            swoosh.HandleHit(target, damageDone);
        else if (CurrentElement is DonQuijoteDeLaManchaDash dash)
            dash.HandleHit(target, damageDone);
    }

    private static Condition MouseLeft { get; set; }
    private static Condition MouseRight { get; set; }
    private static Condition StabActive { get; set; }
    private static Condition Always { get; set; }

    public override void Load()
    {
        base.Load();
        MouseLeft = new("MouseLeft", () => Main.LocalPlayer.controlUseItem && Main.LocalPlayer.altFunctionUse != 2);
        MouseRight = new("MouseRight", () => Main.LocalPlayer.controlUseTile || Main.LocalPlayer.altFunctionUse == 2);
        StabActive = new("StabActive", () => Main.LocalPlayer.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0);
        Always = new("Always", () => true);
    }

    protected override void SetupSequence(Sequence sequence)
    {
        var mplr = Player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        mplr.Dashing = false;
        var outerGroup = new ConditionalMultiGroup();
        var innerGroup = new ConditionalMultiGroup();

        innerGroup.DataList.Add(new() { Wrapper = new Wrapper(new DonQuijoteDeLaManchaStab() { givenCycle = 1, rangeOffsetMin = 0, rangeOffsetMax = 1, ModifyData = new(1.00f, .5f, .5f, 1.10f, 0, 1.00f) }), Argument = new(StabActive) });
        innerGroup.DataList.Add(new() { Wrapper = new Wrapper(new CustomSwooshInfo()), Argument = new(Always) });
        var innerSequence = new Sequence();
        innerSequence.Groups.Add(innerGroup);

        outerGroup.DataList.Add(new() { Wrapper = new Wrapper(innerSequence), Argument = new(MouseLeft) });
        outerGroup.DataList.Add(new() { Wrapper = new Wrapper(new DonQuijoteDeLaManchaDash() { ModifyData = new(1.00f, 3.00f, 1.00f, 1.00f, 100, 10.00f) }), Argument = new(MouseRight) });
        sequence.Groups.Add(outerGroup);
    }
}

public class DonQuijoteGBItem : GlobalItem
{
    public override void Load()
    {
        On_ItemSlot.TryItemSwap += On_ItemSlot_TryItemSwap;
        MonoModHooks.Add(typeof(ItemLoader).GetMethod(nameof(ItemLoader.RightClick), BindingFlags.Static | BindingFlags.Public), DonQuijoteModifyRightClick);
        base.Load();
    }

    public static void DonQuijoteModifyRightClick(Action<Item, Player> orig, Item item, Player player)
    {
        if (!DonQuijoteDeLaMancha.Active) goto Label;
        var damageClass = item.DamageType;
        bool flag = damageClass == DamageClass.Melee || damageClass.GetEffectInheritance(DamageClass.Melee) || !damageClass.GetModifierInheritance(DamageClass.Melee).Equals(StatInheritanceData.None);
        if (item.damage <= 0 || !flag) goto Label;
        if (item.useTime == 0) goto Label;
        if (item.type == ModContent.ItemType<DonQuijoteDeLaMancha>()) goto Label;
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        mplr.itemDefinition = new(item.type);
        if (Main.netMode == NetmodeID.MultiplayerClient)
            mplr.SyncPlayer(-1, player.whoAmI, false);
        item.stack++;
        DonQuijoteDeLaMancha.Active = false;
    Label:
        orig.Invoke(item, player);
    }

    private void On_ItemSlot_TryItemSwap(On_ItemSlot.orig_TryItemSwap orig, Item item)
    {
        if (DonQuijoteDeLaMancha.Active) return;
        orig.Invoke(item);
    }

    public override bool CanRightClick(Item item)
    {
        if (item.type == ItemID.None) return false;
        if (!DonQuijoteDeLaMancha.Active) return false;
        var damageClass = item.DamageType;
        bool flag = damageClass == DamageClass.Melee || damageClass.GetEffectInheritance(DamageClass.Melee) || !damageClass.GetModifierInheritance(DamageClass.Melee).Equals(StatInheritanceData.None);
        if (item.damage <= 0 || !flag) return false;
        if (item.useTime == 0) return false;
        if (item.type == ModContent.ItemType<UnloadedItem>()) return false;
        if (item.type == ModContent.ItemType<DonQuijoteDeLaMancha>()) return false;
        if (DonQuijoteDeLaMancha.Active)
            return Main.mouseRightRelease = true;

        return false;
    }
}

public class WindMill : ModProjectile
{
    private static Asset<Texture2D> wheelTex;
    private bool _registered = false;

    private const int EraseCooldownTicks = 5;
    private static readonly Dictionary<int, int> _projEraseCooldown = new();

    public override void SetStaticDefaults()
    {
        wheelTex = ModContent.Request<Texture2D>("MatterRecord/Contents/DonQuijoteDeLaMancha/WindMill_Wheel");
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Projectile.timeLeft = 600;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.width = 106;
        Projectile.height = 136;

        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;

        base.SetDefaults();
    }

    public override void AI()
    {
        if (Projectile.ai[0] == 0f)
            Projectile.ai[0] = 10f;

        Projectile.ai[1] += Projectile.ai[0];

        if (Projectile.active && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
        {
            DonQuijoteDeLaMancha.WindmillPositions[Projectile.owner] = Projectile.Center;
        }

        if (!_registered && Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
        {
            var player = Main.player[Projectile.owner];
            if (player != null)
            {
                var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                mplr.WindmillCount++;
                _registered = true;
            }
        }

        EraseEnemyProjectiles();

        base.AI();
    }

    public override void OnKill(int timeLeft)
    {
        if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
        {
            var player = Main.player[Projectile.owner];
            if (player != null)
            {
                var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (mplr.WindmillCount > 0)
                    mplr.WindmillCount--;
            }
            DonQuijoteDeLaMancha.WindmillPositions.Remove(Projectile.owner);
        }
        base.OnKill(timeLeft);
    }

    public static void KillProjectile(Projectile proj)
    {
        proj.Kill();
        proj.netUpdate = true;
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
    }

    private void EraseEnemyProjectiles()
    {
        if (!DonQuijoteProgression.Tier10_WindmillErase) return;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (!p.active || p.friendly)
                continue;
            if (!p.Hitbox.Intersects(Projectile.Hitbox))
                continue;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                if (CanSendErase(i))
                    WindMillEraseSync.Get(i).Send();
                p.Kill();
            }
            else
            {
                KillProjectile(p);
            }
        }
    }

    private bool CanSendErase(int index)
    {
        int now = (int)Main.GameUpdateCount;
        if (_projEraseCooldown.TryGetValue(index, out int last) && now - last < EraseCooldownTicks)
            return false;
        _projEraseCooldown[index] = now;
        return true;
    }

    public override bool PreDraw(Player player, ref Color lightColor)
    {
        float progress = 1f - (float)Projectile.timeLeft / 600f;
        float alpha = 1f;
        if (progress < 0.1f)
            alpha = progress / 0.1f;
        else if (progress > 0.9f)
            alpha = (1f - progress) / 0.1f;
        lightColor *= alpha;
        return true;
    }

    public override void PostDraw(Player player, Color lightColor)
    {
        Main.spriteBatch.Draw(wheelTex.Value, Projectile.Center + new Vector2(0, -12) - Main.screenPosition, null, lightColor, Projectile.ai[1], new Vector2(53, 55), 1f, 0, 0);
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Player player = Main.player[Projectile.owner];
        if (player == null) return;
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();

        mplr.TryAddCombo();

        for (int i = 0; i < 5; i++)
        {
            Dust.NewDust(target.Center, 10, 10, DustID.Firework_Red, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
        }

        base.OnHitNPC(target, hit, damageDone);
    }
}