using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

        public override void OnEnterWorld()
        {
            ClearSavedState();
            SearchCore.ClearSearch();
        }
    }

    // 指引投射物
    public class LocatorProjectile : ModProjectile
    {
        private Vector2 targetPos;
        private bool hasTarget;

        public override string Texture => "MatterRecord/Contents/TheAdventureofSherlockHolmes/LocatorProjectile";

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 28;
            Projectile.light = 0.8f;  // 发光效果
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

            // 发光呼吸效果
            Projectile.light = 0.6f + 0.3f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f);
        }
    }

    // Mod系统：管理投射物生成和状态同步
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

            // 实时同步标记到玩家数据（供弹幕使用）
            LocatorPlayer player = Main.LocalPlayer.GetModPlayer<LocatorPlayer>();
            player.SavedMarkers.Clear();
            player.SavedMarkers.AddRange(SearchCore.Results); // 已过滤无效标记

            if (isMapFullscreen)
            {
                RemoveProjectile(); // 地图全屏时移除弹幕
                wasMapFullscreen = true;
                return;
            }

            // 非地图模式：更新弹幕
            UpdateLocatorProjectile();
            wasMapFullscreen = false;
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