using MatterRecord.Contents.Recorder;

namespace MatterRecord.Contents.TheTaleOfTheHeike;

// 饰品物品类
public class TheTaleOfTheHeike : RecordBookItem
{
    public override ItemRecords RecordType => ItemRecords.TheTaleOfTheHeike;
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
        float damageMultiplier = 1f + (1f - targetHpPercent) * 0.3f;
        modifiers.FinalDamage *= damageMultiplier;
    }

    // 受到伤害时，根据玩家当前生命百分比增加所受伤害
    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (!Equipped) return;

        float playerHpPercent = Player.statLife / (float)Player.statLifeMax2;
        float damageMultiplier = 1f + (1f - playerHpPercent) * 0.3f;
        modifiers.FinalDamage *= damageMultiplier;
    }
}