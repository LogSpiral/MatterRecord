using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.AliceInWonderland;

public class AliceInWonderlandPlayer : ModPlayer
{
    private static AliceInWonderlandPortalRenderer AliceInWonderlandPortalRenderer { get; } = new AliceInWonderlandPortalRenderer();

    public override void Load()
    {
        On_Main.DrawPlayers_BehindNPCs += WonderLandPortal_BN;
        On_Main.DrawPlayers_AfterProjectiles += WonderLandPortal_AP;
        On_Main.DrawMiscMapIcons += WonderLandPortal_MapIcon;
        base.Load();
    }

    private void WonderLandPortal_MapIcon(On_Main.orig_DrawMiscMapIcons orig, Main self, SpriteBatch spriteBatch, Vector2 mapTopLeft, Vector2 mapX2Y2AndOff, Rectangle? mapRect, float mapScale, float drawScale, ref string mouseTextString)
    {
        DrawMapIcons_PortalStart(spriteBatch, mapTopLeft, mapX2Y2AndOff, mapRect, mapScale, drawScale, ref mouseTextString);
        DrawMapIcons_PortalEnd(spriteBatch, mapTopLeft, mapX2Y2AndOff, mapRect, mapScale, drawScale, ref mouseTextString);
        orig.Invoke(self, spriteBatch, mapTopLeft, mapX2Y2AndOff, mapRect, mapScale, drawScale, ref mouseTextString);
    }

    private static void DrawMapIcons_PortalEnd(SpriteBatch spriteBatch, Vector2 mapTopLeft, Vector2 mapX2Y2AndOff, Rectangle? mapRect, float mapScale, float drawScale, ref string mouseTextString)
    {
        Vector2? potionOfReturnOriginalUsePosition = Main.LocalPlayer.GetModPlayer<AliceInWonderlandPlayer>().CurrentPortalEnd;
        if (!potionOfReturnOriginalUsePosition.HasValue)
            return;

        Vector2 vec = (potionOfReturnOriginalUsePosition + new Vector2(0f, -Main.LocalPlayer.height / 2)).Value / 16f - mapTopLeft;
        vec *= mapScale;
        vec += mapX2Y2AndOff;
        vec = vec.Floor();
        if (!mapRect.HasValue || mapRect.Value.Contains(vec.ToPoint()))
        {
            Texture2D value = ModAsset.PortalEnd.Value;
            Rectangle rectangle = value.Frame();
            spriteBatch.Draw(value, vec, rectangle, Main.DiscoColor, 0f, rectangle.Size() / 2f, drawScale, SpriteEffects.None, 0f);
            if (Utils.CenteredRectangle(vec, rectangle.Size() * drawScale).Contains(Main.MouseScreen.ToPoint()))
                mouseTextString = ModContent.GetInstance<AliceInWonderlandWatch>().GetLocalizedValue("End");
        }
    }

    private static void DrawMapIcons_PortalStart(SpriteBatch spriteBatch, Vector2 mapTopLeft, Vector2 mapX2Y2AndOff, Rectangle? mapRect, float mapScale, float drawScale, ref string mouseTextString)
    {
        Vector2? potionOfReturnHomePosition = Main.LocalPlayer.GetModPlayer<AliceInWonderlandPlayer>().CurrentPortalStart;
        if (!potionOfReturnHomePosition.HasValue)
            return;

        Vector2 vec = (potionOfReturnHomePosition + new Vector2(0f, -Main.LocalPlayer.height / 2)).Value / 16f - mapTopLeft;
        vec *= mapScale;
        vec += mapX2Y2AndOff;
        vec = vec.Floor();
        if (!mapRect.HasValue || mapRect.Value.Contains(vec.ToPoint()))
        {
            Texture2D value = ModAsset.PortalStart.Value;
            Rectangle rectangle = value.Frame();
            spriteBatch.Draw(value, vec, rectangle, Main.DiscoColor, 0f, rectangle.Size() / 2f, drawScale, SpriteEffects.None, 0f);
            if (Utils.CenteredRectangle(vec, rectangle.Size() * drawScale).Contains(Main.MouseScreen.ToPoint()))
                mouseTextString = ModContent.GetInstance<AliceInWonderlandWatch>().GetLocalizedValue("Start");
        }
    }

