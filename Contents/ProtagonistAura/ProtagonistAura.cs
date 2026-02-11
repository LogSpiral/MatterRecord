using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria.DataStructures;

namespace MatterRecord.Contents.ProtagonistAura;
public class ProtagonistAura : ModItem
{
    public override void Load()
    {
        if (Main.dedServ)
            return;

        // 注册上相应部位的贴图
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
        EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
    }

    private void SetupDrawing()
    {
        // Since the equipment textures weren't loaded on the server, we can't have this code running server-side
        if (Main.dedServ)
            return;

        // 初始化一些信息
        int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
        int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
        int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

        ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false; // 隐藏原版头部绘制
        ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true; // 隐藏上身皮肤绘制
        ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true; // 隐藏皮肤手臂
        ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true; // 隐藏皮肤
    }

    public override void SetStaticDefaults()
    {
        SetupDrawing();
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.GetModPlayer<ProtagonistAuraPlayer>().HasProtagonistAura = !hideVisual;
        base.UpdateAccessory(player, hideVisual);
    }

    public override void UpdateVanity(Player player)
    {
        player.GetModPlayer<ProtagonistAuraPlayer>().HasProtagonistAura = true;
        base.UpdateVanity(player);
    }

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 14;
        Item.vanity = true;
        Item.rare = ItemRarityID.Pink;
        Item.value = Item.buyPrice(0, 2);
        Item.accessory = true;
    }
}