using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.EmeraldTablet;

public class BossBagRecordPlayer : ModPlayer
{
    public HashSet<int> ExchangedBossBags = [];

    public Item ItemToExchange { get; set; }

    public override void SaveData(TagCompound tag)
    {
        tag["ExchangedBossBags"] = ExchangedBossBags.ToList();
        if (ItemToExchange != null && !ItemToExchange.IsAir)
            tag["e"] = ItemIO.Save(ItemToExchange);
    }

    public override void LoadData(TagCompound tag)
    {
        ExchangedBossBags.Clear();
        var list = tag.GetList<int>("ExchangedBossBags");
        if (list != null)
            ExchangedBossBags = [.. list];

        if(tag.TryGet<Item>("e",out var value))
            ItemToExchange = value;
    }

    public void RecordExchange(int itemType)
    {
        ExchangedBossBags.Add(itemType);
    }

    public bool HasExchanged(int itemType)
    {
        return ExchangedBossBags.Contains(itemType);
    }
}