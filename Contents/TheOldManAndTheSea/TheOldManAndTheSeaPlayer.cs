using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.TheOldManAndTheSea
{
    public class TheOldManAndTheSeaPlayer : ModPlayer
    {
        public int Energy;               // 当前能量
        public const int MaxEnergy = 100;
        public bool IsActivated;         // 是否激活
        private int energyTimer;         // 衰减计时器（帧数）
        private float shakeTimer;        // 用于UI抖动的计时器

        public override void ResetEffects() { }

        public override void UpdateDead()
        {
            IsActivated = false;
            Energy = 0;
        }

        public override void PostUpdate()
        {
            // 检查玩家是否处于钓鱼状态
            bool isFishing = false;
            if (Player.HeldItem != null && Player.HeldItem.fishingPole > 0)
            {
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    Projectile p = Main.projectile[i];
                    if (p.active && p.owner == Player.whoAmI && p.bobber)
                    {
                        isFishing = true;
                        break;
                    }
                }
            }

            // 能量衰减逻辑
            if (Energy > 0)
            {
                bool shouldDecay = false;
                if (IsActivated)
                    shouldDecay = true;
                else if (!isFishing)
                    shouldDecay = true;

                if (shouldDecay)
                {
                    energyTimer++;
                    // 激活时每3秒(180帧) -1，未激活时每5秒(300帧) -1
                    int decayInterval = IsActivated ? 180 : 300;
                    if (energyTimer >= decayInterval)
                    {
                        energyTimer = 0;
                        Energy--;
                        if (Energy < 0) Energy = 0;
                        if (Energy == 0 && IsActivated)
                            IsActivated = false;
                    }
                }
                else
                {
                    energyTimer = 0;
                }
            }
            else
            {
                energyTimer = 0;
                if (IsActivated) IsActivated = false;
            }

            // 更新UI抖动计时器（只在激活时增加）
            if (IsActivated)
                shakeTimer += 0.1f;
            else
                shakeTimer = 0;
        }

        /// <summary> 添加能量 </summary>
        public void AddEnergy(int amount)
        {
            Energy += amount;
            if (Energy > MaxEnergy) Energy = MaxEnergy;
        }

        /// <summary> 切换激活状态 </summary>
        public void ToggleActivation()
        {
            if (Energy > 0)
                IsActivated = !IsActivated;
        }

        /// <summary> 捕获鱼获时填充能量 </summary>
        public override void ModifyCaughtFish(Item item)
        {
            int energyGain = 0;
            int rarity = item.rare;

            switch (rarity)
            {
                case -1: energyGain = 2; break;   // 灰色
                case 0: energyGain = 3; break;   // 白色
                case 2: energyGain = 6; break;   // 绿色
                case 1: energyGain = 8; break;   // 蓝色
                case -11: energyGain = 25; break;  // 琥珀色
                default:
                    if (rarity >= 3) energyGain = 12; // 橙色及以上
                    break;
            }

            if (energyGain > 0)
                AddEnergy(energyGain);

            base.ModifyCaughtFish(item);
        }

        /// <summary> 提供给UI的抖动偏移量 </summary>
        public Vector2 GetUIShakeOffset()
        {
            if (!IsActivated) return Vector2.Zero;
            // 使用正弦和余弦产生平滑抖动，幅度 2 像素
            float x = (float)Math.Sin(shakeTimer * 1.7f) * 2f;
            float y = (float)Math.Cos(shakeTimer * 2.3f) * 2f;
            return new Vector2(x, y);
        }
    }
}