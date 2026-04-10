using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.Utilities;

namespace MatterRecord.Contents.Recorder;

public partial class RecordBookItem
{
    public override bool CanUseItem(Player player) => this.IsRecordUnlocked;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (this.IsRecordUnlocked) return;
        HashSet<string> whiteList = ["ItemName", "Favorite", "FavoriteDesc"];
        tooltips.RemoveAll(line => line.Mod is "Terraria" or "MatterRecord" && !whiteList.Contains(line.Name));
        tooltips.Add(new(Mod, "LockedRecords", Language.GetTextValue("Mods.MatterRecord.Items.LockedRecords")));
    }

    public override bool? PrefixChance(int pre, UnifiedRandom rand)
    {
        if (!this.IsRecordUnlocked)
            return false;
        return null;
    }
    public static bool UnlockStyleInventoryVisualMask { get; set; }
    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (!this.IsRecordUnlocked && !UnlockStyleInventoryVisualMask)
        {
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, position, null, drawColor, 0, new Vector2(14, 16), 0.85f, 0, 0);
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, position, null, Color.Lerp(Color.Transparent, Color.White with { A = 0 }, MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f), 0, new Vector2(14, 16), 0.85f, 0, 0);
            return false;
        }
        UnlockStyleInventoryVisualMask = false;
        return true;
    }
    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (!this.IsRecordUnlocked)
        {
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, Item.Center - Main.screenPosition, null, lightColor, rotation, new Vector2(14, 16), scale, 0, 0);
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, Item.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White with { A = 0 }, MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f), rotation, new Vector2(14, 16), scale, 0, 0);
            return false;
        }
        return true;
    }

    public override bool CanEquipAccessory(Player player, int slot, bool modded) => this.IsRecordUnlocked;
}
