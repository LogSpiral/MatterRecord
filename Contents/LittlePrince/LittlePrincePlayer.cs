namespace MatterRecord.Contents.LittlePrince;

public class LittlePrincePlayer : ModPlayer
{
    public bool EquippedRose;

    public override void ResetEffects()
    {
        EquippedRose = false;
        base.ResetEffects();
    }

    public override void Load()
    {
        On_Player.DropCoins += PrinceBanDropCoins;
        On_Player.DropTombstone += PrinceBanDropTombstone;
        base.Load();
    }
    private static void PrinceBanDropTombstone(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, Terraria.Localization.NetworkText deathText, int hitDirection)
    {
        if (self.GetModPlayer<LittlePrincePlayer>().EquippedRose) return;
        orig(self, coinsOwned, deathText, hitDirection);
    }

    public override void UpdateEquips()
    {
        // Player.buffImmune[BuffID.ManaSickness] = EquippedRose;

        base.UpdateEquips();
    }

    private static long PrinceBanDropCoins(On_Player.orig_DropCoins orig, Player self)
    {
        if (self.GetModPlayer<LittlePrincePlayer>().EquippedRose)
        {
            self.lostCoins = 0L;
            self.lostCoinString = "";
            return 0L;
        }
        return orig(self);
    }

    public override void Unload()
    {
        On_Player.DropCoins -= PrinceBanDropCoins;
        On_Player.DropTombstone -= PrinceBanDropTombstone;
        base.Unload();
    }
}