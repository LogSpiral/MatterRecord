using MatterRecord.Contents.Recorder;
using System.Collections.Generic;

namespace MatterRecord.Contents.TheTaleOfTheHeike;

// 饰品物品类
public class TheTaleOfTheHeike : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheTaleOfTheHeike;
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!this.IsRecordUnlocked) return;
        player.GetModPlayer<TheTaleOfTheHeikePlayer>().Equipped = true;
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.Muramasa);
    }
}

// 玩家效果类
public class TheTaleOfTheHeikePlayer : ModPlayer
{
    public bool Equipped;

    public override void ResetEffects()
    {
        Equipped = false;
    }

    // 对敌人造成伤害时，根据目标当前生命百分比增加伤害
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (!Equipped) return;

        float targetHpPercent = target.life / (float)target.lifeMax;
        float damageMultiplier = 1f + (1f - targetHpPercent) * 0.25f;
        modifiers.FinalDamage *= damageMultiplier;
    }

    // 受到伤害时，根据玩家当前生命百分比增加所受伤害
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (!Equipped) return;

        float playerHpPercent = Player.statLife / (float)Player.statLifeMax2;
        float damageMultiplier = 1f + (1f - playerHpPercent) * 0.25f;
        modifiers.FinalDamage *= damageMultiplier;
    }

    public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (item.type != ItemID.Muramasa) return;
        if (target.life > 0) return;
        if (!DungeonEnermies.Contains(target.type)) return;
        if (!RecorderSystem.ShouldSpawnRecordItem<TheTaleOfTheHeike>()) return;

        Player.QuickSpawnItem(target.GetSource_Loot(), ModContent.ItemType<TheTaleOfTheHeike>());
        RecorderSystem.SetCooldown<TheTaleOfTheHeike>();
    }


    private static HashSet<int> DungeonEnermies { get; } =
        [
            NPCID.SkeletronHand,
            NPCID.SkeletronHead,

            NPCID.AngryBones,
            NPCID.AngryBonesBig,
            NPCID.AngryBonesBigHelmet,
            NPCID.AngryBonesBigMuscle,

            NPCID.DarkCaster,

            NPCID.CursedSkull,

            NPCID.DungeonSlime,

            NPCID.DungeonGuardian,

            NPCID.BlueArmoredBones,
            NPCID.BlueArmoredBonesMace,
            NPCID.BlueArmoredBonesNoPants,
            NPCID.BlueArmoredBonesSword,

            NPCID.RustyArmoredBonesAxe,
            NPCID.RustyArmoredBonesFlail,
            NPCID.RustyArmoredBonesSword,
            NPCID.RustyArmoredBonesSwordNoArmor,

            NPCID.HellArmoredBones,
            NPCID.HellArmoredBonesMace,
            NPCID.HellArmoredBonesSpikeShield,
            NPCID.HellArmoredBonesSword,

            NPCID.Paladin,

            NPCID.Necromancer,
            NPCID.NecromancerArmored,

            NPCID.RaggedCaster,
            NPCID.RaggedCasterOpenCoat,

            NPCID.DiabolistRed,
            NPCID.DiabolistWhite,

            NPCID.SkeletonCommando,
            NPCID.SkeletonSniper,
            NPCID.TacticalSkeleton,
            NPCID.GiantCursedSkull,
            NPCID.BoneLee,
            NPCID.DungeonSpirit
        ];
}