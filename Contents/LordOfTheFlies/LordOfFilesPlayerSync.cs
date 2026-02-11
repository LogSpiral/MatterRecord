using MatterRecord.Contents.TheoryOfFreedom;
using NetSimplified;
using NetSimplified.Syncing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.LordOfTheFlies;

[AutoSync]
internal class LordOfFilesPlayerSync : NetModule
{
    private byte _whoAmI;
    private byte _storedAmmoCount;
    private bool _isInTrialMode;
    private byte _chargeTimer;
    private byte _chargingEnergy;
    public static LordOfFilesPlayerSync Get(int whoAmI, int storedAmmoCount, bool isInTrialMode, int chargeTimer, int chargingEnergy)
    {
        var packet = NetModuleLoader.Get<LordOfFilesPlayerSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._storedAmmoCount = (byte)storedAmmoCount;
        packet._isInTrialMode = isInTrialMode;
        packet._chargeTimer = (byte)chargeTimer;
        packet._chargingEnergy = (byte)chargingEnergy;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<LordOfTheFliesPlayer>();
        modPlayer.StoredAmmoCount = _storedAmmoCount;
        modPlayer.IsInTrialMode = _isInTrialMode;
        modPlayer.ChargeTimer = _chargeTimer;
        modPlayer.ChargingEnergy = _chargingEnergy;
        if (Main.dedServ)
            Get(
                _whoAmI, 
                _storedAmmoCount, 
                _isInTrialMode,
                _chargeTimer, 
                _chargingEnergy)
                .Send(-1, Sender);
    }
}
