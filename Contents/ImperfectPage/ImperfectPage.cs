using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Localization;
using RecorderNPC = MatterRecord.Contents.Recorder.Recorder;

namespace MatterRecord.Contents.ImperfectPage
{
    public class ImperfectPage : ModItem
    {
        internal static bool Active;

        // 本地化键（仅保留弹药显示相关）
        public static LocalizedText CurrentAmmoText { get; private set; }
        public static LocalizedText NoAmmoText { get; private set; }

        public override void SetStaticDefaults()
        {
            CurrentAmmoText = this.GetLocalization("CurrentAmmo");
            NoAmmoText = this.GetLocalization("NoAmmo");
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.rare = ItemRarityID.Blue;
            Item.UseSound = SoundID.Item6;
            Item.maxStack = 1;
            Item.consumable = false;
        }

        public override bool CanUseItem(Player player) => true;

        public override bool? UseItem(Player player)
        {
            int npcType = ModContent.NPCType<RecorderNPC>();
            bool exists = false;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.type == npcType)
                {
                    exists = true;
                    Vector2 targetCenter = player.Center + new Vector2(0, -50);
                    npc.position = targetCenter - new Vector2(npc.width / 2, npc.height / 2);
                    npc.netUpdate = true;
                    break;
                }
            }

            if (!exists)
            {
                Vector2 spawnPos = player.Center + new Vector2(0, -100);
                while (Collision.SolidTiles(spawnPos - new Vector2(10, 10), 20, 20))
                {
                    spawnPos.Y -= 16;
                }
                int npcIndex = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, npcType);
                if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
                {
                    NPC npc = Main.npc[npcIndex];
                    npc.homeTileX = (int)(player.Center.X / 16);
                    npc.homeTileY = (int)(player.Center.Y / 16);
                    npc.direction = 1;
                    npc.netUpdate = true;
                }
            }

            return true;
        }

        public override bool CanRightClick() => true;
        public override bool ConsumeItem(Player player) => false;

        public override void RightClick(Player player)
        {
            Active = !Active;
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item4);
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (Active)
            {
                float factor = Main.GlobalTimeWrappedHourly % 1;
                spriteBatch.Draw(TextureAssets.Item[Type].Value, position, frame, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), 0, origin, scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);
            }
            base.PostDrawInInventory(spriteBatch, position, frame, drawColor, itemColor, origin, scale);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            if (Active)
            {
                float factor = Main.GlobalTimeWrappedHourly % 1;
                spriteBatch.Draw(TextureAssets.Item[Type].Value, Item.Center - Main.screenPosition, null, Color.Red with { A = 0 } * (0.5f - MathF.Cos(factor * MathHelper.TwoPi) * 0.5f), rotation, TextureAssets.Item[Type].Value.Size() / 2, scale * (1 + .5f * MathF.Pow(factor, 3)), 0, 0);
            }
            base.PostDrawInWorld(spriteBatch, lightColor, alphaColor, rotation, scale, whoAmI);
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var system = ModContent.GetInstance<ImperfectPageSystem>();
            int ammoType = system.FavoriteAmmoType;

            string ammoDisplay;
            if (ammoType > 0 && ContentSamples.ItemsByType.ContainsKey(ammoType))
            {
                string ammoName = Lang.GetItemName(ammoType).Value;
                ammoDisplay = $"[i:{ammoType}] {ammoName}";
            }
            else
            {
                ammoDisplay = NoAmmoText.Value;
            }
            string ammoLineText = string.Format(CurrentAmmoText.Value, ammoDisplay);
            var ammoLine = tooltips.Find(line => line.Name == "CurrentAmmo");
            if (ammoLine == null)
            {
                ammoLine = new TooltipLine(Mod, "CurrentAmmo", ammoLineText);
                tooltips.Add(ammoLine);
            }
            else
            {
                ammoLine.Text = ammoLineText;
            }
        }
    }
}