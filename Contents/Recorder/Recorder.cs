using MatterRecord.Contents.ImperfectPage;  // 添加对 ImperfectPageSystem 的引用
using MatterRecord.Contents.LordOfTheFlies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.Chat;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Personalities;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace MatterRecord.Contents.Recorder;

[AutoloadHead]
public partial class Recorder : ModNPC
{
    public override void Load()
    {
        IL_WorldGen.SpawnTownNPC += RecorderArriveModify;
    }

    private static void RecorderArriveModify(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(c => c.MatchLdsfld(typeof(Main).GetField(nameof(Main.netMode), BindingFlags.Static | BindingFlags.Public)))) return;
        var index = cursor.Index;

        if (!cursor.TryGotoNext(c => c.MatchBr(out _))) return;
        if (!cursor.Next.MatchBr(out var label)) return;

        cursor.Index = index;
        cursor.EmitLdloc(11);
        cursor.EmitDelegate<Func<int,bool>>(i => 
        {
            var npc = Main.npc[i];
            if (npc.type == ModContent.NPCType<Recorder>())
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText(Language.GetTextValue("LegacyMultiplayer.19", npc.FullName), new Color(255, 240, 20));
                else if (Main.netMode == NetmodeID.Server)
                    ChatHelper.BroadcastChatMessage(NetworkText.FromKey("LegacyMultiplayer.19", npc.GetFullNetName()), new Color(255, 240, 20));
                return true;
            }
            return false;
        });
        cursor.EmitBrtrue(label);
    }

    public override void Unload()
    {
        IL_WorldGen.SpawnTownNPC -= RecorderArriveModify;
    }
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

        InitializeLocalization();
        InteractionRegister();

    }
    public override bool ModifyDeathMessage(ref NetworkText customText, ref Color color)
    {
        customText = NetworkText.FromKey("LegacyMultiplayer.20", NPC.GivenName);
        color = new Color(255, 240, 20);
        return true;
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
            new RecorderFlavorTextElement()
        ]);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        UpdateChat();
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
        // 5% 概率发射湮灭弹
        if (Main.rand.NextFloat() < 0.05f)
        {
            projType = ModContent.ProjectileType<AnnihilationBullet>();
        }
        else
        {
            int favoriteItemType = ModContent.GetInstance<ImperfectPageSystem>().FavoriteAmmoType;
            projType = ProjectileID.Bullet; // 默认火枪子弹

            if (favoriteItemType > 0)
            {
                Item favoriteItem = ContentSamples.ItemsByType[favoriteItemType];
                if (favoriteItem != null && favoriteItem.ammo == AmmoID.Bullet && favoriteItem.shoot > ProjectileID.None)
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

    // ==================== 战斗状态判定 ====================
    /// <summary>
    /// 判断当前记录者是否处于战斗状态（索敌范围内存在敌人）
    /// </summary>
    public bool IsInCombat()
    {
        float range = 520; 
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && !npc.friendly && npc.damage > 0 && !npc.townNPC && npc.type != NPCID.TargetDummy)
            {
                if (Vector2.Distance(NPC.Center, npc.Center) < range)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 静态方法：检查世界中是否存在任意记录者处于战斗状态
    /// </summary>
    public static bool IsAnyRecorderInCombat()
    {
        foreach (NPC npc in Main.npc)
        {
            if (npc.active && npc.type == ModContent.NPCType<Recorder>())
            {
                if (npc.ModNPC is Recorder recorder && recorder.IsInCombat())
                    return true;
            }
        }
        return false;
    }
    // ======================================================
}
public class RecorderFlavorTextElement : FlavorTextBestiaryInfoElement, IBestiaryInfoElement, ICategorizedBestiaryInfoElement
{
    private bool _clicked;
    private int _textCounter = 0;
    private static string _warn;
    public RecorderFlavorTextElement() : base("")
    {

    }
    public UIBestiaryEntryInfoPage.BestiaryInfoCategory ElementCategory => UIBestiaryEntryInfoPage.BestiaryInfoCategory.FlavorText;

    public new UIElement ProvideUIElement(BestiaryUICollectionInfo info)
    {
        if (info.UnlockState < BestiaryEntryUnlockState.CanShowStats_2)
        {
            return null;
        }
        _clicked = false;
        _textCounter = 0;
        _warn = Language.GetTextValue("Mods.MatterRecord.Bestiary.Recorder.Warn");

        UIPanel obj = new UIPanel(Main.Assets.Request<Texture2D>("Images/UI/Bestiary/Stat_Panel"), null, 12, 7)
        {
            Width = new StyleDimension(-11f, 1f),
            Height = new StyleDimension(109f, 0f),
            BackgroundColor = new Color(43, 56, 101),
            BorderColor = Color.Transparent,
            Left = new StyleDimension(3f, 0f),
            PaddingLeft = 4f,
            PaddingRight = 4f
        };
        UIText UITextFlavor = new UIText(Language.GetTextValue("Mods.MatterRecord.Bestiary.Recorder.Common"), 0.8f)
        {
            HAlign = 0f,
            VAlign = 0f,
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            IsWrapped = true
        };

        UIText UITextWarn = new UIText("", 0.8f)
        {
            HAlign = 0f,
            VAlign = 0f,
            Width = StyleDimension.FromPixelsAndPercent(0f, 1f),
            Height = StyleDimension.FromPixelsAndPercent(0f, 1f),
            IsWrapped = true
        };
        UITextWarn.SetText(Language.GetText("Mods.MatterRecord.Bestiary.Recorder.Request").Format(Main.gameMenu ? "" : Main.LocalPlayer.name ?? ""));
        UITextWarn.OnLeftClick += delegate
        {
            _clicked = true;
            UITextWarn.TextColor = Color.Red;
        };
        UITextWarn.OnUpdate += delegate
        {
            if (!_clicked)
            {
                UITextWarn.TextColor = Color.Lerp(UITextWarn.TextColor, UITextWarn.IsMouseHovering ? Color.White : Color.Gray, 0.1f);
                return;
            }
            _textCounter++;
            int index = _textCounter / 10;
            if (_textCounter % 10 == 0 && index <= _warn.Length)
                UITextWarn.SetText(_warn[0..index]);
        };
        UITextWarn.Top = new StyleDimension(UITextFlavor.MinHeight.Pixels, 0);
        AddDynamicResize(obj, UITextFlavor, UITextWarn);
        obj.Append(UITextFlavor);
        obj.Append(UITextWarn);

        return obj;
    }

    public static void AddDynamicResize(UIElement container, UIText text, UIText text2)
    {
        text.OnInternalTextChange += delegate
        {
            text2.Top = new StyleDimension(text.MinHeight.Pixels, 0);
            container.Height = new StyleDimension(text.MinHeight.Pixels + text2.MinHeight.Pixels, 0f);
        };
        text2.OnInternalTextChange += delegate
        {
            container.Height = new StyleDimension(text.MinHeight.Pixels + text2.MinHeight.Pixels, 0f);
        };
    }
}