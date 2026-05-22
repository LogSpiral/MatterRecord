using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.EasterEgg
{
    public class EasterEggGlobalNPC : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.type != NPCID.Bunny && npc.type != NPCID.GoldBunny && npc.type != NPCID.ExplosiveBunny)
                return;
            if (!IsEasterToday())
                return;

            var data = EasterEggSystem.GetData();
            DateTime now = DateTime.Now;
            if (now.Year != data.LastEasterTriggerYear || now.Month != data.LastEasterTriggerMonth || now.Day != data.LastEasterTriggerDay)
            {
                data.LastEasterTriggerYear = now.Year;
                data.LastEasterTriggerMonth = now.Month;
                data.LastEasterTriggerDay = now.Day;
                data.EasterEggTriggeredToday = false;
                EasterEggSystem.Save();
            }

            double chance = data.EasterEggTriggeredToday ? 0.05 : 0.3;
            if (Main.rand.NextFloat() >= chance)
                return;

            data.EasterEggTriggeredToday = true;
            EasterEggSystem.Save();

            Main.NewText("天兔已被击败！", new Color(175, 75, 255));
        }

        private static bool IsEasterToday()
        {
            var today = DateTime.Now.Date;
            int year = today.Year;
            int a = year % 19;
            int b = year / 100;
            int c = year % 100;
            int d = b / 4;
            int e = b % 4;
            int f = (b + 8) / 25;
            int g = (b - f + 1) / 3;
            int h = (19 * a + b - d - g + 15) % 30;
            int i = c / 4;
            int k = c % 4;
            int l = (32 + 2 * e + 2 * i - h - k) % 7;
            int m = (a + 11 * h + 22 * l) / 451;
            int n = h + l - 7 * m + 114;
            int month = n / 31;
            int day = (n % 31) + 1;
            return today.Month == month && today.Day == day;
        }
    }
}