using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesGlobalProj : GlobalProjectile
{
    public bool IsFromTrialMode;
    public override bool InstancePerEntity => true;
    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (IsFromTrialMode) 
        {
            modifiers.FlatBonusDamage += target.lifeMax / 1000;
            modifiers.ArmorPenetration += 20;
        }
    }
}
