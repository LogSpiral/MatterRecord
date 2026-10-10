using MatterRecord.Contents.Rarities;
using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MatterRecord.Contents.EnAttendantGodot;

public class EnAttendantGodot : ModItem, IRecordBookItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ModContent.RarityType<ImmutableQuest>();
    }
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<EnAttendantGodotPlayer>().EnAttendantGodotEquipped = true;
    }

    /// <summary>
    /// 按住 Shift 时在主提示末尾展开「已收藏的敌怪旗帜」，标题 + 条目 + 灰色提示的排布与堂吉诃德的连击加成列表一致。
    /// <para>
    /// 列表是按住 Shift 时按背包现算的（只读、不发包），所以没装备饰品也能看；同步仍然只由
    /// <see cref="EnAttendantGodotPlayer.RebuildBannedList"/> 在装备时负责。
    /// </para>
    /// </summary>
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!this.IsRecordUnlocked) return;

        if (!ShiftHeld)
        {
            tooltips.Add(new TooltipLine(Mod, "ShiftHint", this.GetLocalizedValue("ShiftHint"))
            {
                OverrideColor = Color.Gray
            });
            return;
        }

        var mplr = Main.LocalPlayer.GetModPlayer<EnAttendantGodotPlayer>();
        List<int> banners = mplr.GetFavoritedBannerItems();

        if (banners.Count == 0)
        {
            tooltips.Add(new TooltipLine(Mod, "NoFavoritedBanner", this.GetLocalizedValue("NoFavoritedBanner"))
            {
                OverrideColor = Color.Gray
            });
        }
        else
        {
            // 标题 + 每条收藏的旗帜，按背包格顺序列出
            tooltips.Add(new TooltipLine(Mod, "FavoritedBanners", this.GetLocalizedValue("FavoritedBanners"))
            {
                OverrideColor = Color.Cyan
            });
            int index = 0;
            foreach (int type in banners)
            {
                tooltips.Add(new TooltipLine(Mod, "FavoritedBanner" + index++, Lang.GetItemNameValue(type))
                {
                    OverrideColor = Color.LightGray
                });
            }
        }

        // 没装备时禁用不生效，补一句提醒（原来这行一直是打在聊天栏里的）
        if (!mplr.EnAttendantGodotEquipped)
        {
            tooltips.Add(new TooltipLine(Mod, "ReEquipPlz", this.GetLocalizedValue("ReEquipPlz"))
            {
                OverrideColor = Color.Orange
            });
        }
    }

    private static bool ShiftHeld => Main.keyState.IsKeyDown(Keys.LeftShift) || Main.keyState.IsKeyDown(Keys.RightShift);

    public override string Texture => "MatterRecord/Contents/EnAttendantGodot/EnAttendantGodot";

    public ItemRecords RecordType => ItemRecords.EnAttendantGodot;
}
