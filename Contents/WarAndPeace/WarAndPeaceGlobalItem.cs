using MatterRecord.Contents.Recorder;

namespace MatterRecord.Contents.WarAndPeace;

public class WarAndPeaceGlobalItem : GlobalItem
{
    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.type == ItemID.LicenseCat && RecorderSystem.ShouldSpawnRecordItem<WarAndPeace>())
            player.QuickSpawnItem(item.GetSource_Misc("CatLicense"), ModContent.ItemType<WarAndPeace>());

        base.OnConsumeItem(item, player);
    }
}