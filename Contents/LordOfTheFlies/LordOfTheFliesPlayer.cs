using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesPlayer : ModPlayer
{
    public int StoredAmmoCount { get; set; }

    public bool IsInTrialMode { get; set; }

    public int ChargeTimer;

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(StoredAmmoCount), StoredAmmoCount);
        tag.Add(nameof(IsInTrialMode), IsInTrialMode);
        base.SaveData(tag);
    }
    public override void LoadData(TagCompound tag)
    {
        if(tag.TryGet(nameof(StoredAmmoCount),out int amount))
            StoredAmmoCount = amount;
        if(tag.TryGet(nameof(IsInTrialMode),out bool trial))
            IsInTrialMode = trial;
        base.LoadData(tag);
    }

    public override void PreUpdate()
    {
        if (Player.HeldItem?.ModItem is not LordOfTheFlies)
            IsInTrialMode = false;
        base.PreUpdate();
    }

    public override void UpdateLifeRegen()
    {
        if (IsInTrialMode) 
        {
            Player.manaRegen = 0;
            Player.manaRegenCount = 0;
            Player.manaRegenDelay = 20;
        }
        base.UpdateLifeRegen();
    }
}
