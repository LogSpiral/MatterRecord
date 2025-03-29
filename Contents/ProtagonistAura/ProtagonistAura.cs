using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace MatterRecord.Contents.ProtagonistAura
{
    //[AutoloadEquip(EquipType.Head,EquipType.Body,EquipType.Legs)]
    public class ProtagonistAura : ModItem
    {
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Head}", EquipType.Head, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Body}", EquipType.Body, this);
            EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Legs}", EquipType.Legs, this);
        }
        private void SetupDrawing()
        {
            // Since the equipment textures weren't loaded on the server, we can't have this code running server-side
            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlotHead = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Head);
            int equipSlotBody = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
            int equipSlotLegs = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);

            ArmorIDs.Head.Sets.DrawHead[equipSlotHead] = false;
            ArmorIDs.Body.Sets.HidesTopSkin[equipSlotBody] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlotBody] = true;
            ArmorIDs.Legs.Sets.HidesBottomSkin[equipSlotLegs] = true;
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
                mplr.cDye = dyeItem.dye;
                mplr.HasProtagonistAura = true;
            }
            else
                orig.Invoke(self, isNotInVanitySlot, isSetToHidden, armorItem, dyeItem);
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (HasProtagonistAura)
            {
                drawInfo.cHead = 0;
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
                }
            });
            cursor.EmitLdarg0();
        }
    }
    public class ClothierModify : GlobalNPC
    {
        public override void ModifyShop(NPCShop shop)
        {
            if (shop.NpcType == NPCID.Clothier)
                shop.Add<ProtagonistAura>();
            base.ModifyShop(shop);
        }
    }
    public class AuraLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);
        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            var plr = drawInfo.drawPlayer;
            var mplr = plr.GetModPlayer<ProtagonistAuraPlayer>();
            if (!mplr.HasProtagonistAura) return;
            Vector2 center = plr.Center + Vector2.UnitY * plr.gfxOffY;
            if (!Main.gameMenu)
                center -= Main.screenPosition;
            center = new Vector2((int)center.X, (int)center.Y);

            int offset = mplr.cDye switch
            {
                1 => 0,
                17 => 1,
                54 => 2,
                _ => -1
            };
            if (offset < 0) return;
            int offsetY = (plr.bodyFrame.Top / 56) switch
            {
                >= 7 and <= 9 => -2,
                >= 14 and <= 16 => -2,
                _ => 0
            };
            float t = MathHelper.SmoothStep(0, 1, Main.GlobalTimeWrappedHourly % 1);
            drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/ProtagonistAura/Aura_Glow").Value,
    center + new Vector2(4 + (plr.direction < 0 ? -8 : 0), -23 + offsetY), new Rectangle(offset * 32, 0, 32, 32), (Color.White * (1 - MathF.Cos(MathHelper.TwoPi * MathF.Sqrt(t))) * .75f) with { A = 0 }, 0, new(16), new Vector2(1, 0.6f) * (1 + .5f * t), plr.direction < 0 ? SpriteEffects.FlipHorizontally : 0, 0));

            drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/ProtagonistAura/Aura").Value,
                center + new Vector2(4 + (plr.direction < 0 ? -8 : 0), -23 + offsetY), new Rectangle(offset * 32, 0, 32, 32), Color.White, 0, new(16), new Vector2(1, 0.6f), plr.direction < 0 ? SpriteEffects.FlipHorizontally : 0, 0));

            drawInfo.DrawDataCache.Add(new DrawData(ModContent.Request<Texture2D>("MatterRecord/Contents/ProtagonistAura/Aura").Value,
    center + new Vector2(4 + (plr.direction < 0 ? -8 : 0), -21 + offsetY), new Rectangle(96, 0, 32, offset == 2 ? 12 : 14), Color.White, 0, new(16), 1, plr.direction < 0 ? SpriteEffects.FlipHorizontally : 0, 0));
            //Main.spriteBatch.DrawString(FontAssets.MouseText.Value, (plr.bodyFrame.Top / 56).ToString(), center + Vector2.UnitX * 32, Color.White);
            /*switch (mplr.cDye)
            {
                case 1:
                    {
                        drawInfo.DrawDataCache.Add(new DrawData(TextureAssets.MagicPixel.Value, center, new Rectangle(0, 0, 1, 1), Color.White, 0, new Vector2(.5f), new Vector2(128, 4), 0, 0));
                        break;
                    }
                case 17:
                    {
                        break;
                    }
                case 54:
                    {
                        drawInfo.DrawDataCache.Add(new DrawData(TextureAssets.MagicPixel.Value, center + new Vector2(4, -24), new Rectangle(0, 0, 1, 1), Color.Gray, 0.1f, new Vector2(.5f), new Vector2(24, 4), 0, 0));
                        drawInfo.DrawDataCache.Add(new DrawData(TextureAssets.MagicPixel.Value, center + new Vector2(4, -24), new Rectangle(0, 0, 1, 1), Color.Gray, -0.65f, new Vector2(.5f), new Vector2(18, 4), 0, 0));

                        drawInfo.DrawDataCache.Add(new DrawData(TextureAssets.MagicPixel.Value, center + new Vector2(4, -24), new Rectangle(0, 0, 1, 1), Color.White, 0.1f, new Vector2(.5f), new Vector2(22, 2), 0, 0));
                        drawInfo.DrawDataCache.Add(new DrawData(TextureAssets.MagicPixel.Value, center + new Vector2(4, -24), new Rectangle(0, 0, 1, 1), Color.White, -0.65f, new Vector2(.5f), new Vector2(16, 2), 0, 0));
                        break;
                    }
            }*/
        }
    }
}
