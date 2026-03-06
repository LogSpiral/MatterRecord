using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.Localization;

namespace MatterRecord.Contents.TheInterpretationOfDreams;


[AutoloadHead]
public class DreamSlime : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 14; // The number of frames our sprite has.
        NPCID.Sets.ExtraFramesCount[Type] = 0; // The number of frames after the walking frames.
        NPCID.Sets.AttackFrameCount[Type] = 0; // Town Pets don't have any attacking frames.
        NPCID.Sets.DangerDetectRange[Type] = 250; // How far away the NPC will detect danger. Measured in pixels.
        NPCID.Sets.AttackType[Type] = -1; // Town Pets do not attack. The default for this set is -1, so it is safe to remove this line if you wish.
        NPCID.Sets.AttackTime[Type] = -1; // Town Pets do not attack. The default for this set is -1, so it is safe to remove this line if you wish.
        NPCID.Sets.AttackAverageChance[Type] = 1;  // Town Pets do not attack. The default for this set is 1, so it is safe to remove this line if you wish.
        NPCID.Sets.HatOffsetY[Type] = -2; // An offset for where the party hat sits on the sprite.
        NPCID.Sets.ShimmerTownTransform[Type] = false; // Town Pets don't have a Shimmer variant.
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true; // But they are still immune to Shimmer.
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true; // And Confused.
        NPCID.Sets.ExtraTextureCount[Type] = 0; // Even though we have several variation textures, we don't use this set. The default for this set is 0, so it is safe to remove this line if you wish.
        NPCID.Sets.NPCFramingGroup[Type] = 8; // How the party hat is animated to match the walking animation. Town Cat = 4, Town Dog = 5, Town Bunny = 6, Town Slimes = 7, No offset = 8

        NPCID.Sets.IsTownPet[Type] = true; // Our NPC is a Town Pet
        NPCID.Sets.CannotSitOnFurniture[Type] = false; // True by default which means they cannot sit in chairs. True means they can sit on furniture like the Town Cat.
        NPCID.Sets.TownNPCBestiaryPriority.Add(Type); // Puts our NPC with all of the other Town NPCs.
        NPCID.Sets.PlayerDistanceWhilePetting[Type] = 32; // Distance the player stands from the Town Pet to pet.
        NPCID.Sets.IsPetSmallForPetting[Type] = true; // If set to true, the player's arm will be angled down while petting.

        // Influences how the NPC looks in the Bestiary
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
        {
            Velocity = 0.25f, // Draws the NPC in the bestiary as if its walking +0.25 tiles in the x direction
        };

        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);

    }

    public override void SetDefaults()
    {
        NPC.townNPC = true; // Town Pets are still considered Town NPCs
        NPC.friendly = true;
        NPC.width = 18;
        NPC.height = 20;
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 10;
        NPC.defense = 15;
        NPC.lifeMax = 250;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath6;
        NPC.knockBackResist = 0.5f;
        NPC.housingCategory = 1; // This means it can share a house with a normal Town NPC.
        AnimationType = NPCID.TownSlimeRainbow; // This example matches the animations of the Town Bunny.
    }
    public override bool CanTownNPCSpawn(int numTownNPCs) => DreamWorld.UsedZoologistDream;

    public override List<string> SetNPCNameList()
    {
        return ["梦境史莱姆(W.I.P.)"];
    }

    public override string GetChat()
    {
        return "*未完工史莱姆声*";
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        button = Language.GetTextValue("UI.PetTheAnimal"); // Automatically translated to say "Pet"
    }

    public override bool PreAI()
    {
        // If your Town Pet can sit in chairs with NPCID.Sets.CannotSitOnFurniture[Type] = false
        // We want to move the Town NPC up visually to match the height of the chair.
        // NPC.ai[0] is set to 5f for Town NPC AI when they are sitting in a chair.
        if (NPC.ai[0] == 5f)
        {
            DrawOffsetY = -10; // Remember: Negative Y is up. So, this is moving the NPC up visually by 10 pixels.
        }
        else
        {
            DrawOffsetY = 0; // Reset it back to 0 when not sitting in a chair.
        }
        // Do not try to add or subtract from the DrawOffsetY. It'll cause the sprite to change its height every frame which will make it go off of the screen.

        // If your Town Pet doesn't sit in furniture, you can remove this entire PreAI() method.

        return base.PreAI();
    }

    public override void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        // If your Town Pet can sit in chairs with NPCID.Sets.CannotSitOnFurniture[Type] = false
        // and you've done the above DrawOffsetY to raise it up to the chair's height,
        // you'll notice the chat bubble that appears when hovering over them doesn't get raised up.
        // So, let's move it up as well.
        if (NPC.ai[0] == 5f)
        { // (Sitting in a chair.)
            position.Y -= 18f; // Move upwards.
        }
    }

    public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        // With this hook, we have full control over the position of the party hat.
        // We have already set NPCID.Sets.HatOffsetY[Type] = -2 in SetStaticDefaults which will move the party hat up 2 pixels at all times.
        // We also set PCID.Sets.NPCFramingGroup[Type] = 8 in SetStaticDefaults.
        // NPCFramingGroup is used vertically offset the party hat to match the animations of the NPC.
        // Group 8 has no inherit offsets for the party hat.

        int frame = NPC.frame.Y / NPC.frame.Height; // The current frame.
        int xOffset = 8; // Move the party hat forward so it is actually on the Town Pet's head.
                         // Then move the party hat left/right depending on the frame.
                         // These numbers were achieved by measuring the sprite relative to the "normal" position of the party hat.
        switch (frame)
        {
            case 1:
            case 2:
            case 7:
            case 9:
                xOffset -= 2;
                break;
            case 3:
            case 8:
                xOffset -= 4;
                break;
            case 11:
            case 15:
            case 16:
            case 17:
            case 26:
                xOffset += 2;
                break;
            case 12:
            case 13:
            case 14:
            case 18:
            case 24:
            case 25:
                xOffset += 4;
                break;
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
                xOffset += 6;
                break;
            default:
                break;
        }
        position.X += xOffset * NPC.spriteDirection;

        // We set NPCID.Sets.HatOffsetY[Type] = -2 so that means every frame is moved up 2 additional units.
        int yOffset = 0;
        // Then move the party hat up/down depending on the frame.
        // These numbers were achieved by measuring the sprite relative to the "normal" position of the party hat.
        switch (frame)
        {
            case 3:
            case 4:
                yOffset -= 2;
                break;
            case 5:
            case 6:
            case 10:
            case 17:
            case 26:
                yOffset += 2;
                break;
            case 18:
            case 25:
                yOffset += 4;
                break;
            case 11:
            case 15:
            case 16:
            case 24:
                yOffset += 6;
                break;
            case 19:
            case 20:
            case 23:
                yOffset += 8;
                break;
            case 21:
            case 22:
                yOffset += 10;
                break;
            case 12:
            case 14:
                yOffset += 12;
                break;
            case 13:
                yOffset += 14;
                break;
            default:
                break;
        }
        position.Y += yOffset;

        // Move it up to match the location of the head when sitting in a chair.
        if (NPC.ai[0] == 5f)
        {
            position.Y += -10;
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        return base.PreDraw(spriteBatch, screenPos, drawColor);
    }
}