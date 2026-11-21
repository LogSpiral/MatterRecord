using System;
using Terraria.Audio;
using Terraria.UI;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

/// <summary>
/// 梦匣的自动收纳：把背包和银行里的梦境物品按类别塞进对应的格子。
/// <para>参考更好的体验里旗帜盒 / 药水袋的中键收纳：只搬梦境物品，
/// 格满了就留在原地，不丢东西。</para>
/// </summary>
public static class DreamStorage
{
    private static bool _middleHeld;

    /// <summary>
    /// 中键按下的那一帧返回 true；按住不放只会触发一次。
    /// 只在悬停相关格子时调用，松开后自动复位。
    /// </summary>
    public static bool MiddlePressed()
    {
        bool pressed = Main.mouseMiddle && !_middleHeld;
        _middleHeld = Main.mouseMiddle;
        return pressed;
    }

    /// <summary>
    /// 把玩家身上的梦境物品收进梦匣。返回是否有东西被搬动。
    /// <para>只处理本机玩家：梦匣是本地存档数据，联机时别的客户端看不到。</para>
    /// </summary>
    public static bool AutoStore(Player player)
    {
        if (player.whoAmI != Main.myPlayer)
            return false;

        DreamPlayer dream = player.GetModPlayer<DreamPlayer>();
        bool moved = false;

        // 背包：跳过 58 号位，它是鼠标物品的镜像，单独处理避免和 Main.mouseItem 打架
        for (int i = 0; i < player.inventory.Length; i++)
        {
            if (i == 58)
                continue;
            moved |= Store(ref player.inventory[i], dream);
        }

        // 银行四页也一起收，和更好的体验保持一致
        Item[][] banks = [player.bank.item, player.bank2.item, player.bank3.item, player.bank4.item];
        foreach (Item[] bank in banks)
            for (int i = 0; i < bank.Length; i++)
                moved |= Store(ref bank[i], dream);

        // 光标上拿着的那一叠
        if (!Main.mouseItem.IsAir)
            moved |= Store(ref Main.mouseItem, dream);

        if (moved)
        {
            SoundEngine.PlaySound(SoundID.Grab);
            Recipe.FindRecipes();
        }

        return moved;
    }

    /// <summary>
    /// 把一叠物品并进它对应的梦匣格子；装不下就只搬一部分，一个都装不下时返回 false。
    /// </summary>
    private static bool Store(ref Item item, DreamPlayer dream)
    {
        if (item.IsAir || item.stack <= 0)
            return false;

        int slot = DreamHelper.DreamSlotForItem(item.type);
        if (slot < 0)
            return false;

        Item target = dream.dreamItemSlots[slot];
        if (!target.IsAir && target.type != item.type)
            return false;

        int maxStack = target.IsAir ? item.maxStack : target.maxStack;
        int space = maxStack - (target.IsAir ? 0 : target.stack);
        if (space <= 0)
            return false;

        int moved = Math.Min(space, item.stack);
        if (target.IsAir)
        {
            target.SetDefaults(item.type);
            target.stack = moved;
        }
        else
        {
            target.stack += moved;
        }

        item.stack -= moved;
        if (item.stack <= 0)
            item.TurnToAir();

        return true;
    }
}
