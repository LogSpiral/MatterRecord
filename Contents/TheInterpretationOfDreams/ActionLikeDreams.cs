using LogSpiralLibrary.CodeLibrary;
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

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    public abstract class ActionLikeDreams : ModItem
    {
        public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

        public abstract void UseAction(Player player);

        public override void SetDefaults()
        {
            Item.consumable = true;
            Item.useAnimation = 30;
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
        public override void UseAction(Player player)
        {
            var mplr = player.GetModPlayer<DreamPlayer>();
            if (mplr.WizardDreamCount >= 10)
                Main.NewText("已达使用上限", Color.Blue);
            else
                mplr.WizardDreamCount++;
        }
        public override bool ConsumeItem(Player player) => player.GetModPlayer<DreamPlayer>().WizardDreamCount < 10;
    }
    public class ZoologiseDream : ActionLikeDreams
    {
        public override void UseAction(Player player) => DreamWorld.UsedZoologistDream = true;
        public override bool ConsumeItem(Player player) => !DreamWorld.UsedZoologistDream;
    }
    public class GolferDream : ActionLikeDreams
    {
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.consumable = false;
        }
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
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.consumable = false;
        }
        public override void UseAction(Player player)
        {
            bool flag = Main.GameModeInfo == GameModeData.CreativeMode;
            FieldInfo fldInfo = null;
            if(flag)
                fldInfo= typeof(CreativePowers.ASharedSliderPower).GetField("_sliderCurrentValueCache", BindingFlags.NonPublic | BindingFlags.Instance);
            var power = CreativePowerManager.Instance.GetPower<CreativePowers.ModifyRainPower>();
            if (Main.cloudAlpha == 0)
            {

                fldInfo?.SetValue(power, 1);
                Main.cloudAlpha = Main.maxRaining = 1f;
                Main.StartRain();
            }
            else
            {
                fldInfo?.SetValue(power, 0);
                Main.cloudAlpha = Main.maxRaining = 0f;
                Main.StopRain();
            }
        }
    }
}
