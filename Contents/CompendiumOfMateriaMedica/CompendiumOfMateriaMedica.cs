using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Terraria.DataStructures;
using Terraria.Localization;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

public class CompendiumOfMateriaMedica : ModItem, IRecordBookItem
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

    /// <summary>
    /// 未按住 Shift 时在主 tip 中提示「按住 Shift 查看草药效果」；
    /// 按住 Shift 时改为逐行列出各草药的增益效果。
    /// 由于本草纲目是饰品，独立侧栏 tip 会遮挡饰品栏，因此统一放在主 tip。
    /// </summary>
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!this.IsRecordUnlocked)
        {
            base.ModifyTooltips(tooltips);
            return;
        }

        bool shiftHeld = Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);

        if (!shiftHeld)
        {
            // 未按住 Shift：只添加提示行
            tooltips.Add(new TooltipLine(Mod, "ShiftHint", this.GetLocalizedValue("ShiftHint"))
            {
                OverrideColor = Color.Gray
            });
        }
        else
        {
            // 按住 Shift：把草药效果的多行文本拆成独立行添加
            string[] herbLines = ShiftTooltipText.Value.Split('\n');
            int index = 0;
            foreach (string raw in herbLines)
            {
                string text = raw.Trim();
                if (text.Length == 0)
                    continue;
                tooltips.Add(new TooltipLine(Mod, "HerbEffect" + index, text)
                {
                    OverrideColor = Color.LightGray
                });
                index++;
            }
        }

        base.ModifyTooltips(tooltips);
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