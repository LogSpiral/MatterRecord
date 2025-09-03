namespace MatterRecord.Contents.LordOfTheFlies;

public class RangedMagicDamage : DamageClass
{
    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
    {
        if (damageClass == DamageClass.Generic || damageClass == Ranged || damageClass == Magic)
            return StatInheritanceData.Full;
        return StatInheritanceData.None;
    }

    public override bool GetEffectInheritance(DamageClass damageClass)
    {
        if (damageClass == Ranged)
            return true;
        if (damageClass == Magic)
            return true;

        return false;
    }

    public override void SetDefaultStats(Player player)
    {
        player.GetCritChance<RangedMagicDamage>() += 4;
    }

    public override bool UseStandardCritCalcs => true;
}