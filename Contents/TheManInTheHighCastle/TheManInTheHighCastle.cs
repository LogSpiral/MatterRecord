namespace MatterRecord.Contents.TheManInTheHighCastle;

public class TheManInTheHighCastle : ModItem
{
    public override string Texture => $"Terraria/Images/Item_{ItemID.ScytheWhip}";
    public override void UpdateInventory(Player player)
    {
        player.GetModPlayer<TheManInTheHighCastlePlayer>().HasHookWeapon = true;
    }
    public override void SetDefaults()
    {
        base.SetDefaults();
    }
}
