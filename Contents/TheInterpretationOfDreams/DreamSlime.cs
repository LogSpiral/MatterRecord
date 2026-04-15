using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using MatterRecord.Contents.Recorder;
using RecorderClass = MatterRecord.Contents.Recorder.Recorder;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

[AutoloadHead]
public class DreamSlime : ModNPC
{
    // ========== 常量 ==========
    private const float SEARCH_RANGE = 520f;           // 索敌范围
    private const float RANDOM_HOP_CHANCE = 0.33f;     // 随机跳跃概率
    private const float SPLIT_CHANCE = 0.5f;           // 分裂概率
    private const int MAX_SLIME_COUNT = 3;             // 分裂上限
    private const float GRAVITY = 0.5f;                // 重力加速度（像素/帧²）
    private const int AID_DELAY_MIN = 60;              // 援助倒计时最小帧数
    private const int AID_DELAY_MAX = 181;             // 援助倒计时最大帧数
    private const float JUMP_BASE_POWER = -7f;          // 基础跳跃速度
    private const float JUMP_MIN_POWER = -4f;           // 最小跳跃速度
    private const float JUMP_EXTRA_FACTOR_UP = 0.025f;  // 目标在上方时额外加成系数
    private const float JUMP_EXTRA_FACTOR_DOWN = 0.03f; // 目标在下方时减少系数
    private const float JUMP_MAX_EXTRA = 10f;           // 最大额外速度
    private const float JUMP_MAX_REDUCE = 3f;           // 最大减少量
    private const int GROUND_CHECK_HEIGHT = 4;          // 接地检测额外高度（像素）
    private const float SIDE_WALL_CHECK_OFFSET = 4f;    // 侧向墙检测偏移（像素）
    private const float WALL_COLLIDE_SPEED_THRESHOLD = 0.1f;

    // ========== 字段 ==========
    private int aidTimer = 0;
    private int contactProjIndex = -1;

