namespace MatterRecord.Contents.ProtagonistAura;
public class ClothierModify : GlobalNPC
{
    public override void ModifyShop(NPCShop shop)
    {
        if (shop.NpcType == NPCID.Clothier)
            shop.Add<ProtagonistAura>();
    }
}