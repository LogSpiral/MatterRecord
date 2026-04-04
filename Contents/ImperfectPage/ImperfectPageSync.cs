using NetSimplified;
using NetSimplified.Syncing;

namespace MatterRecord.Contents.ImperfectPage
{
    [AutoSync]
    internal class ImperfectPageSync : NetModule
    {
        private int _favoriteAmmoType;

        public static ImperfectPageSync Get(int favoriteAmmoType)
        {
            var packet = NetModuleLoader.Get<ImperfectPageSync>();
            packet._favoriteAmmoType = favoriteAmmoType;
            return packet;
        }

        public override void Receive()
        {
            // 在服务器上更新世界数据
            var system = ModContent.GetInstance<ImperfectPageSystem>();
            system.FavoriteAmmoType = _favoriteAmmoType;

            // 如果是服务器，则转发给所有客户端（不包括发送者）
            if (Main.netMode == NetmodeID.Server)
            {
                Get(_favoriteAmmoType).Send(-1, Sender);
            }
        }
    }
}