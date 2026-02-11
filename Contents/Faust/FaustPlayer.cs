using System.IO;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.Faust;

public class FaustPlayer : ModPlayer
{
    public ulong ConsumedMoney { get; set; }

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(ConsumedMoney), ConsumedMoney);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        ConsumedMoney = tag.Get<ulong>(nameof(ConsumedMoney));
        base.LoadData(tag);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        FaustSync.Get(Player.whoAmI, ConsumedMoney).Send(toWho, fromWho);
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        FaustPlayer clone = (FaustPlayer)targetCopy;
        clone.ConsumedMoney = ConsumedMoney;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        FaustPlayer clone = (FaustPlayer)clientPlayer;

        if (ConsumedMoney != clone.ConsumedMoney)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void ModifyLuck(ref float luck)
    {
        luck -= ConsumedMoney / 1000000 * 0.01f;

        base.ModifyLuck(ref luck);
    }
}