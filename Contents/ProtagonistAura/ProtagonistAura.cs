using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.ProtagonistAura
{
    [AutoloadEquip(EquipType.Head,EquipType.Body,EquipType.Legs)]
    public class ProtagonistAura : ModItem 
    {
        public override void SetStaticDefaults()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;

            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;

            int equipSlotLeg = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLeg] = true;
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
    public class ClothierModify : GlobalNPC 
    {
        public override void ModifyShop(NPCShop shop)
        {
            shop.Add<ProtagonistAura>();
            base.ModifyShop(shop);
        }
    }
}
