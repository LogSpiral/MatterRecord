using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.Utilities;

namespace MatterRecord.Contents.Recorder;

public class RecordBookItem : GlobalItem
{
    public override bool CanUseItem(Item item, Player player) => item.ModItem is not IRecordBookItem recordItem || recordItem.IsRecordUnlocked;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.ModItem is not IRecordBookItem recordItem || recordItem.IsRecordUnlocked) return;
        HashSet<string> whiteList = ["ItemName", "Favorite", "FavoriteDesc"];
        tooltips.RemoveAll(line => line.Mod is "Terraria" or "MatterRecord" && !whiteList.Contains(line.Name));
        tooltips.Add(new(Mod, "LockedRecords", Language.GetTextValue("Mods.MatterRecord.Items.LockedRecords")));
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (item.ModItem is IRecordBookItem recordItem && !recordItem.IsRecordUnlocked)
            return false;
        return null;
    }
    public static bool UnlockStyleInventoryVisualMask { get; set; }
    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.ModItem is IRecordBookItem recordItem && !recordItem.IsRecordUnlocked && !UnlockStyleInventoryVisualMask)
        {
            spriteBatch.Draw(ModAsset.RecordBook.Value, position, null, drawColor, 0, new Vector2(14, 16), 0.85f, 0, 0);
            spriteBatch.Draw(ModAsset.RecordBook.Value, position, null, Color.Lerp(Color.Transparent, Color.White with { A = 0 }, MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f), 0, new Vector2(14, 16), 0.85f, 0, 0);
            return false;
        }
        UnlockStyleInventoryVisualMask = false;
        return true;
    }
    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (item.ModItem is IRecordBookItem recordItem && !recordItem.IsRecordUnlocked)
        {
            spriteBatch.Draw(ModAsset.RecordBook.Value, item.Center - Main.screenPosition, null, lightColor, rotation, new Vector2(14, 16), scale, 0, 0);
            spriteBatch.Draw(ModAsset.RecordBook.Value, item.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White with { A = 0 }, MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f), rotation, new Vector2(14, 16), scale, 0, 0);
            return false;
        }
        return true;
    }

    public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded) => item.ModItem is not IRecordBookItem recordItem || recordItem.IsRecordUnlocked;

}