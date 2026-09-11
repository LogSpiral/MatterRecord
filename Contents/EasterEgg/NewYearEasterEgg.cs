using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.EasterEgg
{
    /// <summary>
    /// 农历新年烟花彩蛋：在春节正月初一00:00:00时，若无Boss存在，在本地玩家屏幕中发射烟花
    /// </summary>
    public class NewYearEasterEgg : ModSystem
    {
        private static bool _fireworksTriggeredThisYear = false;

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            ChineseLunisolarCalendar lunarCalendar = new ChineseLunisolarCalendar();
            DateTime now = DateTime.Now;

            int lunarYear = lunarCalendar.GetYear(now);
            DateTime springFestivalDate = lunarCalendar.ToDateTime(lunarYear, 1, 1, 0, 0, 0, 0);

            if (now.Date == springFestivalDate.Date)
            {
                var data = EasterEggSystem.GetData();
                if (data.LastNewYearFireworkYear == lunarYear)
                    _fireworksTriggeredThisYear = true;
                else
                    _fireworksTriggeredThisYear = false;

                if (!_fireworksTriggeredThisYear && now.Hour == 0 && now.Minute == 0 && now.Second <= 5)
                {
                    if (!IsAnyBossAlive())
                    {
                        TriggerFireworksForLocalPlayer();
                        data.LastNewYearFireworkYear = lunarYear;
                        EasterEggSystem.Save();
                        _fireworksTriggeredThisYear = true;
                    }
                }
            }
        }

        private bool IsAnyBossAlive()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].boss)
                    return true;
            }
            return false;
        }

        public static void TriggerFireworksForLocalPlayer()
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active || player.dead)
                return;

            Vector2 center = player.Center + new Vector2(0, -200f);
            int totalFireworks = Main.rand.Next(20, 35);

            SpawnFireworkBatch(center, totalFireworks / 2 + Main.rand.Next(-3, 4));
            SpawnFireworkBatch(center + new Vector2(Main.rand.Next(-60, 61), Main.rand.Next(-40, 41)), totalFireworks / 2);
        }

        private static void SpawnFireworkBatch(Vector2 center, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Main.rand.NextVector2Square(-450f, 450f);
                Vector2 position = center + offset;

                int[] fireworkTypes = {
                    ProjectileID.RocketFireworkRed,
                    ProjectileID.RocketFireworkGreen,
                    ProjectileID.RocketFireworkBlue,
                    ProjectileID.RocketFireworkYellow,
                    ProjectileID.FireworkFountainRed,
                    ProjectileID.FireworkFountainBlue,
                    ProjectileID.FireworkFountainYellow,
                };
                int type = fireworkTypes[Main.rand.Next(fireworkTypes.Length)];

                Vector2 velocity;
                if (type >= ProjectileID.RocketFireworkRed && type <= ProjectileID.RocketFireworkYellow)
                {
                    velocity = new Vector2(
                        Main.rand.NextFloat(-3.5f, 3.5f),
                        Main.rand.NextFloat(-14f, -7f)
                    );
                }
                else
                {
                    velocity = new Vector2(
                        Main.rand.NextFloat(-1.2f, 1.2f),
                        Main.rand.NextFloat(-4f, -1f)
                    );
                }

                Projectile.NewProjectile(
                    new EntitySource_WorldEvent(),
                    position,
                    velocity,
                    type,
                    0, 0,
                    Main.LocalPlayer.whoAmI
                );
            }
        }
    }

   
}