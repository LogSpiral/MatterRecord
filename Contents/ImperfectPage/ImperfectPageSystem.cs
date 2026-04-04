using System.IO;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.ImperfectPage
{
    public class ImperfectPageSystem : ModSystem
    {
        // 存储的弹药物品类型，默认0表示未设置
        public int FavoriteAmmoType { get; set; } = 0;

        public override void SaveWorldData(TagCompound tag)
        {
            tag["favoriteAmmoType"] = FavoriteAmmoType;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            FavoriteAmmoType = tag.GetInt("favoriteAmmoType");
        }

        // 网络同步（如果需要多人模式）
        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(FavoriteAmmoType);
        }

        public override void NetReceive(BinaryReader reader)
        {
            FavoriteAmmoType = reader.ReadInt32();
        }
    }
}