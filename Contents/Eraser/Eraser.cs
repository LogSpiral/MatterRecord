using MatterRecord;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.Eraser
{
    public class Eraser : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 0;
            Item.width = 66;
            Item.height = 66;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 0;
            Item.value = Item.buyPrice(1);
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }

        public override void HoldItem(Player player)
        {
            if (player.itemAnimation > 0 && player.HeldItem == Item)
            {
                Rectangle swordHitbox = GetSwordHitbox(player);

                // 清除所有弹幕
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.Hitbox.Intersects(swordHitbox))
                    {
                        p.Kill();
                    }
                }

                // 清除粒子（Dust） - 使用位置判断
                for (int i = 0; i < Main.maxDust; i++)
                {
                    Dust d = Main.dust[i];
                    if (d.active && swordHitbox.Contains(d.position.ToPoint()))
                    {
                        d.active = false;
                    }
                }

                // 清除血污（Gore） - 使用位置判断
                for (int i = 0; i < Main.maxGore; i++)
                {
                    Gore g = Main.gore[i];
                    if (g.active && swordHitbox.Contains(g.position.ToPoint()))
                    {
                        g.active = false;
                    }
                }

                // 秒杀 NPC
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.life > 0 && npc.Hitbox.Intersects(swordHitbox))
                    {
                        npc.NPCLoot();
                        npc.position.X = 999999f;
                        npc.position.Y = 999999f;
                        npc.life = 0;
                        npc.active = false;
                    }
                }
            }
        }

        private Rectangle GetSwordHitbox(Player player)
        {
            int width = Item.width;
            int height = Item.height;
            Vector2 origin = player.Center;
            float direction = player.direction;
            float rotation = 0f;
            if (player.itemAnimation > 0)
            {
                float itemRot = player.itemRotation;
                if (direction == -1)
                    itemRot += MathHelper.Pi;
                rotation = itemRot;
            }
            float halfLength = width * 0.5f;
            Vector2 offset = new Vector2(halfLength * direction, 0).RotatedBy(rotation);
            Vector2 swordCenter = origin + offset;
            return new Rectangle((int)(swordCenter.X - width / 2), (int)(swordCenter.Y - height / 2), width, height);
        }
    }
}