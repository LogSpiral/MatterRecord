using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace MatterRecord.Contents.TortoiseShell;

public class TortoiseShellPlayer : ModPlayer
{
    public override void UpdateEquips()
    {
        if (TortoiseShellActive)//TortoiseShellActive
        {
            if (TortoiseDashing)
                Player.endurance += timer * 0.18f;
            else
                Player.endurance += .9f;
            Player.noKnockback = true;
        }
        TortoiseShellActive = false;
        TortoiseDashing = false;
        base.UpdateEquips();
    }

    public override void PreUpdate()
    {
        if (TortoiseShellActive)
            Player.maxFallSpeed = 40;
        if (_syncPending > 0)
        {
            _syncPending--;
            Player.velocity = _syncVelocity;
        }

        base.PreUpdate();
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (TortoiseShellActive)
        {
            drawInfo.drawPlayer.invis = true;
            drawInfo.colorArmorBody = drawInfo.colorArmorHead = drawInfo.colorArmorLegs = default;
        }
        base.ModifyDrawInfo(ref drawInfo);
    }

    public void SetSyncVelocity(Vector2 velocity) 
    {
        _syncPending = 1;
        _syncVelocity = velocity;
        Player.velocity = velocity;
    }

    public bool TortoiseShellActive;
    public bool TortoiseDashing;
    public float timer;

    private int _syncPending;
    private Vector2 _syncVelocity;
}
