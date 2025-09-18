using Microsoft.Xna.Framework;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesGlobalProj : GlobalProjectile
{
    public bool IsFromTrialMode;
    public bool IsFromLOF;
    public override bool InstancePerEntity => true;

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (IsFromTrialMode)
        {
            // modifiers.FlatBonusDamage += target.lifeMax / 1000;
            modifiers.SetCrit();
            modifiers.ArmorPenetration += 20;
        }
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        IsFromTrialMode = binaryReader.ReadBoolean();
        IsFromLOF = binaryReader.ReadBoolean();
        base.ReceiveExtraAI(projectile, bitReader, binaryReader);
    }

    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(IsFromTrialMode);
        binaryWriter.Write(IsFromLOF);
        base.SendExtraAI(projectile, bitWriter, binaryWriter);
    }

    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is EntitySource_ItemUse itemUseSource && itemUseSource?.Item?.ModItem is LordOfTheFlies)
            IsFromLOF = true;

        base.OnSpawn(projectile, source);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (IsFromLOF)
        {
            var owner = Main.player[projectile.owner];
            var mplr = owner.GetModPlayer<LordOfTheFliesPlayer>();
            if (hit.Crit)
            {
                var rangedModifier = owner.rangedDamage;
                //rangedModifier.Additive *= .1f;
                //rangedModifier.Multiplicative = (rangedModifier.Multiplicative - 1) * .25f + 1;
                var critChance = owner.GetTotalCritChance(DamageClass.Ranged) * 0.01f;
                critChance = MathHelper.Clamp(critChance, 0, 1);
                var dmg = rangedModifier.ApplyTo(Main.DamageVar(target.lifeMax * critChance * 0.0005f + 5, owner.luck));
                bool flag = target.life > 0;
                NPC.HitInfo info = hit;
                info.Damage = (int)dmg;
                info.Knockback = 0;
                // target.StrikeNPC(info);
                owner.StrikeNPCDirect(target, info);
                // if (flag && target.life <= 0)
                // {
                //     mplr.NPCKillCount++;
                // }
                // NetMessage.SendStrikeNPC(target, info);
            }
            // if (target.life <= 0)
            // {
            //     mplr.NPCKillCount++;
            // }
        }
        base.OnHitNPC(projectile, target, hit, damageDone);
    }
}