using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System.Linq;  

namespace MatterRecord.Contents.EmeraldTablet
{
    public class BossBagRecordPlayer : ModPlayer
    {
        public HashSet<int> ExchangedBossBags = new HashSet<int>();

        public override void SaveData(TagCompound tag)
        {
            tag["ExchangedBossBags"] = ExchangedBossBags.ToList();
        }

        public override void LoadData(TagCompound tag)
        {
            ExchangedBossBags.Clear();
            var list = tag.GetList<int>("ExchangedBossBags");
            if (list != null)
                ExchangedBossBags = new HashSet<int>(list);
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
}