using MatterRecord;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace MatterRecord.Contents.Eraser
{
    public class Eraser : ModItem
    {
        // 客户端节流：同一实体在 N tick 内只发送一次擦除请求，避免挥动动画期间每帧重复发包
        private const int EraseCooldownTicks = 5;
        private static readonly Dictionary<int, int> _npcEraseCooldown = new();
        private static readonly Dictionary<int, int> _projEraseCooldown = new();

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
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            // 客户端：本地视觉移除 + 请求服务器权威移除（弹幕状态由服务器管理）
                            if (CanSendErase(_projEraseCooldown, i))
                                EraserEraseSync.Get(1, i).Send();
                            p.Kill();
                        }
                        else
                        {
                            KillProjectile(p);
                        }
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
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            // 客户端：仅本地视觉隐藏，并请求服务器权威秒杀，防止 NPC 被 SyncNPC 同步复活
                            if (CanSendErase(_npcEraseCooldown, i))
                                EraserEraseSync.Get(0, i).Send();
                            npc.active = false;
                            npc.life = 0;
                        }
                        else
                        {
                            KillNpc(npc);
                        }
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

        /// <summary>
        /// 判断某个实体是否已过冷却期、可以发送擦除请求（客户端节流用，避免挥动期间重复发包）。
        /// </summary>
        /// <param name="cooldown">该实体类型的冷却字典。</param>
        /// <param name="index">实体索引。</param>
        /// <returns>距离上次发送已超过冷却阈值时返回 true，并记录本次发送时刻。</returns>
        private static bool CanSendErase(Dictionary<int, int> cooldown, int index)
        {
            int now = (int)Main.GameUpdateCount;
            if (cooldown.TryGetValue(index, out int last) && now - last < EraseCooldownTicks)
                return false;
            cooldown[index] = now;
            return true;
        }

        /// <summary>
        /// 在服务器/单人模式下权威地秒杀 NPC：结算战利品、播放死亡效果、置为死亡并同步给所有客户端。
        /// </summary>
        /// <param name="npc">要秒杀的 NPC。</param>
        public static void KillNpc(NPC npc)
        {
            npc.NPCLoot();
            npc.HitEffect();
            npc.life = 0;
            npc.active = false;
            npc.netUpdate = true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
        }

        /// <summary>
        /// 在服务器/单人模式下权威地移除弹幕并同步给所有客户端。
        /// </summary>
        /// <param name="proj">要移除的弹幕。</param>
        public static void KillProjectile(Projectile proj)
        {
            proj.Kill();
            proj.netUpdate = true;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj.whoAmI);
        }
    }
}