    // ========== 原版覆盖 ==========
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 14;
        NPCID.Sets.ExtraFramesCount[Type] = 0;
        NPCID.Sets.AttackFrameCount[Type] = 0;
        NPCID.Sets.DangerDetectRange[Type] = 250;
        NPCID.Sets.AttackType[Type] = -1;
        NPCID.Sets.AttackTime[Type] = -1;
        NPCID.Sets.AttackAverageChance[Type] = 1;
        NPCID.Sets.HatOffsetY[Type] = -2;
        NPCID.Sets.ShimmerTownTransform[Type] = false;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Shimmer] = true;
        NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        NPCID.Sets.ExtraTextureCount[Type] = 0;
        NPCID.Sets.NPCFramingGroup[Type] = 8;
        NPCID.Sets.IsTownPet[Type] = true;
        NPCID.Sets.CannotSitOnFurniture[Type] = false;
        NPCID.Sets.TownNPCBestiaryPriority.Add(Type);
        NPCID.Sets.PlayerDistanceWhilePetting[Type] = 32;
        NPCID.Sets.IsPetSmallForPetting[Type] = true;

        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Velocity = 0.25f };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
    }

    public override void SetDefaults()
    {
        NPC.townNPC = true;
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
        NPC.housingCategory = 1;
        AnimationType = NPCID.TownSlimeBlue;
        AIType = NPCID.TownSlimeBlue;
    }

    public override bool CanTownNPCSpawn(int numTownNPCs) => DreamWorld.UsedZoologistDream;
    public override List<string> SetNPCNameList() => [this.GetLocalizedValue("Name")];
    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        => bestiaryEntry.Info.AddRange([BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, new FlavorTextBestiaryInfoElement("Mods.MatterRecord.Bestiary.DreamSlime")]);
    public override string GetChat() => Language.GetTextValue($"Mods.MatterRecord.Dialogue.DreamSlime.Chatter_{Main.rand.Next(1, 4)}");
    public override void SetChatButtons(ref string button, ref string button2) => button = Language.GetTextValue("UI.PetTheAnimal");

    public override bool PreAI()
    {
        DrawOffsetY = (NPC.ai[0] == 5f) ? -10 : 0;
        return base.PreAI();
    }

    public override void ChatBubblePosition(ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        if (NPC.ai[0] == 5f) position.Y -= 18f;
    }

    public override void PartyHatPosition(ref Vector2 position, ref SpriteEffects spriteEffects)
    {
        int frame = NPC.frame.Y / NPC.frame.Height;
        int xOffset = 8;
        switch (frame)
        {
            case 1: case 2: case 7: case 9: xOffset -= 2; break;
            case 3: case 8: xOffset -= 4; break;
            case 11: case 15: case 16: case 17: case 26: xOffset += 2; break;
            case 12: case 13: case 14: case 18: case 24: case 25: xOffset += 4; break;
            case 19: case 20: case 21: case 22: case 23: xOffset += 6; break;
        }
        position.X += xOffset * NPC.spriteDirection;

        int yOffset = 0;
        switch (frame)
        {
            case 3: case 4: yOffset -= 2; break;
            case 5: case 6: case 10: case 17: case 26: yOffset += 2; break;
            case 18: case 25: yOffset += 4; break;
            case 11: case 15: case 16: case 24: yOffset += 6; break;
            case 19: case 20: case 23: yOffset += 8; break;
            case 21: case 22: yOffset += 10; break;
            case 12: case 14: yOffset += 12; break;
            case 13: yOffset += 14; break;
        }
        position.Y += yOffset;
        if (NPC.ai[0] == 5f) position.Y += -10;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        => base.PreDraw(spriteBatch, screenPos, drawColor);

    // ========== 主要 AI ==========
    public override void AI()
    {
        bool hasEnemy = CheckHasEnemy();

        if (hasEnemy && NPC.aiStyle != -1)
            EnterCombat();
        else if (!hasEnemy && NPC.aiStyle == -1 && !AnyBossExists())
            ExitCombat();

        if (NPC.aiStyle == -1)
        {
            NPC.defense = 9999;
            NPC.knockBackResist = 0f;
            HandleNoTileCollideDuringFall(); // 下落穿平台及防卡墙
            JumpToEnemy();
        }
        else
        {
            base.AI();
            HandleAidTeleport(); // 援助传送
        }
    }

    // ========== 战斗状态管理 ==========
    private void EnterCombat()
    {
        if (NPC.aiStyle == -1) return;
        NPC.aiStyle = -1;
        NPC.defense = 9999;
        NPC.knockBackResist = 0f;
        NPC.velocity = Vector2.Zero;
        aidTimer = 0;
        SpawnContactProjectile();
    }

    private void ExitCombat()
    {
        bool anyOtherInCombat = false;
        for (int i = 0; i < Main.npc.Length; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.type == Type && npc.whoAmI != NPC.whoAmI && npc.aiStyle == -1)
            {
                anyOtherInCombat = true;
                break;
            }
        }
        if (!anyOtherInCombat) RemoveExtraSlimes();

        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.defense = 15;
        NPC.knockBackResist = 0.5f;
        NPC.velocity = Vector2.Zero;
        aidTimer = 0;
        KillContactProjectile();
        NPC.noTileCollide = false;
    }

    // ========== 下落穿平台与防卡墙 ==========
    private void HandleNoTileCollideDuringFall()
    {
        NPC target = FindNearestEnemy(NPC);
        if (target == null)
        {
            NPC.noTileCollide = false;
            return;
        }

        bool targetBelow = target.Center.Y > NPC.Center.Y;
        bool isFalling = NPC.velocity.Y > 0;

        if (targetBelow && isFalling)
        {
            NPC.noTileCollide = true;

            bool hasGround = IsSolidNonPlatformTile(NPC.Bottom + Vector2.UnitY);
            bool hasWall = HasSideWall();

            if (hasGround || hasWall)
            {
                NPC.noTileCollide = false;
                if (hasWall && Math.Abs(NPC.velocity.X) > WALL_COLLIDE_SPEED_THRESHOLD)
                    NPC.velocity.X = 0;
            }
            else if (NPC.Center.Y >= target.Center.Y)
            {
                NPC.noTileCollide = false;
            }
        }
        else
        {
            NPC.noTileCollide = false;
        }
    }

    private bool IsSolidNonPlatformTile(Vector2 worldPos)
    {
        int x = (int)(worldPos.X / 16);
        int y = (int)(worldPos.Y / 16);
        Tile tile = Framing.GetTileSafely(x, y);
        return tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !tile.IsActuated;
    }

    private bool HasSideWall()
    {
        int direction = (NPC.velocity.X > WALL_COLLIDE_SPEED_THRESHOLD) ? 1
                     : (NPC.velocity.X < -WALL_COLLIDE_SPEED_THRESHOLD) ? -1
                     : NPC.direction;
        if (direction == 0) return false;

        float offsetX = direction * (NPC.width / 2 + SIDE_WALL_CHECK_OFFSET);
        Vector2 checkPosCenter = NPC.Center + new Vector2(offsetX, 0);
        Vector2 checkPosBottom = NPC.Bottom + new Vector2(offsetX, 0);

        return IsSolidNonPlatformTile(checkPosCenter) || IsSolidNonPlatformTile(checkPosBottom);
    }

    // ========== 跳跃逻辑 ==========
    private void JumpToEnemy()
    {
        NPC target = FindNearestEnemy(NPC);
        if (target == null) return;

        bool isGrounded = NPC.collideY || IsBelowGround(NPC.BottomLeft, NPC.width, GROUND_CHECK_HEIGHT);

        if (isGrounded)
        {
            bool doRandomHop = Main.rand.NextFloat() < RANDOM_HOP_CHANCE;

            if (doRandomHop)
            {
                if (CanSplit()) TrySplit();
                float hSpeed = Main.rand.NextFloat(-2f, 2f);
                float vSpeed = -LimitJumpSpeedByCeiling(-Main.rand.NextFloat(4f, 6f));
                NPC.velocity = new Vector2(hSpeed, vSpeed);
                if (hSpeed != 0) NPC.direction = Math.Sign(hSpeed);
            }
            else
            {
                NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
                NPC.velocity.X = NPC.direction * Main.rand.NextFloat(1.5f, 3.5f);
                float jumpPower = CalculateJumpPower(target);
                NPC.velocity.Y = -LimitJumpSpeedByCeiling(-jumpPower);
            }
        }
        else
        {
            // 空中无额外动作
        }

        if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
            NPC.velocity.Y = -3f;
    }

    private bool IsBelowGround(Vector2 topLeft, int width, int checkHeight)
    {
        Rectangle rect = new Rectangle((int)topLeft.X, (int)topLeft.Y, width, checkHeight);
        return Collision.SolidCollision(rect.TopLeft(), rect.Width, rect.Height);
    }

    private float CalculateJumpPower(NPC target)
    {
        float verticalDiff = target.Center.Y - NPC.Center.Y;
        float power = JUMP_BASE_POWER;
        if (verticalDiff < 0)
        {
            float extra = Math.Min(-verticalDiff * JUMP_EXTRA_FACTOR_UP, JUMP_MAX_EXTRA);
            power -= extra;
        }
        else
        {
            float reduce = Math.Min(verticalDiff * JUMP_EXTRA_FACTOR_DOWN, JUMP_MAX_REDUCE);
            power += reduce;
            if (power > JUMP_MIN_POWER) power = JUMP_MIN_POWER;
        }
        return power;
    }

    // ========== 头顶物块限制 ==========
    private float LimitJumpSpeedByCeiling(float desiredUpSpeed)
    {
        if (desiredUpSpeed <= 0) return 0f;

        float npcTopY = NPC.position.Y;
        float npcCenterX = NPC.Center.X;
        float estimatedMaxHeight = (desiredUpSpeed * desiredUpSpeed) / (2f * GRAVITY);
        int maxCheckTiles = (int)(estimatedMaxHeight / 16f) + 2;

        int startTileX = (int)(npcCenterX / 16);
        int startTileY = (int)(npcTopY / 16);

        for (int i = 1; i <= maxCheckTiles; i++)
        {
            Tile tile = Framing.GetTileSafely(startTileX, startTileY - i);
            if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && !tile.IsActuated)
            {
                float ceilingY = (startTileY - i) * 16 + 16;
                float availableHeight = ceilingY - npcTopY;
                if (availableHeight <= 0) return 0f;
                float maxSpeed = (float)Math.Sqrt(2 * GRAVITY * availableHeight);
                return Math.Min(desiredUpSpeed, maxSpeed);
            }
        }
        return desiredUpSpeed;
    }

    // ========== 分裂 ==========
    private bool CanSplit()
    {
        int totalCount = 0;
        foreach (NPC npc in Main.npc)
            if (npc.active && npc.type == Type) totalCount++;
        return totalCount < MAX_SLIME_COUNT && Main.rand.NextFloat() < SPLIT_CHANCE;
    }

    private void TrySplit()
    {
        if (!CanSplit()) return;
        int newNPC = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Bottom.Y, Type);
        if (newNPC != Main.maxNPCs && Main.npc[newNPC].ModNPC is DreamSlime child)
        {
            child.NPC.velocity = Vector2.Zero;
            child.NPC.direction = NPC.direction;
            child.EnterCombat();
        }
    }

    // ========== 援助传送 ==========
    private void HandleAidTeleport()
    {
        if (aidTimer == -1) return;

        List<NPC> combatSlimes = new List<NPC>();
        foreach (NPC npc in Main.npc)
            if (npc.active && npc.type == Type && npc.aiStyle == -1)
                combatSlimes.Add(npc);

        bool hasCombatSlime = combatSlimes.Count > 0;
        bool recorderInCombat = RecorderClass.IsAnyRecorderInCombat();

        if (hasCombatSlime)
        {
            NPC frontline = GetFrontlineSlime(combatSlimes);
            if (aidTimer <= 0)
                aidTimer = Main.rand.Next(AID_DELAY_MIN, AID_DELAY_MAX);
            else if (--aidTimer == 0 && frontline != null)
                TeleportTo(frontline.Bottom);
        }
        else if (recorderInCombat)
        {
            if (aidTimer <= 0)
                aidTimer = Main.rand.Next(AID_DELAY_MIN, AID_DELAY_MAX);
            else if (--aidTimer == 0)
            {
                NPC recorder = FindRecorderNPC();
                if (recorder != null) TeleportTo(recorder.Bottom);
            }
        }
        else
        {
            aidTimer = 0;
        }
    }

    private NPC GetFrontlineSlime(List<NPC> slimes)
    {
        NPC best = null;
        float minDist = float.MaxValue;
        foreach (NPC slime in slimes)
        {
            NPC enemy = FindNearestEnemy(slime);
            if (enemy != null)
            {
                float dist = Vector2.Distance(slime.Center, enemy.Center);
                if (dist < minDist)
                {
                    minDist = dist;
                    best = slime;
                }
            }
            else if (best == null) best = slime;
        }
        return best ?? (slimes.Count > 0 ? slimes[0] : null);
    }

    private NPC FindRecorderNPC()
    {
        foreach (NPC npc in Main.npc)
            if (npc.active && npc.type == ModContent.NPCType<RecorderClass>())
                return npc;
        return null;
    }

    private void TeleportTo(Vector2 targetBottom)
    {
        NPC.position = targetBottom - new Vector2(NPC.width / 2, NPC.height);
        NPC.velocity = Vector2.Zero;
        NPC.netUpdate = true;
        SpawnDustCloud(NPC.position, NPC.width, NPC.height);
        aidTimer = -1;
    }

    // ========== 通用辅助 ==========
    private static NPC FindNearestEnemy(NPC searcher)
    {
        NPC nearest = null;
        float minDist = float.MaxValue;
        foreach (NPC enemy in Main.npc)
        {
            if (enemy.active && !enemy.friendly && enemy.lifeMax > 5 && !enemy.townNPC && enemy.type != NPCID.TargetDummy)
            {
                float dist = Vector2.Distance(searcher.Center, enemy.Center);
                if (dist < SEARCH_RANGE && dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
        }
        return nearest;
    }

    private bool CheckHasEnemy()
    {
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.townNPC && npc.type != NPCID.TargetDummy)
            {
                if (Vector2.Distance(NPC.Center, npc.Center) < SEARCH_RANGE)
                    return true;
            }
        }
        return false;
    }

    private void RemoveExtraSlimes()
    {
        List<NPC> slimes = new List<NPC>();
        foreach (NPC npc in Main.npc)
            if (npc.active && npc.type == Type)
                slimes.Add(npc);

        if (slimes.Count <= 1) return;
        NPC keeper = slimes[0];
        foreach (NPC slime in slimes)
            if (slime.whoAmI < keeper.whoAmI)
                keeper = slime;

        foreach (NPC slime in slimes)
        {
            if (slime != keeper)
            {
                SpawnDustCloud(slime.position, slime.width, slime.height);
                if (slime.ModNPC is DreamSlime dream) dream.KillContactProjectile();
                slime.active = false;
                slime.life = 0;
            }
        }
    }

    private void SpawnDustCloud(Vector2 pos, int width, int height, int dustCount = 15)
    {
        for (int i = 0; i < dustCount; i++)
            Dust.NewDust(pos, width, height, DustID.Cloud, 0f, 0f, 100, default, 1.5f);
    }

    // ========== 弹幕管理 ==========
    private void SpawnContactProjectile()
    {
        KillContactProjectile();
        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
            ModContent.ProjectileType<DreamSlimeContactProjectile>(), 0, 0f, Main.myPlayer, NPC.whoAmI);
        if (proj != Main.maxProjectiles) contactProjIndex = proj;
    }

    private void KillContactProjectile()
    {
        if (contactProjIndex != -1 && Main.projectile[contactProjIndex].active)
            Main.projectile[contactProjIndex].Kill();
        contactProjIndex = -1;
    }

    public override void OnKill() => KillContactProjectile();

    private bool AnyBossExists()
    {
        foreach (NPC npc in Main.npc)
            if (npc.active && npc.boss) return true;
        return false;
    }
}