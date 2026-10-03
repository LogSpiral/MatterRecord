using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace MatterRecord.Contents.WanJianGuiZong;

/// <summary>
/// 「天剑」化身的玩家侧表现：把人藏起来换成剑、飞行期间免击退并大幅减伤。
/// <para><see cref="Riding"/> 由 <see cref="TianJianProjectile"/> 每帧置位（弹幕 AI 跑在玩家更新之后），
/// 这里在 <see cref="UpdateEquips"/> 里消费掉——和巨型陆龟壳同一套写法。</para>
/// </summary>
public class TianJianPlayer : ModPlayer
{
    /// <summary>本帧是否正化身为剑（飞行 + 滑行）。由弹幕每帧置位，读完即清。</summary>
    public bool Riding;

    /// <inheritdoc />
    public override void UpdateEquips()
    {
        if (Riding)
        {
            // 90% 减伤 + 免疫击退。想改成完全无敌，把 endurance 换成 Player.immune = true 每帧刷即可
            Player.endurance += 0.9f;
            Player.noKnockback = true;
            Player.mount?.Dismount(Player);
        }

        Riding = false;
        base.UpdateEquips();
    }

    /// <inheritdoc />
    public override void PreUpdate()
    {
        if (Riding)
            Player.maxFallSpeed = 40f;

        base.PreUpdate();
    }

    /// <inheritdoc />
    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (Riding)
        {
            // 整个人换成剑，本体不画
            drawInfo.hideEntirePlayer = true;
            drawInfo.drawPlayer.invis = true;
            drawInfo.colorArmorBody = drawInfo.colorArmorHead = drawInfo.colorArmorLegs = default;
        }

        base.ModifyDrawInfo(ref drawInfo);
    }
}
