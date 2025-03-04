using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public class TheInterpretationOfDreams : ModItem
{
    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = Item.sellPrice(0, 5);
        Item.rare = ItemRarityID.LightRed;
        Item.useTurn = true;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.width = 24;
        Item.height = 28;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.scale = 1.15f;
        Item.damage = 1;
        base.SetDefaults();
    }
    public override void UseItemHitbox(Player player, ref Rectangle hitbox, ref bool noHitbox)
    {
        foreach (var npc in Main.npc)
        {
            if (!npc.active || !npc.townNPC) continue;
            if (hitbox.Intersects(npc.Hitbox))
            {
                Main.NewText("好好好");
            }
        }
        base.UseItemHitbox(player, ref hitbox, ref noHitbox);
    }
    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        base.MeleeEffects(player, hitbox);
    }
}
