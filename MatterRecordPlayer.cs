namespace MatterRecord;

public class MatterRecordPlayer : ModPlayer
{
    public bool ultraFallEnable;
    public float strengthOfShake;

    public override void ResetEffects()
    {
        ultraFallEnable = false;
        base.ResetEffects();
    }

    public override void PreUpdate()
    {
        if (ultraFallEnable)
            Player.maxFallSpeed = 214514;
        base.PreUpdate();
    }

    public override void ModifyScreenPosition()
    {
        strengthOfShake *= 0.6f;
        if (strengthOfShake < 0.025f) strengthOfShake = 0;
        Main.screenPosition += Main.rand.NextVector2Unit() * strengthOfShake * 48 * 0.15f;
    }
}
