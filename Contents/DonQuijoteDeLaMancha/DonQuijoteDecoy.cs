using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha;

/// <summary>
/// 风车嘲讽的位置欺骗所用的「玩家坐标寄存处」。
/// <para>
/// 敌怪 AI 期间把玩家挪到风车中心，AI 结束后由 <see cref="DonQuijoteGlobalNPC.PostAI"/> 还原。
/// 之所以单独抽出来，是为了让还原不依赖 <c>npc.target</c>：
/// 每只敌怪动的都是「玩家下标」，而不是「我现在的目标」。
/// </para>
/// <para>
/// 同一帧内多只敌怪嘲讽同一名玩家时，只登记最早的那份原始坐标，
/// 任何一次还原都写回同一份坐标，不会出现「还原到风车」的中间态。
/// </para>
/// </summary>
public static class DonQuijoteDecoy
{
    /// <summary>玩家下标 → 被挪走之前的原始 position。</summary>
    private static readonly Dictionary<int, Vector2> _savedPositions = new();

    /// <summary>
    /// 登记原始坐标并把玩家挪到风车中心。
    /// </summary>
    /// <param name="playerIndex">要挪动的玩家下标。</param>
    /// <param name="windmillCenter">风车中心，也就是敌怪这一帧「看到的」玩家位置。</param>
    /// <returns>确实挪动了返回 <c>true</c>；玩家无效或已登记过但坐标失效时返回 <c>false</c>。</returns>
    public static bool Begin(int playerIndex, Vector2 windmillCenter)
    {
        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return false;

        Player player = Main.player[playerIndex];
        if (player == null || !player.active || player.dead)
            return false;

        // 同一帧内第二次嘲讽同一名玩家时保留最早的原始坐标
        if (!_savedPositions.ContainsKey(playerIndex))
            _savedPositions[playerIndex] = player.position;

        player.position = windmillCenter - player.Size * 0.5f;
        return true;
    }

    /// <summary>
    /// 还原指定玩家的坐标。没有登记过就是空操作。
    /// </summary>
    /// <param name="playerIndex">登记过的玩家下标。</param>
    public static void Restore(int playerIndex)
    {
        if (!_savedPositions.TryGetValue(playerIndex, out Vector2 saved))
            return;

        _savedPositions.Remove(playerIndex);

        if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
            return;

        Main.player[playerIndex].position = saved;
    }

    /// <summary>
    /// 帧末兜底：把这一帧还没被还原的玩家全部拉回原位。
    /// </summary>
    public static void RestoreAll()
    {
        if (_savedPositions.Count == 0)
            return;

        foreach (KeyValuePair<int, Vector2> pair in _savedPositions)
        {
            if (pair.Key < 0 || pair.Key >= Main.maxPlayers)
                continue;

            Main.player[pair.Key].position = pair.Value;
        }

        _savedPositions.Clear();
    }

    /// <summary>丢弃所有登记（世界卸载、退出时用）。</summary>
    public static void Clear()
    {
        _savedPositions.Clear();
    }
}

/// <summary>
/// 位置欺骗的帧末兜底与状态清理。
/// </summary>
public class DonQuijoteDecoyRestoreSystem : ModSystem
{
    /// <summary>
    /// 本帧所有实体更新结束后统一兜底还原。
    /// <para>
    /// 时机必须在这里：NPC 更新在 <c>Main.Update</c> 里排在玩家更新之后，
    /// 玩家的位置包也是玩家更新阶段发的。只要本帧内还原，风车坐标就传不出去；
    /// 拖到下一帧再还原，本端已经把假坐标当真实位置上报过了，等于真的传送。
    /// </para>
    /// </summary>
    public override void PostUpdateEverything()
    {
        DonQuijoteDecoy.RestoreAll();
    }

    /// <summary>
    /// 卸载世界时同时清掉风车坐标表：<see cref="DonQuijoteDeLaMancha.WindmillPositions"/>
    /// 与寄存表都是静态的，不清的话重进世界会残留上一个世界/上一局的坐标。
    /// </summary>
    public override void OnWorldUnload()
    {
        DonQuijoteDecoy.Clear();
        DonQuijoteDeLaMancha.WindmillPositions.Clear();
    }
}
