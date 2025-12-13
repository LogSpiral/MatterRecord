using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.Recorder;

public partial class Recorder
{
    const string SHOPNAME = "Shop";
    public override void AddShops()
    {

        var npcShop = new NPCShop(Type, SHOPNAME);
        foreach (var pair in ContentSamples.ItemsByType)
        {
            if (pair.Value.ModItem is IRecordBookItem recordBook)
                npcShop.Add(pair.Key, new Condition("", () => RecorderSystem.CheckUnlock(recordBook)));
        }
        npcShop.Register();
    }

}
