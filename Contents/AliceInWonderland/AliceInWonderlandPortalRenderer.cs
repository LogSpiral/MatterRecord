using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;

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

internal class AliceInWonderlandPortalRenderer : IPlayerRenderer
{
    private List<DrawData> _voidLensData = new List<DrawData>();
    private AliceInWonderlandInteractionChecker _interactionChecker = new AliceInWonderlandInteractionChecker();

    public void DrawPlayers(Camera camera, IEnumerable<Player> players)
    {
        foreach (Player player in players)
        {
            DrawReturnGateInWorld(camera, player);
        }
    }

    public void DrawPlayerHead(Camera camera, Player drawPlayer, Vector2 position, float alpha = 1f, float scale = 1f, Color borderColor = default(Color))
    {
        DrawReturnGateInMap(camera, drawPlayer);
    }

    public void DrawPlayer(Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow = 0f, float scale = 1f)
    {
        DrawReturnGateInWorld(camera, drawPlayer);
    }

    private void DrawReturnGateInMap(Camera camera, Player player)
    {
    }
    public static bool TryGetGateHitbox(Player player, out Rectangle homeHitbox)
    {
        var mplr = player.GetModPlayer<AliceInWonderlandPlayer>();
        homeHitbox = Rectangle.Empty;
        if (!mplr.CurrentPortalStart.HasValue)
        {
            return false;
        }

        Vector2 vector = new Vector2(0f, -21f);
        Vector2 center = mplr.CurrentPortalStart.Value + vector;
        homeHitbox = Utils.CenteredRectangle(center, new Vector2(24f, 40f));
        return true;
    }
    private void DrawReturnGateInWorld(Camera camera, Player player)
    {
        var mplr = player.GetModPlayer<AliceInWonderlandPlayer>();
        Rectangle homeHitbox = Rectangle.Empty;
        if (!TryGetGateHitbox(player, out homeHitbox))
            return;

        int num = 0;
        AHoverInteractionChecker.HoverStatus hoverStatus = AHoverInteractionChecker.HoverStatus.NotSelectable;
        if (player == Main.LocalPlayer)
            _interactionChecker.AttemptInteraction(player, homeHitbox);

        if (Main.SmartInteractPotionOfReturn)
            hoverStatus = AHoverInteractionChecker.HoverStatus.Selected;

        num = (int)hoverStatus;
        if (!mplr.CurrentPortalEnd.HasValue)
            return;

        SpriteBatch spriteBatch = camera.SpriteBatch;
        SamplerState sampler = camera.Sampler;
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.None, camera.Rasterizer, null, camera.GameViewMatrix.TransformationMatrix);
        float opacity = ((player.whoAmI == Main.myPlayer) ? 1f : 0.1f);
        Vector2 value = mplr.CurrentPortalEnd.Value;
        Vector2 vector = new Vector2(0f, -21f);
        Vector2 worldPosition = value + vector;
        Vector2 worldPosition2 = homeHitbox.Center.ToVector2();
        PotionOfReturnGateHelper potionOfReturnGateHelper = new PotionOfReturnGateHelper(PotionOfReturnGateHelper.GateType.ExitPoint, worldPosition, opacity);
        PotionOfReturnGateHelper potionOfReturnGateHelper2 = new PotionOfReturnGateHelper(PotionOfReturnGateHelper.GateType.EntryPoint, worldPosition2, opacity);
        if (!Main.gamePaused)
        {
            potionOfReturnGateHelper.Update();
            potionOfReturnGateHelper2.Update();
        }

        _voidLensData.Clear();
        potionOfReturnGateHelper.DrawToDrawData(_voidLensData, 0);
        potionOfReturnGateHelper2.DrawToDrawData(_voidLensData, num);

        foreach (DrawData voidLensDatum in _voidLensData)
            voidLensDatum.Draw(spriteBatch);
        //{
        //    var dummy = voidLensDatum;
        //    var randColor = Main.hslToRgb(new Vector3(Main.rand.NextFloat(), 1.0f, Main.rand.NextFloat()));
        //    dummy.color = Color.Lerp(voidLensDatum.color, randColor, Main.rand.NextFloat());
        //    dummy.position += Main.rand.NextVector2Unit() * Main.rand.NextFloat();
        //    dummy.Draw(spriteBatch);
        //}

        spriteBatch.End();
    }
}
