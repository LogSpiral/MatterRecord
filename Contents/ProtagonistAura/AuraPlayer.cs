using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
namespace MatterRecord.Contents.ProtagonistAura;
public class AuraLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

    public override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.colorArmorHead == default) return;
        if (drawInfo.drawPlayer.dead) return;
        var plr = drawInfo.drawPlayer;
        var mplr = plr.GetModPlayer<ProtagonistAuraPlayer>();
        if (!mplr.HasProtagonistAura) return;
        Vector2 center = plr.MountedCenter + Vector2.UnitY * plr.gfxOffY;
        if (!Main.gameMenu)
            center -= Main.screenPosition;
        center = new Vector2((int)center.X, (int)center.Y);

        int offset = mplr.cDye switch
        {
            1 => 0,
            17 => 1,
            54 => 2,
            _ => -1
        };
        if (offset < 0) return;
        int offsetY = (plr.bodyFrame.Top / 56) switch
        {
            >= 7 and <= 9 => -2,
            >= 14 and <= 16 => -2,
            _ => 0
        };
        float t = MathHelper.SmoothStep(0, 1, Main.GlobalTimeWrappedHourly % 1);

        drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/ProtagonistAura/Aura_Glow").Value,
center + new Vector2(4 + (plr.direction < 0 ? -8 : 0), (-23 + offsetY) * plr.gravDir), new Rectangle(offset * 32, 0, 32, 32), (Color.White * (1 - MathF.Cos(MathHelper.TwoPi * MathF.Sqrt(t))) * .75f) with { A = 0 }, 0, new(16), new Vector2(1, 0.6f) * (1 + .5f * t), drawInfo.playerEffect, 0));

        drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/ProtagonistAura/Aura").Value,
            center + new Vector2(4 + (plr.direction < 0 ? -8 : 0), (-23 + offsetY) * plr.gravDir), new Rectangle(offset * 32, 0, 32, 32), Color.White, 0, new(16), new Vector2(1, 0.6f), drawInfo.playerEffect, 0));

        drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/ProtagonistAura/Aura").Value,
center + new Vector2(4 + (plr.direction < 0 ? -8 : 0), (-21 + offsetY) * plr.gravDir + (plr.gravDir < 1 ? 18 : 0)), new Rectangle(96, 0, 32, offset == 2 ? 12 : 14), drawInfo.colorArmorHead, 0, new(16), 1, drawInfo.playerEffect, 0));
    }
}
