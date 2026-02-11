using NetSimplified;
using NetSimplified.Syncing;
using Terraria.ModLoader.Config;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

[AutoSync]
internal class DonQuijoteDeLaManchaItemDefinitionSync : NetModule
{
    private byte _whoAmI;
    private string _itemDefinition;
    public static DonQuijoteDeLaManchaItemDefinitionSync Get(int whoAmI, string itemDefinition)
    {
        var packet = NetModuleLoader.Get<DonQuijoteDeLaManchaItemDefinitionSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._itemDefinition = itemDefinition;
        return packet;
    }
    public override void Receive()
    {
        var player = Main.player[_whoAmI];
        var modPlayer = player.GetModPlayer<DonQuijoteDeLaManchaPlayer>();
        modPlayer.itemDefinition = new ItemDefinition(_itemDefinition);
        if (Main.dedServ)
            Get(_whoAmI, _itemDefinition).Send(-1, Sender);
    }
}
