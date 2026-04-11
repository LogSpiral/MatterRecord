using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Terraria.Localization;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

public class CompendiumOfMateriaMedica : RecordBookItem
{
    public static LocalizedText ShiftTooltipText { get; private set; }

    public override ItemRecords RecordType => ItemRecords.CompendiumOfMateriaMedica;

    public override void SetStaticDefaults()
    {
        ShiftTooltipText = this.GetLocalization("ShiftTooltip");
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 28;
        Item.accessory = true;
        Item.value = Item.buyPrice(copper: 5);
        Item.rare = ItemRarityID.Quest;
        Item.maxStack = 1;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!this.IsRecordUnlocked) return;
        var modPlayer = player.GetModPlayer<CompendiumPlayer>();
        modPlayer.hasCompendium = true;
        modPlayer.showCompendiumVisual = !hideVisual;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!this.IsRecordUnlocked)
        {
            base.ModifyTooltips(tooltips);
            return;
        }
        // 检测 Shift 键是否按住
        bool shiftPressed = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);

        // 移除已有的 ShiftTooltip 行（避免重复）
        var existing = tooltips.Find(line => line.Name == "ShiftTooltip");
        if (existing != null)
            tooltips.Remove(existing);

        // 如果按住 Shift，则添加详细的草药效果说明
        if (shiftPressed)
        {
            var line = new TooltipLine(Mod, "ShiftTooltip", ShiftTooltipText.Value);
            tooltips.Add(line);
        }
    }
}