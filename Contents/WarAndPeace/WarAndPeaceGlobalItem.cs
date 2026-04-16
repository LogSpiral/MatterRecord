using MatterRecord.Contents.Recorder;
using Terraria.DataStructures;

namespace MatterRecord.Contents.WarAndPeace;

public class WarAndPeaceGlobalItem : GlobalItem
{
    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.type == ItemID.LicenseCat && RecorderSystem.ShouldSpawnRecordItem<WarAndPeace>()) 
        {
            player.QuickSpawnItem(new EntitySource_Misc("CatLicense"), ModContent.ItemType<WarAndPeace>());
            RecorderSystem.SetCooldown<WarAndPeace>();
        }

        base.OnConsumeItem(item, player);
    }
}