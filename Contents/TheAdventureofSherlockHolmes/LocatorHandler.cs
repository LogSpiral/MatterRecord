using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using ReLogic.Content;

namespace MatterRecord.Contents.TheAdventureofSherlockHolmes
{
    // 本地玩家数据存储
    public class LocatorPlayer : ModPlayer
    {
        public string SavedSearchQuery = "";
        public List<Point> SavedMarkers = new List<Point>();

        public void SaveSearchState()
        {
            SavedSearchQuery = SearchCore.CurrentQuery;
            SavedMarkers.Clear();
            SavedMarkers.AddRange(SearchCore.Results);
        }

        public void ClearSavedState()
        {
            SavedSearchQuery = "";
            SavedMarkers.Clear();
        }

        // 进入世界时重置所有保存的搜索状态（避免残留指引）
        public override void OnEnterWorld()
        {
            ClearSavedState();
            // 同时清除当前搜索标记（但不影响地图UI）
            SearchCore.ClearSearch();
        }
    }

    // 指引投射物（指向固定坐标）
    public class LocatorProjectile : ModProjectile
    {
        private Vector2 targetPos;
        private bool hasTarget;

        public override string Texture => "MatterRecord/Contents/TheAdventureofSherlockHolmes/LocatorProjectile";

        public override void SetStaticDefaults()
        {
            // Display name will be automatically set from localization or class name
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 28;
            Projectile.light = 0f;        // 移除发光
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.timeLeft = 2;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = false;
        }

        public void SetTarget(Vector2 target)
        {
            targetPos = target;
            hasTarget = true;
            Projectile.timeLeft = 2;
        }

        public void UpdateTarget(Vector2 newTarget)
        {
            targetPos = newTarget;
            hasTarget = true;
        }

        public override void AI()
        {
            if (!hasTarget)
            {
                Projectile.Kill();
                return;
            }

            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            Vector2 direction = targetPos - player.Center;
            if (direction.Length() < 0.1f)
            {
                Projectile.Kill();
                return;
            }
            direction.Normalize();
            Projectile.velocity = direction;

            Projectile.position = player.position + direction * 45f;
            Projectile.rotation = direction.ToRotation() + MathHelper.ToRadians(90f);

            Projectile.timeLeft = 2;
        }
    }

    // Mod系统：监听地图开关，管理投射物生成和状态保存
    public class LocatorSystem : ModSystem
    {
        private bool wasMapFullscreen = false;
        private LocatorProjectile currentProjectile = null;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                ModContent.Request<Texture2D>("MatterRecord/Contents/TheAdventureofSherlockHolmes/LocatorProjectile");
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            bool isMapFullscreen = Main.mapFullscreen;
            if (isMapFullscreen != wasMapFullscreen)
            {
                if (!isMapFullscreen) // 地图关闭时保存搜索状态
                {
                    LocatorPlayer player = Main.LocalPlayer.GetModPlayer<LocatorPlayer>();
                    player.SaveSearchState();
                    RemoveProjectile(); // 地图关闭时移除指引投射物
                }
                wasMapFullscreen = isMapFullscreen;
            }

            if (isMapFullscreen)
            {
                RemoveProjectile();
                return;
            }

            // 非地图模式：更新指引投射物
            UpdateLocatorProjectile();
        }

        private void UpdateLocatorProjectile()
        {
            LocatorPlayer player = Main.LocalPlayer.GetModPlayer<LocatorPlayer>();
            var markers = player.SavedMarkers;
            if (markers.Count == 0)
            {
                RemoveProjectile();
                return;
            }

            Vector2 playerCenter = Main.LocalPlayer.Center;
            Point nearest = markers.OrderBy(p => Vector2.DistanceSquared(playerCenter, p.ToWorldCoordinates())).First();
            Vector2 targetPos = nearest.ToWorldCoordinates();

            if (currentProjectile != null && currentProjectile.Projectile.active)
            {
                // 更新目标位置
                currentProjectile.UpdateTarget(targetPos);
            }
            else
            {
                CreateProjectile(targetPos);
            }
        }

        private void CreateProjectile(Vector2 target)
        {
            RemoveProjectile();
            int projIndex = Projectile.NewProjectile(Projectile.GetSource_None(), Main.LocalPlayer.Center, Vector2.Zero, ModContent.ProjectileType<LocatorProjectile>(), 0, 0f, Main.LocalPlayer.whoAmI);
            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
            {
                currentProjectile = Main.projectile[projIndex].ModProjectile as LocatorProjectile;
                currentProjectile?.SetTarget(target);
            }
        }

        private void RemoveProjectile()
        {
            if (currentProjectile != null && currentProjectile.Projectile.active)
            {
                currentProjectile.Projectile.Kill();
            }
            currentProjectile = null;
        }
    }
}