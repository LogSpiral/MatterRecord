using MatterRecord.Contents.Recorder;
using System;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using System.Linq;

namespace MatterRecord.Contents.TheCountOfMonteCristo
{
    public class MonteCristoPlayer : ModPlayer
    {
        private const int MaxChargeFrames = 600;      // 10 秒蓄力上限
        private const int TeleportThreshold = 300;    // 5 秒 -> 传送
        private const int HopeThreshold = 600;        // 10 秒 -> 希望
        private const int RevengeDurationFrames = 1200; // 复仇 20 秒

        private int delayedRespawnFrames = 0;
        private bool wasDead = false;
        private bool hasGivenBuffThisDeath = false;
        private Vector2 deathPosition = Vector2.Zero;
        private bool hasDeathPosition = false;
        private bool hasMonteCristoEquippedCache = false;

        private bool _hopeBlockActive = false;
        private bool _played5sSound = false;
        private bool _played10sSound = false;

        public int CurrentDelayedFrames => delayedRespawnFrames;
        public bool ShouldShowProgressBar => hasMonteCristoEquippedCache && Player.dead && Player.respawnTimer > 0;

        /// <summary>
        /// 最近一次死亡的位置，专供「基督山伯爵」的获取判定使用。
        /// 与 <see cref="deathPosition"/> 不同：该值在复活后不会被清空，每次死亡都会覆盖，
        /// 直至玩家离开世界才重置。
        /// </summary>
        public Vector2 LastDeathPosition { get; private set; }

        /// <summary>是否已经记录过至少一次死亡点。</summary>
        public bool HasLastDeathPosition { get; private set; }

        /// <summary>
        /// 「基督山伯爵」获取判定范围：以死亡点为中心的 1024×1024 像素正方形区域，
        /// 因此这里取半边长 512 像素。
        /// </summary>
        public const float RecordKillRangeHalfSize = 512f;

        /// <summary>
        /// 本次死亡时原版实际掉落的钱币总额（单位：铜币，已含原版的难度折扣等计算）。
        /// 由 <see cref="MonteCristoRecordDropCoins"/> 在死亡流程中写入，每次死亡覆盖旧值。
        /// </summary>
        public long LastDeathDroppedCoins { get; private set; }

        /// <summary>
        /// 死亡后在死亡点范围内累计拾取到的钱币面值（单位：铜币）。
        /// 每次死亡时清零，由拾取链路累加，用于「讨回失落财宝」的获取判定。
        /// </summary>
        public long NearbyPickedCoins { get; private set; }

        /// <summary>
        /// 「基督山伯爵」拾取累计判定的范围半边长（像素）：以死亡点为中心的正方形。
        /// 与 <see cref="RecordKillRangeHalfSize"/> 同为 512，保持两处方形判定语义一致。
        /// </summary>
        private const int CoinRangeHalfSize = 512;

        /// <summary>
        /// 订阅原版「死亡掉落钱币」的 detour。
        /// </summary>
        public override void Load()
        {
            On_Player.DropCoins += MonteCristoRecordDropCoins;
            base.Load();
        }

        /// <summary>
        /// 退订 detour，避免模组热重载后重复注册同一条钩子。
        /// </summary>
        public override void Unload()
        {
            On_Player.DropCoins -= MonteCristoRecordDropCoins;
            base.Unload();
        }

        /// <summary>
        /// 拦截原版死亡掉落钱币流程：记录本次死亡实际掉落的铜币总额，并清零「附近拾取额」，
        /// 为「死里逃生后讨回财宝」的获取判定准备数据。
        /// </summary>
        /// <param name="orig">原版 <c>Player.DropCoins</c> 的原始实现。</param>
        /// <param name="self">正在执行死亡掉落流程的玩家。</param>
        /// <returns>原版实现的返回值（1 表示确实掉落了钱币，0 表示未掉落）。</returns>
        private static long MonteCristoRecordDropCoins(On_Player.orig_DropCoins orig, Player self)
        {
            // 必须先执行原版逻辑：原版会把本次计算出的铜币总额写入 Player.lostCoins
            long result = orig(self);

            // 只记录到本地玩家自己的 ModPlayer。服务器与旁观端收到 PlayerDeath 消息时
            // 也会重放死亡流程，若不加此守卫，会把这些端的记录一并污染。
            if (self.whoAmI == Main.myPlayer)
            {
                var mp = self.GetModPlayer<MonteCristoPlayer>();
                mp.LastDeathDroppedCoins = self.lostCoins;
                mp.NearbyPickedCoins = 0;
            }

            return result;
        }

        private bool HasMonteCristoEquipped()
        {
            int expectedType = ModContent.ItemType<TheCountOfMonteCristo>();
            if (expectedType == 0) return false;
            int extraSlots = Player.GetAmountOfExtraAccessorySlotsToShow();
            int startIndex = 3;
            int endIndex = startIndex + 5 + extraSlots;
            for (int i = startIndex; i < endIndex; i++)
            {
                Item item = Player.armor[i];
                if (item != null && !item.IsAir && item.type == expectedType)
                    return true;
            }
            return false;
        }

