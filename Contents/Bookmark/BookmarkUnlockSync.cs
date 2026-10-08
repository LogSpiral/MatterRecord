using NetSimplified;
using System.IO;

namespace MatterRecord.Contents.Bookmark;

/// <summary>
/// 「事象记录」专用饰品栏的永久解锁状态同步（双向）。
/// <para>栏位启用判定（<see cref="BookmarkSlot.IsEnabled"/>）依赖 <see cref="BookmarkPlayer.EternalBookmarkUnlocked"/>，
/// 而 ModPlayer 的存档字段默认只存在于本机：服务端与其它客户端必须显式收到，
/// 否则他们那边栏位是关闭的，栏位内饰品的 UpdateAccessory 不会执行。</para>
/// </summary>
internal class BookmarkUnlockSync : NetModule
{
    /// <summary>解锁状态所属的玩家索引。</summary>
    private byte _whoAmI;

    /// <summary>是否已永久解锁。</summary>
    private bool _unlocked;

    public static BookmarkUnlockSync Get(int whoAmI, bool unlocked)
    {
        var packet = NetModuleLoader.Get<BookmarkUnlockSync>();
        packet._whoAmI = (byte)whoAmI;
        packet._unlocked = unlocked;
        return packet;
    }

    public override void Send(ModPacket p)
    {
        p.Write(_whoAmI);
        p.Write(_unlocked);
    }

    public override void Read(BinaryReader r)
    {
        _whoAmI = r.ReadByte();
        _unlocked = r.ReadBoolean();
    }

    public override void Receive()
    {
        if (_whoAmI >= Main.maxPlayers)
            return;

        Main.player[_whoAmI].GetModPlayer<BookmarkPlayer>().EternalBookmarkUnlocked = _unlocked;

        // 服务端收到客户端的上报后转发给其它客户端，让所有人都知道这个人的栏位已解锁
        if (Main.netMode == NetmodeID.Server)
            Get(_whoAmI, _unlocked).Send(-1, Sender);
    }
}
