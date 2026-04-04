using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.CompendiumOfMateriaMedica
{
    /// <summary>
    /// 本草纲目饰品的玩家数据类。
    /// 负责存储草药效果状态、管理附近草药扫描、应用属性加成以及控制草药保护。
    /// 支持多人模式同队共享：只要队伍中有一人装备了本草纲目，全队都能获得草药效果（但只有装备者本人获得草药保护）。
    /// </summary>
    public class CompendiumPlayer : ModPlayer
    {
        // ---------- 基础状态 ----------
        /// <summary>是否装备了本草纲目饰品（每帧在 ResetEffects 中重置，由 UpdateAccessory 设置）。</summary>
        public bool hasCompendium = false;

        /// <summary>饰品是否可见（用于决定是否启用草药保护）。</summary>
        public bool showCompendiumVisual = false;

        // ---------- 药水触发的效果标志（永久生效，直到增益消失） ----------
        public bool potionBlinkroot = false;
        public bool potionDaybloom = false;
        public bool potionDeathweed = false;
        public bool potionFireblossom = false;
        public bool potionMoonglow = false;
        public bool potionShiverthorn = false;
        public bool potionWaterleaf = false;

        // ---------- 附近图格触发的效果标志（基于计时器，离开范围后延迟消失） ----------
        public bool proximityBlinkroot = false;
        public bool proximityDaybloom = false;
        public bool proximityDeathweed = false;
        public bool proximityFireblossom = false;
        public bool proximityMoonglow = false;
        public bool proximityShiverthorn = false;
        public bool proximityWaterleaf = false;

        // ---------- 计时器（单位：帧，60帧 = 1秒） ----------
        public int proximityTimerBlinkroot = 0;
        public int proximityTimerDaybloom = 0;
        public int proximityTimerDeathweed = 0;
        public int proximityTimerFireblossom = 0;
        public int proximityTimerMoonglow = 0;
        public int proximityTimerShiverthorn = 0;
        public int proximityTimerWaterleaf = 0;

        /// <summary>附近草药扫描计数器，每60帧扫描一次。</summary>
        private int proximityScanCounter = 0;

        /// <summary>
        /// 每帧重置状态，仅用于检测饰品是否被卸下。
        /// 注意：卸下饰品时不会清除已获得的效果，让它们自然持续。
        /// </summary>
        public override void ResetEffects()
        {
            // 重置装备标志，供下一帧判断
            hasCompendium = false;
            // 注意：不清除 showCompendiumVisual，因为它由 UpdateAccessory 每帧设置
            // 注意：不清除任何药水或附近草药效果，让它们自然衰减
        }

        /// <summary>
        /// 判断当前玩家是否应该获得草药效果（自己装备或同队有人装备）。
        /// </summary>
        public bool ShouldGainHerbEffects()
        {
            return hasCompendium || HasTeammateWithCompendium();
        }

        /// <summary>
        /// 检查当前玩家所在队伍中是否有其他玩家装备了本草纲目。
        /// </summary>
        private bool HasTeammateWithCompendium()
        {
            if (Player.team == 0) return false; // 无队伍，不共享
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (other != null && other.active && other.team == Player.team && other.whoAmI != Player.whoAmI)
                {
                    var otherComp = other.GetModPlayer<CompendiumPlayer>();
                    if (otherComp != null && otherComp.hasCompendium)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 每帧更新，处理所有与草药相关的逻辑。
        /// </summary>
        public override void PostUpdate()
        {
            // 如果芬芳草药增益消失，则清除所有效果标志（药水效果已过期，附近草药效果也已超时）
            if (!Player.HasBuff(ModContent.BuffType<FragrantHerbs>()))
            {
                ClearAllPotionEffects();
                ClearAllProximityEffects();
            }

            // 计算当前玩家是否应该获得草药效果
            bool canGain = ShouldGainHerbEffects();

            // 附近草药扫描和计时器更新（仅当可以获得效果时）
            if (canGain)
            {
                // 每秒扫描一次附近草药
                proximityScanCounter++;
                if (proximityScanCounter >= 60)
                {
                    proximityScanCounter = 0;
                    ScanNearbyHerbs();
                }
                UpdateProximityTimers();

                // 草药保护：仅当自己装备且饰品可见时启用原版环境保护机制
                if (hasCompendium && showCompendiumVisual)
                {
                    Player.dontHurtNature = true;
                }
            }
            else
            {
                // 无效果时，仍然需要更新计时器（让已有附近草药效果自然衰减）
                UpdateProximityTimers();
            }

            // 无论是否可以获得新效果，只要已有的效果标志还在，就应用属性加成
            ApplyStatBonuses();
        }

        /// <summary>
        /// 攻击命中 NPC 时，根据当前生效的草药效果附加 debuff。
        /// </summary>
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool finalDeathweed = potionDeathweed || proximityDeathweed;
            bool finalFireblossom = potionFireblossom || proximityFireblossom;
            bool finalShiverthorn = potionShiverthorn || proximityShiverthorn;

            if (finalDeathweed) target.AddBuff(BuffID.Poisoned, 30);
            if (finalFireblossom) target.AddBuff(BuffID.OnFire, 30);
            if (finalShiverthorn) target.AddBuff(BuffID.Frostburn, 30);
        }

        /// <summary>
        /// 根据药水中含有的草药类型，激活对应的药水效果标志。
        /// </summary>
        /// <param name="herbType">草药物品的类型 ID。</param>
        public void ActivatePotionEffect(int herbType)
        {
            switch (herbType)
            {
                case ItemID.Blinkroot: potionBlinkroot = true; break;
                case ItemID.Daybloom: potionDaybloom = true; break;
                case ItemID.Deathweed: potionDeathweed = true; break;
                case ItemID.Fireblossom: potionFireblossom = true; break;
                case ItemID.Moonglow: potionMoonglow = true; break;
                case ItemID.Shiverthorn: potionShiverthorn = true; break;
                case ItemID.Waterleaf: potionWaterleaf = true; break;
            }
        }

        /// <summary>清除所有药水触发效果标志。</summary>
        private void ClearAllPotionEffects()
        {
            potionBlinkroot = false;
            potionDaybloom = false;
            potionDeathweed = false;
            potionFireblossom = false;
            potionMoonglow = false;
            potionShiverthorn = false;
            potionWaterleaf = false;
        }

        /// <summary>
        /// 扫描玩家周围 1000x1000 像素区域内的草药图格（TileID 82/83/84），
        /// 根据检测到的草药类型刷新对应的计时器（3秒），并添加短时芬芳草药增益。
        /// </summary>
        private void ScanNearbyHerbs()
        {
            Vector2 center = Player.Center;
            int left = (int)(center.X - 500);
            int right = (int)(center.X + 500);
            int top = (int)(center.Y - 500);
            int bottom = (int)(center.Y + 500);

            int minTileX = MathHelper.Clamp(left / 16, 0, Main.maxTilesX - 1);
            int maxTileX = MathHelper.Clamp(right / 16, 0, Main.maxTilesX - 1);
            int minTileY = MathHelper.Clamp(top / 16, 0, Main.maxTilesY - 1);
            int maxTileY = MathHelper.Clamp(bottom / 16, 0, Main.maxTilesY - 1);

            bool foundBlinkroot = false, foundDaybloom = false, foundDeathweed = false,
                 foundFireblossom = false, foundMoonglow = false, foundShiverthorn = false,
                 foundWaterleaf = false;

            for (int x = minTileX; x <= maxTileX; x++)
            {
                for (int y = minTileY; y <= maxTileY; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (tile.HasTile && (tile.TileType == 82 || tile.TileType == 83 || tile.TileType == 84))
                    {
                        int style = tile.TileFrameX / 18;
                        switch (style)
                        {
                            case 0: foundDaybloom = true; break;
                            case 1: foundMoonglow = true; break;
                            case 2: foundBlinkroot = true; break;
                            case 3: foundDeathweed = true; break;
                            case 4: foundWaterleaf = true; break;
                            case 5: foundFireblossom = true; break;
                            case 6: foundShiverthorn = true; break;
                        }
                    }
                }
            }

            int buffType = ModContent.BuffType<FragrantHerbs>();
            if (foundBlinkroot) { proximityTimerBlinkroot = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
            if (foundDaybloom) { proximityTimerDaybloom = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
            if (foundDeathweed) { proximityTimerDeathweed = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
            if (foundFireblossom) { proximityTimerFireblossom = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
            if (foundMoonglow) { proximityTimerMoonglow = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
            if (foundShiverthorn) { proximityTimerShiverthorn = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
            if (foundWaterleaf) { proximityTimerWaterleaf = 180; if (!Player.HasBuff(buffType)) Player.AddBuff(buffType, 180); }
        }

        /// <summary>
        /// 更新所有附近图格效果计时器，并根据计时器是否大于0来设置对应的效果标志。
        /// </summary>
        private void UpdateProximityTimers()
        {
            if (proximityTimerBlinkroot > 0) proximityTimerBlinkroot--;
            if (proximityTimerDaybloom > 0) proximityTimerDaybloom--;
            if (proximityTimerDeathweed > 0) proximityTimerDeathweed--;
            if (proximityTimerFireblossom > 0) proximityTimerFireblossom--;
            if (proximityTimerMoonglow > 0) proximityTimerMoonglow--;
            if (proximityTimerShiverthorn > 0) proximityTimerShiverthorn--;
            if (proximityTimerWaterleaf > 0) proximityTimerWaterleaf--;

            proximityBlinkroot = proximityTimerBlinkroot > 0;
            proximityDaybloom = proximityTimerDaybloom > 0;
            proximityDeathweed = proximityTimerDeathweed > 0;
            proximityFireblossom = proximityTimerFireblossom > 0;
            proximityMoonglow = proximityTimerMoonglow > 0;
            proximityShiverthorn = proximityTimerShiverthorn > 0;
            proximityWaterleaf = proximityTimerWaterleaf > 0;
        }

        /// <summary>清除所有附近图格效果标志及计时器（增益消失时调用）。</summary>
        private void ClearAllProximityEffects()
        {
            proximityBlinkroot = false;
            proximityDaybloom = false;
            proximityDeathweed = false;
            proximityFireblossom = false;
            proximityMoonglow = false;
            proximityShiverthorn = false;
            proximityWaterleaf = false;

            proximityTimerBlinkroot = 0;
            proximityTimerDaybloom = 0;
            proximityTimerDeathweed = 0;
            proximityTimerFireblossom = 0;
            proximityTimerMoonglow = 0;
            proximityTimerShiverthorn = 0;
            proximityTimerWaterleaf = 0;
        }

        /// <summary>
        /// 根据最终生效的草药效果（药水触发 OR 附近图格触发），给玩家添加属性加成。
        /// 各草药效果说明：
        /// - 闪耀根 (Blinkroot) ：+5% 挖掘速度，+5% 移动速度。
        /// - 太阳花 (Daybloom) ：+2 防御，+0.5 生命再生/秒。
        /// - 死亡草 (Deathweed)：+4% 所有伤害。
        /// - 火焰花 (Fireblossom)：+2% 暴击率。
        /// - 月光草 (Moonglow) ：+0.5 魔力再生/秒，+5% 魔法伤害。
        /// - 寒颤棘 (Shiverthorn)：+10% 物块放置速度，+10% 墙壁放置速度。
        /// - 水叶草 (Waterleaf) ：+5 渔力，+0.01 幸运。
        /// </summary>
        private void ApplyStatBonuses()
        {
            bool finalBlinkroot = potionBlinkroot || proximityBlinkroot;
            bool finalDaybloom = potionDaybloom || proximityDaybloom;
            bool finalDeathweed = potionDeathweed || proximityDeathweed;
            bool finalFireblossom = potionFireblossom || proximityFireblossom;
            bool finalMoonglow = potionMoonglow || proximityMoonglow;
            bool finalShiverthorn = potionShiverthorn || proximityShiverthorn;
            bool finalWaterleaf = potionWaterleaf || proximityWaterleaf;

            if (finalBlinkroot)
            {
                Player.pickSpeed -= 0.05f;
                Player.moveSpeed += 0.05f;
                Player.maxRunSpeed += 0.05f;
            }
            if (finalDaybloom)
            {
                Player.statDefense += 2;
                Player.lifeRegen += 1;
            }
            if (finalDeathweed)
            {
                Player.GetDamage(DamageClass.Generic) += 0.04f;
            }
            if (finalFireblossom)
            {
                Player.GetCritChance(DamageClass.Generic) += 2f;
            }
            if (finalMoonglow)
            {
                Player.manaRegen += 1;
                Player.GetDamage(DamageClass.Magic) += 0.05f;
            }
            if (finalShiverthorn)
            {
                Player.tileSpeed -= 0.1f;   // 物块放置速度 +10%
                Player.wallSpeed -= 0.1f;   // 墙壁放置速度 +10%
            }
            if (finalWaterleaf)
            {
                Player.fishingSkill += 5;
                Player.luck += 0.01f;
            }
        }
    }
}