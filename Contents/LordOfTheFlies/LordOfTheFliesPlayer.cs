using System;
using System.IO;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesPlayer : ModPlayer
{
    public int StoredAmmoCount { get; set; }

    public bool IsInTrialMode { get; set; }

    public int ChargeTimer;

    public int ChargingEnergy;

    public int SourceRecoveryPauseTimer;

    // 移除击杀数加成机制
    /*public int PlayerKillCount { get; set; }
    public int NPCKillCount { get; set; }*/

    public bool IsChargingAnnihilation { get; set; }

    //public bool ControlUseAnnihilation { get; private set; }

    public int RightCooldown { get; set; }

    /// <summary>蝇王强化1：命中叠层的当前层数（0~5，每层视为护甲防御）。</summary>
    public int DefenseBonusStacks;

    /// <summary>蝇王强化1：防御加成剩余生效时间（单位：帧，300 帧 = 5 秒）。</summary>
    public int DefenseBonusTimer;

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
        // 源质强化后暂停源质恢复的计时器递减
        if (SourceRecoveryPauseTimer > 0)
            SourceRecoveryPauseTimer--;
        if (Player.HeldItem?.ModItem is not LordOfTheFlies || ChargingEnergy < 3)
            IsInTrialMode = false;
        //ChargingEnergy = 0;
        // 源质恢复（使用源质强化后会暂停 30 帧）
        if (!IsInTrialMode && ChargingEnergy < 120 && SourceRecoveryPauseTimer <= 0 && (int)(Main.GlobalTimeWrappedHourly * 60) % 5 == 0)
        {
            ChargingEnergy++;
        }
        base.PreUpdate();
    }

    /// <summary>
    /// 蝇王强化1：命中敌人时叠加防御层数（每次 +1 层，最高 5 层，持续 5 秒）。
    /// 参考本草纲目写法，不新增 buff，层数防御直接以 statDefense 形式生效。
    /// </summary>
    public void AddDefenseStack()
    {
        if (DefenseBonusStacks < 5)
            DefenseBonusStacks++;
        DefenseBonusTimer = 300; // 5 秒 = 300 帧
    }

    /// <summary>
    /// 计算蝇王强化1当前提供的额外护甲防御值。
    /// 每层 = 装备护甲防御 × 0.03（最低 1），总加成 = 层数 × 每层值。
    /// </summary>
    /// <returns>当前强化1提供的额外护甲防御值。</returns>
    public int GetDefenseBonus()
    {
        if (DefenseBonusStacks <= 0)
            return 0;
        int armorDefense = Player.armor[0].defense + Player.armor[1].defense + Player.armor[2].defense;
        int perStack = Math.Max(1, (int)(armorDefense * 0.03f));
        return DefenseBonusStacks * perStack;
    }

    /// <summary>
    /// 每帧更新强化1的层数计时，并把叠层防御视为护甲防御直接加成到玩家防御。
    /// 参考本草纲目写法：不新增 buff、不绘制图标，仅通过属性生效。
    /// </summary>
    public override void PostUpdateEquips()
    {
        // 层数计时递减，超时清零
        if (DefenseBonusTimer > 0)
        {
            DefenseBonusTimer--;
            if (DefenseBonusTimer <= 0)
                DefenseBonusStacks = 0;
        }

        // 叠层防御视为护甲防御，直接加成（仅提供容错，输出平衡在伤害公式中抵消）
        // 进度锁：未解锁 Tier1 时强制清零，防止绕过锁累积防御
        if (!LordOfTheFliesProgression.Tier1_DefenseOnHit)
            DefenseBonusStacks = 0;
        else if (DefenseBonusStacks > 0)
            Player.statDefense += GetDefenseBonus();
        base.PostUpdateEquips();
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        modifiers.DamageSource.TryGetCausingEntity(out var causing);
        if (causing is Player && modifiers.DamageSource.SourceProjectileLocalIndex != -1)
        {
            var proj = Main.projectile[modifiers.DamageSource.SourceProjectileLocalIndex];
            if (proj.GetGlobalProjectile<LordOfTheFliesGlobalProj>().IsFromTrialMode)
            {
                // 无视 20 护甲（项3 进度锁）
                if (LordOfTheFliesProgression.Tier3_Penetration)
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