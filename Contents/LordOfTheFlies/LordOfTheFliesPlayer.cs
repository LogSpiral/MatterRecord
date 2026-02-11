using System.IO;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesPlayer : ModPlayer
{
    public int StoredAmmoCount { get; set; }

    public bool IsInTrialMode { get; set; }

    public int ChargeTimer;

    public int ChargingEnergy;

    // 移除击杀数加成机制
    /*public int PlayerKillCount { get; set; }
    public int NPCKillCount { get; set; }*/

    public bool IsChargingAnnihilation { get; set; }

    //public bool ControlUseAnnihilation { get; private set; }

    public int RightCooldown { get; set; }

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(StoredAmmoCount), StoredAmmoCount);
        // tag.Add(nameof(PlayerKillCount), PlayerKillCount);
        // tag.Add(nameof(NPCKillCount), NPCKillCount);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet(nameof(StoredAmmoCount), out int amount))
            StoredAmmoCount = amount;
        /* if (tag.TryGet(nameof(PlayerKillCount), out int count))
            PlayerKillCount = count;
        if (tag.TryGet(nameof(NPCKillCount), out int count2))
            NPCKillCount = count2; */
        base.LoadData(tag);
    }

    public override void UpdateEquips()
    {
        if (Player.HeldItem?.ModItem is not LordOfTheFlies) return;
        /*var count = NPCKillCount;
        var factor = count / (count + 200f);
        var count2 = PlayerKillCount;
        var factor2 = count2 / (count2 + 20f);
        Player.rangedDamage.Additive += factor * .5f;
        Player.rangedDamage.Additive += factor2;*/
        base.UpdateEquips();
    }

    public override void PreUpdate()
    {
#if false
        StoredAmmoCount = 6;
        ChargingEnergy = 120;
#endif
        RightCooldown--;
        if (Player.HeldItem?.ModItem is not LordOfTheFlies || ChargingEnergy < 3)
            IsInTrialMode = false;
        //ChargingEnergy = 0;
        if (!IsInTrialMode && ChargingEnergy < 120 && (int)(Main.GlobalTimeWrappedHourly * 60) % 5 == 0)
        {
            ChargingEnergy++;
        }
        base.PreUpdate();
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        modifiers.DamageSource.TryGetCausingEntity(out var causing);
        if (causing is Player && modifiers.DamageSource.SourceProjectileLocalIndex != -1)
        {
            var proj = Main.projectile[modifiers.DamageSource.SourceProjectileLocalIndex];
            if (proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>().IsFromTrialMode)
            {
                modifiers.ArmorPenetration += 20;
                modifiers.FinalDamage += 1;
            }
        }
        base.ModifyHurt(ref modifiers);
    }

    /*public override void OnHurt(Player.HurtInfo info)
    {
        info.DamageSource.TryGetCausingEntity(out var causing);
        if (causing is Player plr && info.DamageSource.SourceProjectileLocalIndex != -1)
        {
            var proj = Main.projectile[info.DamageSource.SourceProjectileLocalIndex];
            if (proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>().IsFromLOF)
            {
                if (Player.statLife - info.Damage <= 0)
                {
                    var mplr = plr.GetModPlayer<LordOfTheFliesPlayer>();
                    mplr.PlayerKillCount++;
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        mplr.SyncPlayer(-1, plr.whoAmI, false);
                }
            }
        }
    }*/

    public void ReceivePlayerSync(BinaryReader reader)
    {
        StoredAmmoCount = reader.ReadByte();
        IsInTrialMode = reader.ReadBoolean();
        ChargeTimer = reader.ReadByte();
        ChargingEnergy = reader.ReadByte();
        // PlayerKillCount = reader.ReadInt32();
        // NPCKillCount = reader.ReadInt32();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        LordOfTheFliesPlayer clone = (LordOfTheFliesPlayer)targetCopy;
        clone.StoredAmmoCount = StoredAmmoCount;
        clone.IsInTrialMode = IsInTrialMode;
        clone.ChargeTimer = ChargeTimer;
        clone.ChargingEnergy = ChargingEnergy;
        // clone.PlayerKillCount = PlayerKillCount;
        // clone.NPCKillCount = NPCKillCount;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        LordOfTheFliesPlayer clone = (LordOfTheFliesPlayer)clientPlayer;

        if (StoredAmmoCount != clone.StoredAmmoCount
            || IsInTrialMode != clone.IsInTrialMode
            /*|| PlayerKillCount != clone.PlayerKillCount*/
            /*|| NPCKillCount != clone.NPCKillCount*/)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        LordOfFilesPlayerSync.Get(
            Player.whoAmI,
            StoredAmmoCount,
            IsInTrialMode,
            ChargeTimer,
            ChargingEnergy).Send(toWho, fromWho);
    }

    public void SyncAnniCharging(int toWho, int fromWho)
    {
        LordOfTheFliesAnniChargingSync.Get(Player.whoAmI, IsChargingAnnihilation).Send(toWho, fromWho);
    }
}