﻿using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using System.Reflection;
using Terraria.ModLoader.Default;
using Terraria.UI;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.IO;
using ReLogic.Content;
using Terraria.UI.Chat;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Core;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.System;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Contents.Melee.StandardMelee;
using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing.RenderDrawingContents;
using LogSpiralLibrary.CodeLibrary.Utilties;
using LogSpiralLibrary.CodeLibrary.Utilties.Extensions;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

public class DonQuijoteDeLaMancha : MeleeSequenceItem<DonQuijoteDeLaManchaProj>
{
    public static bool SlashActive => MatterRecordConfig.Instance.DonQuijoteSlashActive;
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.width = 66;
        Item.height = 66;

        Item.rare = ItemRarityID.Red;
        Item.UseSound = MySoundID.Scythe;
        /*Item.damage = 155;
        Item.useTime = 24;
        time = 24;
        Item.knockBack = 14f;*/

        Item.damage = 21;
        Item.useTime = 60;
        Item.useAnimation = 60;
        Item.knockBack = 10f;
        Item.value = Item.sellPrice(0, 2);
        Item.useTurn = true;
        Item.noUseGraphic = false;
        Item.noMelee = false;
    }
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
                Item.shoot = 0;
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
    public override bool AltFunctionUse(Player player)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        if (mplr.DashCoolDown <= 0)
            return true;

        if (mplr.StabTimeLeft > 0)
        {
            mplr.StabTimeLeft = 0;
            for (int n = 0; n < 30; n++)
            {
                Dust.NewDustPerfect(player.Center, MyDustId.PurpleLight, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16));
            }
            CombatText.NewText(player.Hitbox with { Y = player.Hitbox.Y + 64 }, Color.MediumPurple, "解除突刺状态");
        }
        return false;
    }
    public override void Load()
    {
        On_Item.TryGetPrefixStatMultipliersForItem += On_Item_TryGetPrefixStatMultipliersForItem;
        base.Load();
    }

    private bool On_Item_TryGetPrefixStatMultipliersForItem(On_Item.orig_TryGetPrefixStatMultipliersForItem orig, Item self, int rolledPrefix, out float dmg, out float kb, out float spd, out float size, out float shtspd, out float mcst, out int crt)
    {
        PreFixStat = orig;
        return orig.Invoke(self, rolledPrefix, out dmg, out kb, out spd, out size, out shtspd, out mcst, out crt);
    }

    public static On_Item.orig_TryGetPrefixStatMultipliersForItem PreFixStat;
    public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (PreFixStat?.Invoke(Item, Item.prefix, out float dmg, out _, out _, out _, out _, out _, out _) == true)
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
        if (item.useAnimation == 0) return 1f;
        return item.useAnimation / 60f;
    }
    public override float UseAnimationMultiplier(Player player)
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        var definition = mplr.itemDefinition;
        var item = new Item(definition.Type);
        if (item.useAnimation == 0) return 1f;
        return item.useAnimation / 60f;
    }
    public override void HoldItem(Player player)
    {
        player.aggro += 400;
        if (player.whoAmI == Main.myPlayer)
        {
            if (player.altFunctionUse != 2 && player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft <= 0 && !SlashActive)
            {
                Item.shoot = 0;
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
        }
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
    public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
    {
        if (Active)
        {
            float factor = Main.GlobalTimeWrappedHourly % 1;
            spriteBatch.Draw(TextureAssets.Item[Type].Value, Item.Center - Main.screenPosition, null, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), rotation, new Vector2(33), scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);

        }
        base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
    }
    public override bool CanRightClick()
    {
        return true;
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
            var line = new TooltipLine(Mod, "FindItemPlz", this.GetLocalizedValue("FinItemHint"));
            if (index == -1)
                tooltips.Add(line);
            else
                tooltips.Insert(index, line);
        }
        float k = Main.mouseTextColor / 255f;
        k = .85f + .15f * k;
        var dmgTip = new TooltipLine(Mod, "Sheep", this.GetLocalizedValue("SheepDamage")) { OverrideColor = new Color(120, 190, 120, 255) * k };
        if (index == -1)
            tooltips.Add(dmgTip);
        else
            tooltips.Insert(index + 1, dmgTip);

        base.ModifyTooltips(tooltips);
    }
    public override bool CanShoot(Player player)
    {
        if (SlashActive)
            return base.CanShoot(player);
        bool flag = (player.altFunctionUse == 2 || player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0) && player.ownedProjectileCounts[ModContent.ProjectileType<DonQuijoteDeLaManchaProj>()] == 0;
        if (flag)
            return true;
        Item.shoot = 0;
        Item.noUseGraphic = false;
        Item.noMelee = false;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.channel = false;
        return false;
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
    }
    public override void UpdateStandardInfo(StandardInfo standardInfo, VertexDrawStandardInfo vertexStandard)
    {

        standardInfo.standardColor = Color.DarkRed * (player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0 ? 0.3f : 0.1f);
        standardInfo.standardTimer = player.controlUseItem && !player.controlUseTile ? Math.Clamp(player.itemTimeMax, 1, 30) : 10;
        standardInfo.standardOrigin = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0 ? new Vector2(.3f, .7f) : new Vector2(.1f, .9f);

        vertexStandard.scaler = DonQuijoteDeLaMancha.SlashActive || player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0 || player.GetModPlayer<DonQuijoteDeLaManchaPlayer>().Dashing ? 120 : 0;

    }
    //public override string Texture => $"Terraria/Images/Item_{ItemID.ShadowJoustingLance}";

    class DonQuijoteDeLaManchaDash : MeleeAction
    {
        public override string Category => "";
        public override bool Attacktive => Factor < .65f;
        //bool givenUp;
        Vector2 originVelocity;
        WindMill windMill;
        public override void OnEndAttack()
        {
            Owner.velocity *= MathHelper.Clamp(.05f * (1 + MathF.Sqrt(originVelocity.Length())), 0, 1);
            Owner.velocity += originVelocity * MathF.Pow(0.9f, originVelocity.Length());//
            if (Owner is Player plr)
            {
                var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                var maxT = 60;
                if (mplr.itemDefinition != null && mplr.itemDefinition.Type > 0)
                    maxT = new Item(mplr.itemDefinition.Type).useAnimation;
                mplr.StabTimeLeft = Math.Max((maxT < 30 ? 6 * maxT + 300 : 12 * maxT + 120) * 3 / 4, 300);
                mplr.NextHitImmune = false;
                mplr.Dashing = false;
            }
            base.OnEndAttack();
        }
        public override void OnAttack()
        {

            //Owner.velocity += targetedVector * .125f * (1 - Factor).HillFactor2();
            if (Owner is Player plr)
            {
                plr.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake += 1f / (1f + timerMax / 30f);
                plr.GetModPlayer<LogSpiralLibraryPlayer>().ultraFallEnable = true;
                //if (!plr.controlUseTile && !plr.controlUseItem && !givenUp)
                //{
                //    timer = 2;
                //    givenUp = true;
                //}

                //plr.velocity += targetedVector * 0.05f;
                //plr.immune = true;
                //plr.immuneTime = 6;
            }
            var rand = Main.rand.NextFloat(0.25f, 0.5f);
            for (int k = 0; k < 15; k++)
            {
                var vec = ((k * MathHelper.TwoPi / 15f).ToRotationVector2() * 6 * new Vector2(rand, 1)).RotatedBy(Rotation);
                //DoggoDust(Owner.Center, vec - targetedVector * .5f);
                //DoggoDust(Owner.Center, vec * 2 - targetedVector * .25f);
            }
            if (current != null)
            {
                current.timeLeft = (byte)MathHelper.Lerp(current.timeLeftMax, 0, Utils.GetLerpValue(0.25f, 1, Factor, true));
                current.xScaler = 2 + Owner.velocity.Length() / 32;
                current.scaler = standardInfo.VertexStandard.scaler * ModifyData.actionOffsetSize * offsetSize / 3 * 4f * current.xScaler;
                current.center = Owner.Center - Rotation.ToRotationVector2() * current.scaler * .5f;
                current.rotation = Rotation;
                current.negativeDir = flip;
            }
            base.OnAttack();
        }
        public override void OnStartSingle()
        {
            //givenUp = false;
            KValue = 1f;
            if (Owner is Player plr)
            {
                var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (mplr.DashCoolDown > 0)
                {
                    Projectile.Kill();
                    return;
                }
                var proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), plr.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition, default, ModContent.ProjectileType<WindMill>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                windMill = proj.ModProjectile as WindMill;
            }


            base.OnStartSingle();
        }
        public override void OnStartAttack()
        {
            SoundEngine.PlaySound(SoundID.Item92);
            var rand = Main.rand.NextFloat(0.25f, 0.5f);
            originVelocity = Owner.velocity;
            //if (Owner is Player plr) plr.AddBuff(ModContent.BuffType<DoggoBoost>(), 180);
            if (windMill != null)
                windMill.Projectile.ai[0] = 10;
            if (Owner is Player plr)
            {
                var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                mplr.NextHitImmune = true;
                mplr.Dashing = true;
                var maxT = 60;
                if (mplr.itemDefinition != null && mplr.itemDefinition.Type > 0)
                    maxT = new Item(mplr.itemDefinition.Type).useAnimation;
                mplr.DashCoolDown = maxT < 30 ? 6 * maxT + 300 : 12 * maxT + 120;
                mplr.DashCoolDownMax = mplr.DashCoolDown;
                mplr.startPoint = plr.Center;
            }

            Owner.velocity += targetedVector.SafeNormalize(default) * (1200 / timerMax + 45);
            if (Owner is Player plr2)
            {
                Owner.velocity *= ((plr2.GetModPlayer<LogSpiralLibraryPlayer>().targetedMousePosition - plr2.Center).Length()) / 1440 + 1 / 6f - .1f;
            }
            for (int k = 0; k < 60; k++)
            {
                var vec = ((k * MathHelper.TwoPi / 60f).ToRotationVector2() * 15 * new Vector2(rand, 1)).RotatedBy(Rotation);
                //DoggoDust(Owner.Center, vec - targetedVector);
                //DoggoDust(Owner.Center, vec * 2 - targetedVector * .5f);

            }
            var verS = standardInfo.VertexStandard;
            if (verS.active)
            {
                var u = UltraStab.NewUltraStabOnDefaultCanvas(verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize / 3 * 8f, Owner.Center - targetedVector);
                u.heatMap = verS.heatMap;
                u.negativeDir = flip;
                u.rotation = Rotation;
                u.xScaler = 16;
                u.ColorVector = verS.colorVec;
                u.ApplyStdValueToVtxEffect(standardInfo);
                current = u;
            }
            base.OnStartAttack();
        }
        UltraStab current;
    }
    class DonQuijoteDeLaManchaStab : RapidlyStabInfo
    {
        public override string Category => "";
        public override void OnHitEntity(Entity victim, int damageDone, object[] context)
        {
            if (Owner is Player player)
            {
                damageDone /= Math.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue);
                var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                if (victim is NPC npc && npc.CanBeChasedBy())
                {
                    hitCount++;
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
            float delta = Main.rand.NextFloat(0.85f, 1.15f) * damageDone;
            Main.LocalPlayer.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake += delta * .15f;//
            for (int n = 0; n < 30 * delta * (standardInfo.dustAmount + .2f); n++)
                MiscMethods.FastDust(victim.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), standardInfo.standardColor);
        }
        int hitCount = 0;
        public override void OnEndSingle()
        {
            Owner.velocity += targetedVector.SafeNormalize(default) * 12;
            Owner.velocity = Owner.velocity.SafeNormalize(default) * 12;
            base.OnEndSingle();
        }
        public override void OnStartSingle()
        {
            hitCount = 0;
            base.OnStartSingle();
        }
        public override void OnEndAttack()
        {
            SoundEngine.PlaySound(SoundID.Item96 with { Volume = 0.5f * SoundID.Item96.Volume });//MaxInstances =-1,
            base.OnEndAttack();
        }
    }

    public override void AI()
    {
        var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        if (player.controlUseItem && !player.controlUseTile && mplr.StabTimeLeft <= 0 && currentData is not DonQuijoteDeLaManchaDash && !MatterRecordConfig.Instance.DonQuijoteSlashActive)
        {
            Projectile.Kill();
            return;
        }
        base.AI();
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        /*float delta = Main.rand.NextFloat(0.85f, 1.15f) * (damageDone / MathHelper.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue));
        player.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake += delta * .15f;//
        for (int n = 0; n < 30 * delta * (StandardInfo.dustAmount + .2f); n++)
            OtherMethods.FastDust(target.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), StandardInfo.standardColor);*/
        base.OnHitNPC(target, hit, damageDone);
    }
}
public class DonQuijoteDeLaManchaPlayer : ModPlayer
{
    public bool NextHitImmune;//冲锋引起的一次伤害免疫
    public bool Dashing;
    public override bool FreeDodge(Player.HurtInfo info)
    {
        if (NextHitImmune && Dashing)
        {
            NextHitImmune = false;
            Player.immune = true;
            //Player.immuneTime = 2;
            return true;
        }
        return false;
    }
    public int DashCoolDown;
    public int DashCoolDownMax;
    public int StabTimeLeft;
    public Vector2 startPoint;
    public ItemDefinition itemDefinition = new();
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
    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        if (Dashing)
        {
            damage *= (1 + 0.5f * MathF.Log((Player.Center - startPoint).Length() / 16f + 1));//
        }
        base.ModifyWeaponDamage(item, ref damage);
    }
    public override void UpdateDead()
    {
        Dashing = false;
        base.UpdateDead();
    }
    public override void Load()
    {
        SequenceSystem.entityConditions.Add("DonQuijoteDeLaManchaStabing", entity => entity is Player plr && plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>().StabTimeLeft > 0);
        SequenceSystem.FastAddStandardEntityCondition("Mods.MatterRecord.Condition.DonQuijoteDeLaManchaStabing");

        /*SequenceSystem.entityConditions.Add("DonQuijoteDeLaManchaDashCoolDownFinished", entity => entity is Player plr && plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>().DashCoolDown <= 0);
        SequenceSystem.FastAddStandardEntityCondition("Mods.MatterRecord.Condition.DonQuijoteDeLaManchaDashCoolDownFinished");*/
        base.Load();
    }
    public override void ResetEffects()
    {
        if (StabTimeLeft > 0)
            StabTimeLeft--;
        if (DashCoolDown > 0)
            DashCoolDown--;
        base.ResetEffects();
    }
    public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
    {
        if (Dashing)
        {
            DashCoolDown += 30;
            DashCoolDownMax += 30;
        }

        base.OnHitByNPC(npc, hurtInfo);
    }
    public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
    {
        if (Dashing)
        {
            DashCoolDown += 30;
            DashCoolDownMax += 30;
        }

        base.OnHitByProjectile(proj, hurtInfo);
    }
    /*public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (DashCoolDown > 0)
        {
            //DrawData drawData = new DrawData(TextureAssets.Projectile[ModContent.ProjectileType<DonQuijoteDeLaManchaProj>()].Value,Player.Center - Main.screenPosition,nu);
            //drawInfo.DrawDataCache.Add(drawData);
            Main.spriteBatch.DrawString(FontAssets.MouseText.Value, $"冲刺冷却{DashCoolDown / 60f:0.0}/{DashCoolDownMax / 60f:0.0}", Player.Center - Main.screenPosition + new Vector2(-48, -54), Color.White);
        }
        base.ModifyDrawInfo(ref drawInfo);
    }*/
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (DashCoolDown > 0 && Main.myPlayer == Player.whoAmI && !Player.DeadOrGhost)
        {
            Vector2 cen = Player.Center + Player.gfxOffY * Vector2.UnitY - Main.screenPosition - new Vector2(16, 160);
            drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/DonQuijoteDeLaMancha/DashCooldown_Recover").Value, cen, null, Color.White, 0, new Vector2(), 1f, 0));

            drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/DonQuijoteDeLaMancha/DashCooldown").Value, cen, new Rectangle(0, 0, 32, (int)(32f * DashCoolDown / DashCoolDownMax)), Color.White, 0, new Vector2(), 1f, 0));
            string text = $"冲刺冷却{DashCoolDown / 60f:0.0}/{DashCoolDownMax / 60f:0.0}";
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, text, cen + new Vector2(16, 48), Color.White, Color.Black, 0, FontAssets.MouseText.Value.MeasureString(text) * .5f, Vector2.One);
            //Main.spriteBatch.DrawString(FontAssets.MouseText.Value,, cen + Vector2.UnitY * 48, Color.White);
        }
        base.ModifyDrawInfo(ref drawInfo);
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
    static Asset<Texture2D> wheelTex;

    public override void SetStaticDefaults()
    {
        wheelTex = ModContent.Request<Texture2D>("MatterRecord/Contents/DonQuijoteDeLaMancha/WindMill_Wheel");
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {
        Projectile.timeLeft = 180;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.friendly = true;
        Projectile.width = 106;
        Projectile.height = 136;
        base.SetDefaults();
    }
    public override void AI()
    {
        Projectile.ai[1] += Projectile.ai[0];
        base.AI();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        lightColor *= MathHelper.SmoothStep(0, 1, (90 - MathF.Abs(90 - Projectile.timeLeft)) / 10f);
        return base.PreDraw(ref lightColor);
    }
    public override void PostDraw(Color lightColor)
    {
        lightColor *= MathHelper.SmoothStep(0, 1, (90 - MathF.Abs(90 - Projectile.timeLeft)) / 10f);
        Main.spriteBatch.Draw(wheelTex.Value, Projectile.Center + new Vector2(0, -12) - Main.screenPosition, null, lightColor, Projectile.ai[1], new Vector2(53, 55), 1f, 0, 0);
        base.PostDraw(lightColor);
    }
}
