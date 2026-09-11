using System.Text;
using Terraria.Localization;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica;

public class FragrantHerbs : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = false;
        Main.buffNoSave[Type] = false;
        Main.buffNoTimeDisplay[Type] = false;
    }

    /// <summary>
    /// 动态生成增益描述：列出当前所有生效的草药效果，每种草药一行。
    /// 药水触发与附近图格触发只要其一满足即视为生效（与 CompendiumPlayer 保持一致）。
    /// </summary>
    public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    {
        var player = Main.LocalPlayer;
        if (player == null || player.whoAmI != Main.myPlayer)
            return;

        var comp = player.GetModPlayer<CompendiumPlayer>();
        if (comp == null)
            return;

        // 最终生效标志 = 药水效果 OR 附近图格效果
        bool blinkroot = comp.potionBlinkroot || comp.proximityBlinkroot;
        bool daybloom = comp.potionDaybloom || comp.proximityDaybloom;
        bool deathweed = comp.potionDeathweed || comp.proximityDeathweed;
        bool fireblossom = comp.potionFireblossom || comp.proximityFireblossom;
        bool moonglow = comp.potionMoonglow || comp.proximityMoonglow;
        bool shiverthorn = comp.potionShiverthorn || comp.proximityShiverthorn;
        bool waterleaf = comp.potionWaterleaf || comp.proximityWaterleaf;

        var sb = new StringBuilder();

        // 统一走本地化键取值，避免英文语言环境下仍显示中文。
        // 注意：不能在此处缓存结果——ModifyBuffText 每次显示都会调用，语言切换后自然刷新。
        string T(string key) => Language.GetTextValue("Mods.MatterRecord.Buffs.FragrantHerbs." + key);

        // 每种草药一行，属性加成与命中附加合并显示
        if (blinkroot) sb.AppendLine(T("Blinkroot"));
        if (daybloom) sb.AppendLine(T("Daybloom"));
        if (deathweed) sb.AppendLine(T("Deathweed"));
        if (fireblossom) sb.AppendLine(T("Fireblossom"));
        if (moonglow) sb.AppendLine(T("Moonglow"));
        if (shiverthorn) sb.AppendLine(T("Shiverthorn"));
        if (waterleaf) sb.AppendLine(T("Waterleaf"));

        if (sb.Length == 0)
        {
            tip = T("TipNone");
            return;
        }

        tip = T("TipActive") + "\n" + sb.ToString().TrimEnd();
    }
}