using MatterRecord.Contents.TheInterpretationOfDreams;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.AliceInWonderland;

public class AliceInWonderlandPlayer : ModPlayer
{
    public bool CapturedAliceRabbit;
    public int PortalSpawnedToday;
    public bool PortalSpawnLock;
    public Vector2? CurrentPortalStart;
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
            if (Player.PotionOfReturnHomePosition == null)
            {
                int count = Main.rand.Next(40, 60);
                for (int n = 0; n < count; n++)
                {
                    var dust = Dust.NewDustPerfect(
                        Player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(32),
                        DustID.FireworksRGB,
                        Main.rand.NextVector2Unit() * Main.rand.NextFloat(32),
                        0,
                        Main.rand.Next([Color.Black, Color.Cyan, Color.Purple, Color.Pink]),
                        Main.rand.NextFloat(1f));
                    dust.noGravity = true;
                }
                PortalSpawnLock = true;
                PortalSpawnedToday++;
                CurrentPortalStart = null;
                return;
            }
            DustHintTimer--;
            if (DustHintTimer % 10 == 0)
            {
                int count = Main.rand.Next(0, 10);
                for (int n = 0; n < count; n++)
                {
                    var vec = CurrentPortalStart.Value;
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

            if (DustHintTimer < 0) CurrentPortalStart = null;
        }
        base.ResetEffects();
    }
}
