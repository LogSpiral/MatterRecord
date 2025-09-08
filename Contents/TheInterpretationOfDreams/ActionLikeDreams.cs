// using LogSpiralLibrary.CodeLibrary.Utilties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Events;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public abstract class ActionLikeDreams(Asset<Texture2D> icon, Func<bool> condition) : BasicDream(icon, condition)
{
    protected ActionLikeDreams(int index, Func<bool> condition) : this(index == -1 ? null : TextureAssets.NpcHead[index], condition)
    {
    }

    public override string Texture => $"Terraria/Images/Item_{ItemID.Cloud}";

    public abstract void UseAction(Player player);

    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item4; // MySoundID.MagicShiny;
        Item.noUseGraphic = true;
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

public class WizardDream() : ActionLikeDreams(10, () => NPC.savedWizard)
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
            Main.NewText(this.GetLocalizedValue("ReachedLimit"), Color.Blue);
        else
        {
            mplr.WizardDreamCount++;
            Item.stack--;
            if (Item.stack <= 0)
                Item.TurnToAir();
        }
    }

    public override bool ConsumeItem(Player player) => player.GetModPlayer<DreamPlayer>().WizardDreamCount < 10;

    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.SpellTome);
}

public class ZoologiseDream() : ActionLikeDreams(26, () => Main.GetBestiaryProgressReport().CompletionPercent >= 0.1f)
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.consumable = true;
    }

    public override void UseAction(Player player)
    {
        if (DreamWorld.UsedZoologistDream)
            Main.NewText(this.GetLocalizedValue("TheSameSlime"), Color.Pink);
        else
        {
            DreamWorld.UsedZoologistDream = true;
            Item.stack--;
            if (Item.stack <= 0)
                Item.TurnToAir();
        }
    }

    public override bool ConsumeItem(Player player) => !DreamWorld.UsedZoologistDream;

    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.LicenseCat);
}

public class GolferDream() : ActionLikeDreams(25, () => NPC.savedGolfer)
{
    public override void UseAction(Player player)
    {
        if (Sandstorm.Happening)
            Sandstorm.StopSandstorm();
        else
            Sandstorm.StartSandstorm();
    }

    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.GolfBall);
}

public class PirateDream() : ActionLikeDreams(19, () => NPC.downedPirates)
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

    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Cannonball);
}

public class BrokenDream() : ActionLikeDreams(-1, null)
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.consumable = true;
        Item.maxStack = 9999;
        Item.rare = ItemRarityID.Blue;
    }

    public override void UseAction(Player player)
    {
        List<int> buffs = [];
        for (int n = 0; n < ItemID.Count; n++)
        {
            var item = ContentSamples.ItemsByType[n];
            if (item.buffTime is 0 || item.buffType is 0) continue;
            var id = item.buffType;
            if (id is BuffID.Honey or BuffID.WellFed or BuffID.WellFed2 or BuffID.WellFed3) continue;
            if (!Main.buffNoTimeDisplay[id] && !Main.debuff[id] && !Main.vanityPet[id] && !Main.lightPet[id] && !Main.meleeBuff[id])
                buffs.Add(id);
        }
        player.AddBuff(Main.rand.Next(buffs), 6000);

        Item.stack--;
        if (Item.stack <= 0)
            Item.TurnToAir();
    }

    public override void AddRecipes()
    {
    }
}