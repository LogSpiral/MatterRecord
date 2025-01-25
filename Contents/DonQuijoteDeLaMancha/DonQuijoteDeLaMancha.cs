using LogSpiralLibrary.CodeLibrary.DataStructures.Drawing;
using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures.Melee;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using LogSpiralLibrary.CodeLibrary.DataStructures.SequenceStructures;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha
{
    public class DonQuijoteDeLaMancha : MeleeSequenceItem<DonQuijoteDeLaManchaProj>
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.ShadowJoustingLance}";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 62;
            Item.height = 62;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.rare = ItemRarityID.Red;
            Item.UseSound = MySoundID.Scythe;
            Item.damage = 155;
            Item.knockBack = 14f;
        }
        public override bool AltFunctionUse(Player player)
        {
            //Main.NewText("111");
            var mplr = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
            if (mplr.StabTimeLeft > 0)
            {
                mplr.StabTimeLeft = 0;
                for (int n = 0; n < 30; n++) 
                {
                    Dust.NewDustPerfect(player.Center, MyDustId.PurpleLight, Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16));
                }
                CombatText.NewText(player.Hitbox with { Y = player.Hitbox.Y + 64 }, Color.MediumPurple, "解除突刺状态");
                return false;
            }
            if (mplr.DashCoolDown > 0)
                return false;
            return true;
        }
    }
    public class DonQuijoteDeLaManchaProj : MeleeSequenceProj
    {
        public override StandardInfo StandardInfo => base.StandardInfo with
        {
            standardColor = new Color(65, 15, 140)/* * (currentData?.Factor ?? 1)*/,
            //standardGlowTexture = ModContent.Request<Texture2D>(GlowTexture).Value,
            standardTimer = player.itemAnimationMax,//(int)( * (player.HasBuff<DoggoBoost>() ? 0.8f : 1f))
            vertexStandard = Main.netMode == NetmodeID.Server ? default : new VertexDrawInfoStandardInfo() with
            {
                active = true,
                //heatMap = ModContent.Request<Texture2D>("DoggosDoggoDoggo/Shaders/HeatMap_Doggo6", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value,
                //renderInfos = [[DoggosConfig.Instance.distortConfigs.effectInfo], [DoggosConfig.Instance.maskConfigs.maskEffectInfo, DoggosConfig.Instance.bloomConfigs.effectInfo]],
                //renderInfos = [[DoggosConfig.Instance.maskConfigs.maskEffectInfo, DoggosConfig.Instance.bloomConfigs.effectInfo], [DoggosConfig.Instance.distortConfigs.effectInfo]],

                scaler = 120,
                timeLeft = 10,
                colorVec = new Vector3(0, 1, 0),
                alphaFactor = 2f
            },
            itemType = ModContent.ItemType<DonQuijoteDeLaMancha>()
        };
        public override string Texture => $"Terraria/Images/Item_{ItemID.ShadowJoustingLance}";

        class DonQuijoteDeLaManchaDash : MeleeAction
        {
            public override bool Attacktive => Factor < .85f;
            bool givenUp;
            Vector2 originVelocity;
            public override void OnEndAttack()
            {
                Owner.velocity *= MathHelper.Clamp(.07f * (1 + MathF.Sqrt(originVelocity.Length())), 0, 1);
                Owner.velocity += originVelocity * MathF.Pow(0.9f, originVelocity.Length());//
                if (Owner is Player plr)
                {
                    var mplr = plr.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
                    mplr.StabTimeLeft = 300;
                    mplr.NextHitImmune = true;
                }    
                base.OnEndAttack();
            }
            public override void OnAttack()
            {
                Owner.velocity += targetedVector * .25f * (1 - Factor).HillFactor2();
                if (Owner is Player plr)
                {
                    plr.GetModPlayer<LogSpiralLibraryPlayer>().strengthOfShake += 4f;
                    plr.GetModPlayer<LogSpiralLibraryPlayer>().ultraFallEnable = true;
                    if (!plr.controlUseTile && !plr.controlUseItem && !givenUp)
                    {
                        timer = 2;
                        givenUp = true;
                    }
                    //plr.velocity += targetedVector * 0.05f;
                    plr.immune = true;
                    plr.immuneTime = 6;

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
                    current.timeLeft = (byte)MathHelper.Lerp(current.timeLeftMax, 0, Terraria.Utils.GetLerpValue(0.25f, 1, Factor, true));
                    current.xScaler = 2 + Owner.velocity.Length() / 32;
                    current.scaler = standardInfo.vertexStandard.scaler * ModifyData.actionOffsetSize * offsetSize / 3 * 4f * current.xScaler;
                    current.center = Owner.Center - Rotation.ToRotationVector2() * current.scaler * .5f;
                    current.rotation = Rotation;
                    current.negativeDir = flip;
                }
                base.OnAttack();
            }
            public override void OnStartSingle()
            {
                givenUp = false;
                KValue = 1f;
                base.OnStartSingle();
            }
            public override void OnStartAttack()
            {
                SoundEngine.PlaySound(SoundID.Item92);
                var rand = Main.rand.NextFloat(0.25f, 0.5f);
                originVelocity = Owner.velocity;
                //if (Owner is Player plr) plr.AddBuff(ModContent.BuffType<DoggoBoost>(), 180);
                for (int k = 0; k < 60; k++)
                {
                    var vec = ((k * MathHelper.TwoPi / 60f).ToRotationVector2() * 15 * new Vector2(rand, 1)).RotatedBy(Rotation);
                    //DoggoDust(Owner.Center, vec - targetedVector);
                    //DoggoDust(Owner.Center, vec * 2 - targetedVector * .5f);

                }
                var verS = standardInfo.vertexStandard;
                if (verS.active)
                {
                    var u = UltraStab.NewUltraStab(standardInfo.standardColor, verS.timeLeft, verS.scaler * ModifyData.actionOffsetSize * offsetSize / 3 * 8f, Owner.Center - targetedVector, verS.heatMap, flip, Rotation, 2, colorVec: verS.colorVec);
                    if (verS.renderInfos == null)
                        u.ResetAllRenderInfo();
                    else
                    {
                        u.ModityAllRenderInfo(verS.renderInfos);
                    }
                    u.ApplyStdValueToVtxEffect(standardInfo);
                    current = u;

                }
                base.OnStartAttack();
            }
            UltraStab current;
        }
        class DonQuijoteDeLaManchaStab : RapidlyStabInfo
        {
            public override void OnEndSingle()
            {
                Owner.velocity += targetedVector.SafeNormalize(default) * 2;
                base.OnEndSingle();
            }
            public override void OnEndAttack()
            {
                SoundEngine.PlaySound(SoundID.Item96 with { MaxInstances =-1,Volume = 0.25f * SoundID.Item96.Volume});
                base.OnEndAttack();
            }
        }
    }
    public class DonQuijoteDeLaManchaPlayer : ModPlayer
    {
        public bool NextHitImmune;//冲锋引起的一次伤害免疫
        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (NextHitImmune) 
            {
                NextHitImmune = false;
                Player.immune = true;
                Player.immuneTime = 15;
                return true;
            }
            return false;
        }
        public int DashCoolDown;
        public int StabTimeLeft;
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
            if(DashCoolDown > 0)
                DashCoolDown--;
            base.ResetEffects();
        }
    }
}
