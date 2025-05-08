using LogSpiralLibrary.CodeLibrary;
using LogSpiralLibrary.CodeLibrary.Utilties;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.GameContent.Events;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public abstract class ActionLikeDreams : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

        public abstract void UseAction(Player player);

        public override void SetDefaults()
        {
            Item.useTime = Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = MySoundID.MagicShiny;
            base.SetDefaults();
        }

        public override bool? UseItem(Player player)
        {
            var mplr = player.GetModPlayer<DreamPlayer>();
            if (player.itemAnimation == 1)
            {
                UseAction(player);
            }
            return base.UseItem(player);
        }

    }
    public class WizardDream : ActionLikeDreams
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.consumable = true;
            Item.maxStack = 10;
        }
        public override void UseAction(Player player)
        {
            var mplr = player.GetModPlayer<DreamPlayer>();

            if (mplr.WizardDreamCount >= 10)
                Main.NewText("已达使用上限", Color.Blue);
            else 
            {
                mplr.WizardDreamCount++;
                Item.stack--;
                if(Item.stack <= 0)
                    Item.TurnToAir();
            }
        }
        public override bool ConsumeItem(Player player) => player.GetModPlayer<DreamPlayer>().WizardDreamCount < 10;
    }
    public class ZoologiseDream : ActionLikeDreams
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.consumable = true;
        }
        public override void UseAction(Player player) 
        {
            if (DreamWorld.UsedZoologistDream)
                Main.NewText("你梦到了同一只史莱姆", Color.Pink);
            else 
            {
                DreamWorld.UsedZoologistDream = true;
                Item.stack--;
                if (Item.stack <= 0)
                    Item.TurnToAir();
            }
        }
        public override bool ConsumeItem(Player player) => !DreamWorld.UsedZoologistDream;
    }
    public class GolferDream : ActionLikeDreams
    {
        public override void UseAction(Player player)
        {
            if (Sandstorm.Happening)
                Sandstorm.StopSandstorm();
            else
                Sandstorm.StartSandstorm();
        }
    }
    public class PirateDream : ActionLikeDreams
    {
        public override void UseAction(Player player)
        {
            if (Main.cloudAlpha <= 0.02f)
            {
                Main.StartRain();
                Main.cloudAlpha = Main.maxRaining = 1f;
            }
            else
            {
                Main.StopRain();
                Main.cloudAlpha = Main.maxRaining = 0f;
            }
        }
    }
    public class BrokenDream : ActionLikeDreams 
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.consumable = true;
            Item.maxStack = 9999;
        }
        public override void UseAction(Player player)
        {
            int buff;
            do buff = Main.rand.Next(BuffID.Count);
            while (Main.buffNoTimeDisplay[buff]);

            player.AddBuff(buff, 6000);
            Item.stack--;
            if (Item.stack <= 0)
                Item.TurnToAir();
        }
    }
}
