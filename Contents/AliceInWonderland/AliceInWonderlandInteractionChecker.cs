using Microsoft.Xna.Framework;
using Terraria.GameContent.ObjectInteractions;
namespace MatterRecord.Contents.AliceInWonderland;
public class AliceInWonderlandInteractionChecker : AHoverInteractionChecker
{
    public override bool? AttemptOverridingHoverStatus(Player player, Rectangle rectangle)
    {
        if (Main.SmartInteractPotionOfReturn)
            return true;

        return null;
    }

    public override void DoHoverEffect(Player player, Rectangle hitbox)
    {
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<AliceInWonderlandWatch>();
    }

    public override bool ShouldBlockInteraction(Player player, Rectangle hitbox) => Player.BlockInteractionWithProjectiles != 0;

    public override void PerformInteraction(Player player, Rectangle hitbox)
    {
        var mplr = player.GetModPlayer<AliceInWonderlandPlayer>();
        if (mplr.CurrentPortalEnd.HasValue)
        {
            Vector2 newPos = mplr.CurrentPortalEnd.Value + player.Size * new Vector2(-0.5f, -1f);
            int num = 8;
            player.Teleport(newPos, num);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, newPos.X, newPos.Y, num);
            mplr.CurrentPortalEnd = null;
            mplr.CurrentPortalStart = null;

            int count = Main.rand.Next(40, 60);
            for (int n = 0; n < count; n++)
            {
                var dust = Dust.NewDustPerfect(
                    player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(32),
                    DustID.FireworksRGB,
                    Main.rand.NextVector2Unit() * Main.rand.NextFloat(32),
                    0,
                    Main.hslToRgb(new Vector3(Main.rand.NextFloat(), 1.0f, Main.rand.NextFloat())),
                    Main.rand.NextFloat(1f));
                dust.noGravity = true;
            }
            //Main.rand.Next([Color.Black, Color.Cyan, Color.Purple, Color.Pink])
            mplr.PortalSpawnLock = true;
            mplr.PortalSpawnedToday++;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                mplr.SyncPlayer(-1, player.whoAmI, false);
        }
    }
}
