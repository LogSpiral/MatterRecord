namespace MatterRecord.Contents.WarAndPeace;
public class WarAndPeacePlayer : ModPlayer
{
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Player.HasBuff<Peace>())
            modifiers.FinalDamage.Flat -= 5;
    }
}
