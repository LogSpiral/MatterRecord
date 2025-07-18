using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
