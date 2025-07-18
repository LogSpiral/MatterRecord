using MatterRecord.Contents.TheoryOfFreedom;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesPlayer : ModPlayer
{
    public int StoredAmmoCount { get; set; }

    public bool IsInTrialMode { get; set; }

    public int ChargeTimer;

    public int ChargingEnergy;

    public int PlayerKillCount { get; set; }


    public bool IsChargingAnnihilation { get; set; }

    //public bool ControlUseAnnihilation { get; private set; }

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(StoredAmmoCount), StoredAmmoCount);
        tag.Add(nameof(PlayerKillCount), PlayerKillCount);
        base.SaveData(tag);
    }
    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet(nameof(StoredAmmoCount), out int amount))
            StoredAmmoCount = amount;
        if (tag.TryGet(nameof(PlayerKillCount), out int count))
            PlayerKillCount = count;
        base.LoadData(tag);
    }

    public override void PreUpdate()
    {
        if (Player.HeldItem?.ModItem is not LordOfTheFlies)
            IsInTrialMode = false;
        //ChargingEnergy = 0;
        if (!IsInTrialMode && ChargingEnergy < 120 && (int)(Main.GlobalTimeWrappedHourly * 60) % 5 == 0)
        {
            ChargingEnergy++;
        }
        base.PreUpdate();
    }

    /*static ModKeybind AnnihilationModeKeyBind { get; set; }
    public override void Load()
    {
        AnnihilationModeKeyBind = KeybindLoader.RegisterKeybind(Mod, nameof(AnnihilationModeKeyBind), "Mouse3");
        base.Load();
    }
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if(AnnihilationModeKeyBind.JustPressed)
            ControlUseAnnihilation = true;
        else if(AnnihilationModeKeyBind.JustReleased)
            ControlUseAnnihilation= false;
        base.ProcessTriggers(triggersSet);
    }*/
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
    public override void OnHurt(Player.HurtInfo info)
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
    }


    public void ReceivePlayerSync(BinaryReader reader)
    {
        StoredAmmoCount = reader.ReadByte();
        IsInTrialMode = reader.ReadBoolean();
        ChargeTimer = reader.ReadByte();
        ChargingEnergy = reader.ReadByte();
        PlayerKillCount = reader.ReadInt32();
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        LordOfTheFliesPlayer clone = (LordOfTheFliesPlayer)targetCopy;
        clone.StoredAmmoCount = StoredAmmoCount;
        clone.IsInTrialMode = IsInTrialMode;
        clone.ChargeTimer = ChargeTimer;
        clone.ChargingEnergy = ChargingEnergy;
        clone.PlayerKillCount = PlayerKillCount;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        LordOfTheFliesPlayer clone = (LordOfTheFliesPlayer)clientPlayer;

        if (StoredAmmoCount != clone.StoredAmmoCount
            || IsInTrialMode != clone.IsInTrialMode
            || PlayerKillCount != clone.PlayerKillCount)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)PacketType.LordOfFilesPlayerSync);
        packet.Write((byte)Player.whoAmI);
        packet.Write((byte)StoredAmmoCount);
        packet.Write(IsInTrialMode);
        packet.Write((byte)ChargeTimer);
        packet.Write((byte)ChargingEnergy);
        packet.Write(PlayerKillCount);
        packet.Send(toWho, fromWho);
        base.SyncPlayer(toWho, fromWho, newPlayer);
    }

    public void SyncAnniCharging(int toWho, int fromWho) 
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)PacketType.LordOfFilesChargingSync);
        packet.Write((byte)Player.whoAmI);
        packet.Write(IsChargingAnnihilation);
        packet.Send(toWho,fromWho);
    }

    public void ReceiveAnniCharging(BinaryReader reader) 
    {
        IsChargingAnnihilation = reader.ReadBoolean();
    }
}