        public override void UpdateDead()
        {
            if (Player.whoAmI != Main.myPlayer) return;

            hasMonteCristoEquippedCache = HasMonteCristoEquipped();
            if (!hasMonteCristoEquippedCache) return;

            if (Player.respawnTimer > 0 && Main.mouseRight && delayedRespawnFrames < MaxChargeFrames)
            {
                // 当 respawnTimer 处于整秒（mod == 0）或即将变成整秒（mod == 1）时，
                // 放开一帧，让原版正常递减并触发读秒音效，避免卡在整秒上连响。
                int mod = Player.respawnTimer % 60;
                if (mod != 0 && mod != 1)
                {
                    Player.respawnTimer += 1;
                }

                delayedRespawnFrames += 1;

                // ---- 阶段音效提醒 ----
                if (!_played5sSound && delayedRespawnFrames >= TeleportThreshold)
                {
                    _played5sSound = true;
                    SoundEngine.PlaySound(SoundID.Item29, Player.Center);
                }
                if (!_played10sSound && delayedRespawnFrames >= HopeThreshold)
                {
                    _played10sSound = true;
                    SoundEngine.PlaySound(SoundID.Item4, Player.Center);
                }
            }
        }

        public override void PostUpdate()
        {
            if (Player.whoAmI != Main.myPlayer) return;

            if (!Player.dead)
                hasMonteCristoEquippedCache = HasMonteCristoEquipped();

            bool isDead = Player.dead;
            if (wasDead && !isDead && !hasGivenBuffThisDeath)
            {
                // 10 秒 -> 希望
                if (delayedRespawnFrames >= HopeThreshold)
                {
                    Player.AddBuff(ModContent.BuffType<HopeBuff>(), 2);
                }

                // 5 秒 -> 传送
                if (delayedRespawnFrames >= TeleportThreshold)
                {
                    bool hasOtherPlayer = Main.player.Any(p => p != null && p.active && p.whoAmI != Player.whoAmI);
                    bool hasBoss = Main.npc.Any(n => n.active && n.boss);

                    Vector2 targetPosition;
                    if (hasOtherPlayer && hasBoss)
                    {
                        Player nearest = null;
                        float nearestDistSq = float.MaxValue;
                        Vector2 myPos = Player.Center;
                        for (int i = 0; i < Main.maxPlayers; i++)
                        {
                            Player other = Main.player[i];
                            if (other != null && other.active && other.whoAmI != Player.whoAmI)
                            {
                                float d2 = Vector2.DistanceSquared(myPos, other.Center);
                                if (d2 < nearestDistSq)
                                {
                                    nearestDistSq = d2;
                                    nearest = other;
                                }
                            }
                        }
                        targetPosition = nearest != null
                            ? nearest.position
                            : (hasDeathPosition ? deathPosition : Player.position);
                    }
                    else
                    {
                        targetPosition = hasDeathPosition ? deathPosition : Player.position;
                    }

                    Player.Teleport(targetPosition, 0);
                    Player.velocity = Vector2.Zero;
                }

                delayedRespawnFrames = 0;
                hasGivenBuffThisDeath = true;
                hasDeathPosition = false;

                _played5sSound = false;
                _played10sSound = false;
            }
            wasDead = isDead;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            hasGivenBuffThisDeath = false;
            delayedRespawnFrames = 0;
            wasDead = true;
            deathPosition = Player.position;
            hasDeathPosition = true;
            // 同步记录给「基督山伯爵」获取判定用的长期死亡点：每次死亡覆盖旧值，
            // 且不在复活流程里清空（复活传送仍使用上面的 deathPosition / hasDeathPosition）。
            LastDeathPosition = Player.position;
            HasLastDeathPosition = true;
            // 兜底：即使 detour 因异常路径未走到，也要保证拾取累计从本次死亡重新开始
            NearbyPickedCoins = 0;
            hasMonteCristoEquippedCache = HasMonteCristoEquipped();
            _played5sSound = false;
            _played10sSound = false;
        }

        public override void OnEnterWorld()
        {
            hasGivenBuffThisDeath = false;
            delayedRespawnFrames = 0;
            wasDead = false;
            hasDeathPosition = false;
            // 进入世界时重置长期死亡点，避免把上一个世界的位置带进新世界
            LastDeathPosition = Vector2.Zero;
            HasLastDeathPosition = false;
            // 离开并重进世界时一并清空钱币记录，避免把上一个世界的数据带过来
            LastDeathDroppedCoins = 0;
            NearbyPickedCoins = 0;
            hasMonteCristoEquippedCache = HasMonteCristoEquipped();
            _played5sSound = false;
            _played10sSound = false;
        }

