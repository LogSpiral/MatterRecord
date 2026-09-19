using MatterRecord.Contents.EternalWine;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.LittlePrince;

/// <summary>
/// 小王子饰品的玩家侧逻辑：装备状态标记、死亡拦截与冷却计时。
/// </summary>
public class LittlePrincePlayer : ModPlayer
{
    /// <summary>
    /// 死亡拦截冷却时长（帧）：18000 帧 = 5 分钟。
    /// </summary>
    public const int ReviveCooldownFrames = 18000;

    /// <summary>
    /// 拦截触发后给予的无敌帧时长：60 帧 = 1 秒。
    /// </summary>
    private const int ReviveImmuneFrames = 60;

    /// <summary>
    /// 本帧是否装备了小王子。由 <see cref="LittlePrince.UpdateAccessory"/> 置位、
    /// 在本类 <see cref="ResetEffects"/> 中清零，因此只代表「当前帧」的装备状态。
    /// </summary>
    public bool EquippedRose;

    /// <summary>
    /// 本帧小王子饰品是否可见（未关闭饰品可见性）。
    /// 与 <see cref="EquippedRose"/> 同为每帧状态，由 <see cref="LittlePrince.UpdateAccessory"/> 置位、
    /// 在 <see cref="ResetEffects"/> 中清零。仅用于决定是否生成玻璃罩，不影响死亡拦截本身。
    /// </summary>
    public bool EquippedRoseVisible;

    /// <summary>
    /// 死亡拦截的冷却剩余帧数，0 表示就绪。
    /// 只在真正触发拦截时被写入，之后每帧自然递减——不采用「未触发即清零」的降级写法，
    /// 否则任何一次普通死亡都会把冷却抹掉。
    /// </summary>
    public int ReviveCooldownTimer = 0;

    /// <summary>
    /// 每帧复位玩家效果。此处负责清空装备标记，并推进死亡拦截冷却。
    /// </summary>
    public override void ResetEffects()
    {
        EquippedRose = false;
        EquippedRoseVisible = false;

        // ResetEffects 是玩家级帧计时器的标准递减点（每帧都会重跑），不能把递减放到 PreKill 里
        if (ReviveCooldownTimer > 0)
            ReviveCooldownTimer--;

        base.ResetEffects();
    }

    /// <summary>
    /// 每帧把「凋谢的玫瑰」Buff 的状态与 <see cref="ReviveCooldownTimer"/> 对齐：
    /// 冷却中则挂上（已存在则刷新显示时间），冷却结束或玩家已死亡则移除。
    /// Buff 只做展示，任何清除都不会影响 <see cref="ReviveCooldownTimer"/> 本身。
    /// </summary>
    public override void PostUpdate()
    {
        int buffType = ModContent.BuffType<LittlePrinceWitheredRose>();

        // 玩家死亡时原版会清空全部 Buff，此处不再补挂；重生时 OnRespawn 会把冷却清零
        if (ReviveCooldownTimer <= 0 || Player.dead)
        {
            Player.ClearBuff(buffType);
        }
        else
        {
            int index = Player.FindBuffIndex(buffType);
            if (index < 0)
            {
                // 首次挂上：持续时间取剩余冷却，之后每帧对其刷新
                Player.AddBuff(buffType, ReviveCooldownTimer);
            }
            else
            {
                // 直接把读条时间对齐真实冷却，显示即剩余
                Player.buffTime[index] = ReviveCooldownTimer;
            }
        }

        base.PostUpdate();
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

    /// <summary>
    /// 玩家即将死亡时的拦截：装备小王子且冷却就绪时取消本次死亡，
    /// 保留 1 点生命、给予 1 秒无敌，并由权威端执行世界效果。
    /// 只处理本地玩家（owner）——多人下每个端都会替任意玩家重放死亡，
    /// 旁观端不掌握他人的冷却状态，介入只会误判。
    /// </summary>
    public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
    {
        if (Player.whoAmI != Main.myPlayer)
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);

        if (!EquippedRose || ReviveCooldownTimer > 0)
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);

        ReviveCooldownTimer = ReviveCooldownFrames;

        Player.statLife = 1;
        if (Player.statLife < 1)
            Player.statLife = 1;

        Player.immune = true;
        Player.immuneTime = ReviveImmuneFrames;
        foreach (int cooldownID in ImmunityHelper.GetAllImmunityCooldownIDs())
            Player.AddImmuneTime(cooldownID, ReviveImmuneFrames);

        SoundEngine.PlaySound(SoundID.Item29, Player.Center);

        if (Main.netMode == NetmodeID.MultiplayerClient)
            LittlePrinceReviveSync.Get(Player.whoAmI, EquippedRoseVisible).Send();
        else if (Main.netMode == NetmodeID.SinglePlayer)
            LittlePrinceReviveEffects.Apply(Player, EquippedRoseVisible);

        return false;
    }

    /// <summary>
    /// 玩家重生时清空死亡拦截冷却：真正死亡（未触发拦截）后，下一条命不携带旧冷却。
    /// </summary>
    public override void OnRespawn()
    {
        ReviveCooldownTimer = 0;
        base.OnRespawn();
    }
}