using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public abstract class UnlockLikeDreams(DreamState dreamState, Func<bool> condition) : ActionLikeDreams(GetHeadIcon(dreamState), condition)
{
    private static Asset<Texture2D> GetHeadIcon(DreamState dreamState)
    {
        if (dreamState is DreamState.SkeletonMerchant)
            return ModContent.Request<Texture2D>("MatterRecord/Contents/TheInterpretationOfDreams/SkeletonMerchantHead");
        return TextureAssets.NpcHead[dreamState switch
        {
            DreamState.Princess => 45,
            DreamState.SantaClaus => 11,
            DreamState.GoblinTinkerer => 9,
            DreamState.Merchant => 2,
            DreamState.Mechanic => 8,
            DreamState.Demolitionist => 4,
            DreamState.TaxCollector => 23,
            DreamState.Steampunker => 13,
            DreamState.Dryad => 5,
            DreamState.WitchDoctor => 18,
            DreamState.Painter => 17,
            DreamState.Truffle => 12,
            DreamState.PartyGirl => 15,
            DreamState.ArmsDealer => 6,
            DreamState.Angler => 22,
            DreamState.Nurse => 3,
            DreamState.TravellingMerchant => 21,
            _ => 0
        }];
    }

    private DreamState DreamState { get; init; } = dreamState;

    public override void UseAction(Player player)
    {
        var mplr = player.GetModPlayer<DreamPlayer>();
        if (mplr.CheckUnlock(DreamState))
        {
            Main.NewText(Language.GetTextValue("Mods.MatterRecord.Items.TheInterpretationOfDreams.Unlocked"));
        }
        else
        {
            mplr.UnlockState |= DreamState;
            Item.stack--;
            if (Item.stack <= 0)
                Item.TurnToAir();
        }
    }

    public override void SetStaticDefaults()
    {
        DreamWorld.DreamTypeByState[DreamState] = Type;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var index = tooltips.FindIndex(line => line.Name == "ItemName");
        if (index != -1)
            tooltips.Insert(index + 1, new TooltipLine(Mod, "UnlockHint", Language.GetTextValue("Mods.MatterRecord.Items.TheInterpretationOfDreams.UnlockHint")));
        base.ModifyTooltips(tooltips);
    }
}

public class SkeletonMerchantDream() : UnlockLikeDreams(DreamState.SkeletonMerchant, null)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Bone, 10);
}

public class PrincessDream() : UnlockLikeDreams(DreamState.Princess, () => NPC.unlockedPrincessSpawn)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.RoyalTiara);
}

public class SantaClausDream() : UnlockLikeDreams(DreamState.SantaClaus, () => NPC.downedFrost && Main.xMas)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.ChristmasTree);
}

public class GoblinTinkererDream() : UnlockLikeDreams(DreamState.GoblinTinkerer, () => NPC.savedGoblin)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Ruler);
}

public class MerchantDream() : UnlockLikeDreams(DreamState.Merchant, NPC.SpawnAllowed_Merchant)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.PiggyBank);
}

public class MechanicDream() : UnlockLikeDreams(DreamState.Mechanic, () => NPC.savedMech)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Wrench);
}

public class DemolitionistDream() : UnlockLikeDreams(DreamState.Demolitionist, NPC.SpawnAllowed_Demolitionist)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Bomb);
}

public class TaxCollectorDream() : UnlockLikeDreams(DreamState.TaxCollector, () => NPC.savedTaxCollector)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.GoldCoin);
}

public class SteampunkerDream() : UnlockLikeDreams(DreamState.Steampunker, () => (Main.tenthAnniversaryWorld && !Main.remixWorld) || NPC.downedMechBossAny)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Cog);
}

public class DryadDream() : UnlockLikeDreams(DreamState.Dryad, NPC.SpawnAllowed_DyeTrader)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.PurificationPowder);
}

public class WitchDoctorDream() : UnlockLikeDreams(DreamState.WitchDoctor, () => NPC.downedQueenBee)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Blowgun);
}

public class PainterDream() : UnlockLikeDreams(DreamState.Painter, null)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.Paintbrush);
}

public class TruffleDream() : UnlockLikeDreams(DreamState.Truffle, () => NPC.unlockedTruffleSpawn)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.StrangeGlowingMushroom);
}

public class PartyGirlDream() : UnlockLikeDreams(DreamState.PartyGirl, () => NPC.unlockedPartyGirlSpawn)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.ConfettiGun);
}

public class ArmsDealerDream() : UnlockLikeDreams(DreamState.ArmsDealer, NPC.SpawnAllowed_ArmsDealer)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.IllegalGunParts);
}

public class AnglerDream() : UnlockLikeDreams(DreamState.Angler, () => NPC.savedAngler)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.MasterBait);
}

public class NurseDream() : UnlockLikeDreams(DreamState.Nurse, NPC.SpawnAllowed_Nurse)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.LifeCrystal);
}

public class TravellingMerchatDream() : UnlockLikeDreams(DreamState.TravellingMerchant, null)
{
    public override void ExtraIngredient(Recipe recipe) => recipe.AddIngredient(ItemID.PeddlersHat);
}