
using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.ForgetPill;

public class ForgetPill : ModItem
{
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 15;
        Item.rare = ItemRarityID.Master;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        Item.UseSound = SoundID.Zombie104;
        base.SetDefaults();
    }
    public override bool? UseItem(Player player)
    {
        RecorderSystem.ClearRecord();
        return null;
    }
    public override string Texture => $"Terraria/Images/Item_{ItemID.LihzahrdPowerCell}";

    public override Color? GetAlpha(Color lightColor) => Main.DiscoColor;
}
