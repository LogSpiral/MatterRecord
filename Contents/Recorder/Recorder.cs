using MatterRecord.Contents.ImperfectPage;  // 添加对 ImperfectPageSystem 的引用
using MatterRecord.Contents.LordOfTheFlies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;

namespace MatterRecord.Contents.Recorder;

[AutoloadHead]
public partial class Recorder : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 23;

        NPCID.Sets.ExtraFramesCount[Type] = 9;
        NPCID.Sets.AttackFrameCount[Type] = 4;
        NPCID.Sets.DangerDetectRange[Type] = 700;
        NPCID.Sets.AttackType[Type] = 1;
        NPCID.Sets.AttackTime[Type] = 60;
        NPCID.Sets.AttackAverageChance[Type] = 1;
        NPCID.Sets.HatOffsetY[Type] = 4;
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Velocity = 1f,
            Direction = 1
        };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        NPC.Happiness
            .SetBiomeAffection<OceanBiome>(AffectionLevel.Like)
            .SetBiomeAffection<UnderworldBiome>(AffectionLevel.Dislike)
            .SetNPCAffection(NPCID.Nurse, AffectionLevel.Love)
            .SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Like)
            .SetNPCAffection(NPCID.Angler, AffectionLevel.Hate);
        ContentSamples.NpcBestiaryRarityStars[Type] = 3;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 40;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.knockBackResist = 0.5f;
        AnimationType = NPCID.Steampunker;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.Info.AddRange([
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
            new FlavorTextBestiaryInfoElement("Mods.MatterRecord.Bestiary.Recorder")
        ]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return true;
    }

    public override bool CanTownNPCSpawn(int numTownNPCs) => true;

    public override List<string> SetNPCNameList()
    {
        return [this.GetLocalizedValue("Name")];
    }

    public override bool CanGoToStatue(bool toKingStatue) => true;

    // ==================== 攻击行为 ====================
    public override void TownNPCAttackStrength(ref int damage, ref float knockback)
    {
        // 原复杂逻辑保持不变
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            var player = Main.LocalPlayer;
            float rangeFactor = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
            float genericFactor = player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);
            rangeFactor -= genericFactor;
            genericFactor += rangeFactor - 1;
            var critFactor = player.GetTotalCritChance(DamageClass.Ranged) * .01f;
            critFactor += .04f;
            int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;
            damage = (int)Math.Max(defense * 0.85f, 12);
        }
        else
        {
            foreach (var player in Main.player)
            {
                if (!player.active) continue;
                float rangeFactor = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
                float genericFactor = player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);
                rangeFactor -= genericFactor;
                genericFactor += rangeFactor - 1;
                var critFactor = player.GetTotalCritChance(DamageClass.Ranged) * .01f;
                critFactor += .04f;
                int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;
                int curDamage = (int)(Math.Max(defense * 0.85f, 12));
                if (curDamage > damage)
                    damage = curDamage;
            }
        }
        if (damage < 1) damage = 1;
        knockback = 4f;
    }

    public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
    {
        cooldown = 0;
        randExtraCooldown = 0;
    }

    public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
    {
        // 5% 概率发射湮灭弹（保留原样）
        if (Main.rand.NextFloat() < 0.05f)
        {
            projType = ModContent.ProjectileType<AnnihilationBullet>();
        }
        else
        {
            // 优先使用世界数据中存储的弹药（通过 ImperfectPageSystem）
            int favoriteItemType = ModContent.GetInstance<ImperfectPageSystem>().FavoriteAmmoType;
            if (favoriteItemType > 0)
            {
                Item favoriteItem = ContentSamples.ItemsByType[favoriteItemType];
                if (favoriteItem != null && favoriteItem.ammo == AmmoID.Bullet && favoriteItem.shoot > 0)
                {
                    projType = favoriteItem.shoot;
                }

            }

        }
        attackDelay = 10;
    }



    public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        multiplier = 80f;
        randomOffset = 0f;
    }
    // ======================================================

    public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        item = ModAsset.LordOfTheFlies.Value;
        itemFrame = new Rectangle(0, 0, item.Width, item.Height);
        horizontalHoldoutOffset = -8;
        base.DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }
}