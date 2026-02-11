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
        Main.npcFrameCount[Type] = 23; // The total amount of frames the NPC has

        NPCID.Sets.ExtraFramesCount[Type] = 9; // Generally for Town NPCs, but this is how the NPC does extra things such as sitting in a chair and talking to other NPCs. This is the remaining frames after the walking frames.
        NPCID.Sets.AttackFrameCount[Type] = 4; // The amount of frames in the attacking animation.
        NPCID.Sets.DangerDetectRange[Type] = 700; // The amount of pixels away from the center of the NPC that it tries to attack enemies.
        NPCID.Sets.AttackType[Type] = 1; // The type of attack the Town NPC performs. 0 = throwing, 1 = shooting, 2 = magic, 3 = melee
        NPCID.Sets.AttackTime[Type] = 60; // The amount of time it takes for the NPC's attack animation to be over once it starts.
        NPCID.Sets.AttackAverageChance[Type] = 1; // The denominator for the chance for a Town NPC to attack. Lower numbers make the Town NPC appear more aggressive.
        NPCID.Sets.HatOffsetY[Type] = 4;
        // Influences how the NPC looks in the Bestiary
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers()
        {
            Velocity = 1f, // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
            Direction = 1 // -1 is left and 1 is right. NPCs are drawn facing the left by default but ExamplePerson will be drawn facing the right
                          // Rotation = MathHelper.ToRadians(180) // You can also change the rotation of an NPC. Rotation is measured in radians
                          // If you want to see an example of manually modifying these when the NPC is drawn, see PreDraw
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

        // 喜欢海洋
        // 反感地狱
        // 喜爱护士
        // 喜欢派对女孩
        // 憎恶渔夫
        NPC.Happiness
            .SetBiomeAffection<OceanBiome>(AffectionLevel.Like)
            .SetBiomeAffection<UnderworldBiome>(AffectionLevel.Dislike)
            .SetNPCAffection(NPCID.Nurse, AffectionLevel.Love)
            .SetNPCAffection(NPCID.PartyGirl, AffectionLevel.Like)
            .SetNPCAffection(NPCID.Angler, AffectionLevel.Hate)
        ;
        ContentSamples.NpcBestiaryRarityStars[Type] = 3;
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true; // Sets NPC to be a Town NPC
        NPC.friendly = true; // NPC Will not attack player
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
        // We can use AddRange instead of calling Add multiple times in order to add multiple items at once
        bestiaryEntry.Info.AddRange([
            // Sets the preferred biomes of this town NPC listed in the bestiary.
            // With Town NPCs, you usually set this to what biome it likes the most in regards to NPC happiness.
            BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,

				// Sets your NPC's flavor text in the bestiary. (use localization keys)
				new FlavorTextBestiaryInfoElement("Mods.MatterRecord.Bestiary.Recorder")
        ]);
    }

    // The PreDraw hook is useful for drawing things before our sprite is drawn or running code before the sprite is drawn
    // Returning false will allow you to manually draw your NPC
    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {

        return true;
    }


    public override bool CanTownNPCSpawn(int numTownNPCs) => true;


    public override List<string> SetNPCNameList()
    {
        return [
                this.GetLocalizedValue("Name")
            ];
    }

    public override bool CanGoToStatue(bool toKingStatue) => true;


    public override void TownNPCAttackStrength(ref int damage, ref float knockback)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            var player = Main.LocalPlayer;
            float rangeFactor = player.GetTotalDamage(DamageClass.Ranged).ApplyTo(1f);
            float genericFactor = player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f);

            rangeFactor -= genericFactor;
            genericFactor += rangeFactor - 1;

            var critFactor = player.GetTotalCritChance(DamageClass.Ranged) * .01f;
            critFactor += .04f;

            //int defense = player.statDefense;
            int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;

            damage = (int)Math.Max(defense * (0.75f + rangeFactor + critFactor) / (1 + genericFactor * .5f), 1);
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

                //int defense = player.statDefense;
                int defense = player.armor[0].defense + player.armor[1].defense + player.armor[2].defense;

                int curDamage = (int)(Math.Max(defense * (0.75f + rangeFactor + critFactor) / (1 + genericFactor * .5f), 1));
                if (curDamage > damage)
                    damage = curDamage;
            }
        }
        damage = damage * 2 / 5;
        if (damage < 1) damage = 1;
        knockback = 4f;
    }

    public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
    {
        cooldown = 30;
        randExtraCooldown = 30;
    }

    public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
    {
        projType = ProjectileID.Bullet;
        attackDelay = 1;
    }
    public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset)
    {
        item = ModAsset.LordOfTheFlies.Value;
        itemFrame = new Rectangle(0, 0, item.Width, item.Height);
        horizontalHoldoutOffset = -8;
        base.DrawTownAttackGun(ref item, ref itemFrame, ref scale, ref horizontalHoldoutOffset);
    }
    public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
    {
        multiplier = 16f;

        // SparklingBall is not affected by gravity, so gravityCorrection is left alone.
    }
}