    private static void WonderLandPortal_AP(On_Main.orig_DrawPlayers_AfterProjectiles orig, Main self)
    {
        AliceInWonderlandPortalRenderer.DrawPlayers(Main.Camera, Main.instance._playersThatDrawAfterProjectiles.Where(p => p.GetModPlayer<AliceInWonderlandPlayer>().CurrentPortalStart.HasValue));
        orig.Invoke(self);
    }

    private static void WonderLandPortal_BN(On_Main.orig_DrawPlayers_BehindNPCs orig, Main self)
    {
        AliceInWonderlandPortalRenderer.DrawPlayers(Main.Camera, Main.instance._playersThatDrawBehindNPCs.Where(p => p.GetModPlayer<AliceInWonderlandPlayer>().CurrentPortalStart.HasValue));
        orig.Invoke(self);
    }

    public bool CapturedAliceRabbit;
    public int PortalSpawnedToday;
    public bool PortalSpawnLock;
    public Vector2? CurrentPortalStart;
    public Vector2? CurrentPortalEnd;
    public int DustHintTimer;

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(CapturedAliceRabbit), CapturedAliceRabbit);
        tag.Add(nameof(PortalSpawnedToday), PortalSpawnedToday);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet(nameof(CapturedAliceRabbit), out bool flag))
            CapturedAliceRabbit = flag;
        if (tag.TryGet(nameof(PortalSpawnedToday), out int count))
            PortalSpawnedToday = count;
        base.LoadData(tag);
    }

    public override bool? CanCatchNPC(NPC target, Item item)
    {
        if (target is { type: NPCID.Bunny, SpawnedFromStatue: false } && !CapturedAliceRabbit)
        {
            target.SpawnedFromStatue = true;
            CapturedAliceRabbit = true;
            Player.QuickSpawnItem(Player.GetSource_CatchEntity(target), ModContent.ItemType<AliceInWonderlandWatch>());
        }
        return null;
    }

    public override void ResetEffects()
    {
        if (Main.dayTime && Main.time < 1.0)
            PortalSpawnedToday = 0;
        if (PortalSpawnLock && (int)Main.time % 60 == 0)
        {
            var spawn = new Vector2(Main.spawnTileX, Main.spawnTileY) * 16;
            if (Vector2.Distance(Player.Center, spawn) < 1024)
                PortalSpawnLock = false;
        }
        if (CurrentPortalStart.HasValue)
        {
            DustHintTimer--;
            if (DustHintTimer % 10 == 0 && DustHintTimer >= 0)
            {
                int count = Main.rand.Next(0, 10);
                for (int n = 0; n < count; n++)
                {
                    var vec = CurrentPortalStart.Value - Vector2.UnitY * 24;
                    var dust = Dust.NewDustPerfect(
                        Vector2.Lerp(Player.Center, vec, Main.rand.NextFloat(0, Main.rand.NextFloat())) + Main.rand.NextVector2Unit() * Main.rand.NextFloat(32),
                        DustID.FireworksRGB,
                        (vec - Player.Center).SafeNormalize(default) * Main.rand.NextFloat(32),
                        0,
                        Main.rand.Next([Color.Black, Color.Cyan, Color.Purple, Color.Pink]),
                        Main.rand.NextFloat(1f));
                    dust.noGravity = true;
                }
            }
        }
        base.ResetEffects();
    }

    public void ReceivePlayerSync(BinaryReader reader)
    {
        if (reader.ReadBoolean())
        {
            CurrentPortalStart = reader.ReadVector2();
            CurrentPortalEnd = reader.ReadVector2();
        }
        else
        {
            CurrentPortalStart = null;
            CurrentPortalEnd = null;
        }
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        AliceInWonderlandPlayer clone = (AliceInWonderlandPlayer)targetCopy;
        clone.CurrentPortalStart = CurrentPortalStart;
        clone.CurrentPortalEnd = CurrentPortalEnd;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        AliceInWonderlandPlayer clone = (AliceInWonderlandPlayer)clientPlayer;

        if (CurrentPortalStart != clone.CurrentPortalStart
            || CurrentPortalEnd != clone.CurrentPortalEnd)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)PacketType.AliceInWonderLandSync);
        packet.Write((byte)Player.whoAmI);
        bool hasValue = CurrentPortalEnd.HasValue;
        packet.Write(hasValue);
        if (hasValue)
        {
            packet.WriteVector2(CurrentPortalStart.Value);
            packet.WriteVector2(CurrentPortalEnd.Value);
        }
        packet.Send(toWho, fromWho);
        base.SyncPlayer(toWho, fromWho, newPlayer);
    }
}