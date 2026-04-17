using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Localization;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

public class CompendiumOfMateriaMedica : ModItem,IRecordBookItem
{
    public static LocalizedText ShiftTooltipText { get; private set; }

    ItemRecords IRecordBookItem.RecordType => ItemRecords.CompendiumOfMateriaMedica;

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
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.StaffofRegrowth);
    }
    public override void Load()
    {
        IL_Player.PlaceThing_Tiles_BlockPlacementForAssortedThings += CompendiumOfMateriaMedicaSpawn;
    }

    private static void CompendiumOfMateriaMedicaSpawn(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(MoveType.After,
        i => i.MatchLdcI4(TileID.BloomingHerbs),
        i => i.MatchBneUn(out _)))
            return;

        cursor.EmitDelegate(() =>
        {
            if (!RecorderSystem.ShouldSpawnRecordItem<CompendiumOfMateriaMedica>())
                return;
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Harvesting Herb"), ModContent.ItemType<CompendiumOfMateriaMedica>());
            RecorderSystem.SetCooldown<CompendiumOfMateriaMedica>();
        });

        if (!cursor.TryGotoNext(i => i.MatchLdloc(22)))
            return;
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBrfalse(out _)))
            return;


        cursor.EmitDelegate(() =>
        {
            if (!RecorderSystem.ShouldSpawnRecordItem<CompendiumOfMateriaMedica>())
                return;
            Main.LocalPlayer.QuickSpawnItem(new EntitySource_Misc("Harvesting Herb"), ModContent.ItemType<CompendiumOfMateriaMedica>());
            RecorderSystem.SetCooldown<CompendiumOfMateriaMedica>();
        });
    }
}