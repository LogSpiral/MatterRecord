using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace MatterRecord.Contents.Recorder;

public class RecordLockItem : GlobalItem
{
    public override bool CanUseItem(Item item, Player player)
    {
        if (item.ModItem is IRecordBookItem recordBookItem)
            return recordBookItem.IsRecordUnlocked;
        return true;
    }

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        if (item.ModItem is IRecordBookItem recordBookItem && !recordBookItem.IsRecordUnlocked)
        {
            HashSet<string> whiteList = ["ItemName", "Favorite", "FavoriteDesc"];
            tooltips.RemoveAll(line => line.Mod is "Terraria" or "MatterRecord" && !whiteList.Contains(line.Name));

            tooltips.Add(new(Mod, "LockedRecords", Language.GetTextValue("Mods.MatterRecord.Items.LockedRecords")));
        }
    }

    public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
    {
        if (item.ModItem is IRecordBookItem recordBookItem && !recordBookItem.IsRecordUnlocked)
            return false;
        return null;
    }
    public static bool UnlockStyleInventoryVisualMask { get; set; }
    public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        if (item.ModItem is IRecordBookItem recordBookItem && !recordBookItem.IsRecordUnlocked && !UnlockStyleInventoryVisualMask)
        {
            UnlockStyleInventoryVisualMask = false;
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, position, null, drawColor, 0, new Vector2(14, 16), 0.85f, 0, 0);
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, position, null, Color.Lerp(Color.Transparent, Color.White with { A = 0 }, MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f), 0, new Vector2(14, 16), 0.85f, 0, 0);
            return false;
        }
        return true;
    }
    public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        if (item.ModItem is IRecordBookItem recordBookItem && !recordBookItem.IsRecordUnlocked)
        {
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, item.Center - Main.screenPosition, null, lightColor, rotation, new Vector2(14, 16), scale, 0, 0);
            spriteBatch.Draw(TextureAssets.Item[ItemID.SpellTome].Value, item.Center - Main.screenPosition, null, Color.Lerp(Color.Transparent, Color.White with { A = 0 }, MathF.Cos(Main.GlobalTimeWrappedHourly * 6) * .25f + .25f), rotation, new Vector2(14, 16), scale, 0, 0);
            return false;
        }
        return true;
    }
}


public static class RecordItemExtension
{
    extension(IRecordBookItem recordBookItem)
    {
        public bool IsRecordUnlocked => RecorderSystem.CheckUnlock(recordBookItem);
    }
}