        // ====================「讨回失落财宝」获取方式 ====================

        /// <summary>
        /// 把钱币物品类型折算成铜币面值。
        /// </summary>
        /// <param name="itemType">物品类型 ID。</param>
        /// <returns>该物品对应的铜币面值；非钱币返回 0。</returns>
        private static long GetCoinCopperValue(int itemType)
        {
            if (itemType == ItemID.CopperCoin) return 1L;
            if (itemType == ItemID.SilverCoin) return 100L;
            if (itemType == ItemID.GoldCoin) return 10000L;
            if (itemType == ItemID.PlatinumCoin) return 1000000L;
            return 0L;
        }

        /// <summary>
        /// 玩家拾取物品时累计「死亡点附近拾回的钱币」，并在拾回额超过死亡掉落额时触发获取。
        /// <para>只做统计不做截留：返回 true 让钱币照常进入玩家的货币栏。</para>
        /// </summary>
        /// <param name="item">刚刚被拾取的物品。</param>
        /// <returns>始终为 true，表示不截留该物品。</returns>
        public override bool OnPickup(Item item)
        {
            // 拾取与累计天然发生在玩家本端；其它端重放该流程会把记录污染
            if (Player.whoAmI != Main.myPlayer) return true;

            // 没有「带着钱死去」的有效记录时不统计
            if (!HasLastDeathPosition || LastDeathDroppedCoins <= 0) return true;

            long coinValue = GetCoinCopperValue(item.type);
            if (coinValue <= 0) return true;

            // 范围判定沿用与「击杀拾金敌怪」一致的 1024×1024 方形区域
            if (Math.Abs(Player.Center.X - LastDeathPosition.X) > CoinRangeHalfSize
                || Math.Abs(Player.Center.Y - LastDeathPosition.Y) > CoinRangeHalfSize)
                return true;

            NearbyPickedCoins += coinValue;

            // 严格大于：拾回的钱必须多于丢掉的钱，才谈得上「讨回财宝」
            if (NearbyPickedCoins > LastDeathDroppedCoins)
                TryConsumeCoinRecord();

            return true;
        }

        /// <summary>
        /// 触发「讨回失落财宝」：单机本端直接产出，多人交由服务器权威产出。
        /// </summary>
        private void TryConsumeCoinRecord()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                GrantRecordItem();
                ClearCoinRecord();
                return;
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // 先本地清零，避免同一帧内多枚钱币各触发一次请求
                ClearCoinRecord();
                MonteCristoCoinSync.Request(Player.whoAmI).Send();
            }
        }

        /// <summary>
        /// 产出《基督山伯爵》。必须由权威端（单机或服务器）调用，产出结果由引擎自动同步。
        /// </summary>
        public void GrantRecordItem()
        {
            // 沿用记录系统既有的「已持有则不产出」与冷却机制
            if (!RecorderSystem.ShouldSpawnRecordItem<TheCountOfMonteCristo>()) return;

            Player.QuickSpawnItem(new EntitySource_Misc("MonteCristoCoinRecord"), ModContent.ItemType<TheCountOfMonteCristo>());
            RecorderSystem.SetCooldown<TheCountOfMonteCristo>();
        }

        /// <summary>
        /// 清空「死亡点 + 死亡掉落额 + 附近拾取额」记录，需再次死亡重新记录后才可能再触发。
        /// </summary>
        public void ClearCoinRecord()
        {
            HasLastDeathPosition = false;
            LastDeathPosition = default;
            LastDeathDroppedCoins = 0;
            NearbyPickedCoins = 0;
        }

        // ==================== HopeBuff 相关 ====================
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (Player.HasBuff(ModContent.BuffType<HopeBuff>()))
            {
                Player.ClearBuff(ModContent.BuffType<HopeBuff>());
                modifiers.SetMaxDamage(1);
                _hopeBlockActive = true;
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (Player.HasBuff(ModContent.BuffType<HopeBuff>()))
            {
                Player.ClearBuff(ModContent.BuffType<HopeBuff>());
                modifiers.SetMaxDamage(1);
                _hopeBlockActive = true;
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (_hopeBlockActive)
            {
                Player.immuneTime = 300;
                Player.immune = true;
                _hopeBlockActive = false;
                Player.AddBuff(ModContent.BuffType<RevengeBuff>(), RevengeDurationFrames);
            }
            base.OnHitByNPC(npc, hurtInfo);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (_hopeBlockActive)
            {
                Player.immuneTime = 300;
                Player.immune = true;
                _hopeBlockActive = false;
                Player.AddBuff(ModContent.BuffType<RevengeBuff>(), RevengeDurationFrames);
            }
            base.OnHitByProjectile(proj, hurtInfo);
        }

        public override void ResetEffects()
        {
            _hopeBlockActive = false;
            base.ResetEffects();
        }
    }
}