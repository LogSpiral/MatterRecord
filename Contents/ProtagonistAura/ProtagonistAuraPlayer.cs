using MonoMod.Cil;
using System;
using Terraria.DataStructures;
namespace MatterRecord.Contents.ProtagonistAura;
public class ProtagonistAuraPlayer : ModPlayer
{
    public bool HasProtagonistAura;
    public int cDye;

    public override void ResetEffects()
    {
        HasProtagonistAura = false;
        base.ResetEffects();
    }

    public override void Load()
    {
        IL_Player.PlayerFrame += ProtagonistAuraModify;
        On_Player.UpdateItemDye += ProtagonistDye;
        base.Load();
    }

    private static void ProtagonistDye(On_Player.orig_UpdateItemDye orig, Player self, bool isNotInVanitySlot, bool isSetToHidden, Item armorItem, Item dyeItem)
    {
        if (armorItem.ModItem is ProtagonistAura && !(isSetToHidden && isNotInVanitySlot))
        {
            var mplr = self.GetModPlayer<ProtagonistAuraPlayer>();
            mplr.cDye = dyeItem.dye; // 获取当前饰品所染上的颜色，不知道有没有更好的做法，我记得example那边好像不支持染料
            mplr.HasProtagonistAura = true;
        }
        else
            orig.Invoke(self, isNotInVanitySlot, isSetToHidden, armorItem, dyeItem);
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (HasProtagonistAura)
        {
            drawInfo.cHead = 0; // 头发不染色(
            drawInfo.cBody = cDye;
            drawInfo.cLegs = cDye;
        }
        base.ModifyDrawInfo(ref drawInfo);
    }

    private void ProtagonistAuraModify(MonoMod.Cil.ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchRet()))
            return;
        for (int n = 0; n < 3; n++)
            if (!cursor.TryGotoPrev(i => i.MatchLdarg0()))
                return;
        cursor.Index++;
        cursor.EmitDelegate<Action<Player>>(player =>
        {
            if (player.GetModPlayer<ProtagonistAuraPlayer>().HasProtagonistAura)
            {
                var mItem = ModContent.GetInstance<ProtagonistAura>();
                player.head = EquipLoader.GetEquipSlot(Mod, mItem.Name, EquipType.Head);
                player.body = EquipLoader.GetEquipSlot(Mod, mItem.Name, EquipType.Body);
                player.legs = EquipLoader.GetEquipSlot(Mod, mItem.Name, EquipType.Legs);
                // 如果有物品就换装
                // 哦不过如果是想要原版的头盔(铁桶)再套在上面，就得自己加绘制层了
            }
        });
        cursor.EmitLdarg0();
    }
}