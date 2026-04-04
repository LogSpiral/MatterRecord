using MatterRecord.Contents.WarAndPeace;

public class WarAndPeacePlayer : ModPlayer
{
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Player.HasBuff<Peace>())
        {
            float reduction = Player.statDefense * 0.1f;   // 防御值 10% 的减免
            modifiers.FinalDamage.Flat -= reduction;
        }
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (Player.HasBuff<War>())
        {
            Item weapon = Player.HeldItem;
            if (weapon != null && !weapon.IsAir && weapon.damage > 0)
            {
                float extraDamage = weapon.damage * 0.1f;   // 武器基础伤害的 10%
                modifiers.FlatBonusDamage += extraDamage;
            }
        }
    }
}