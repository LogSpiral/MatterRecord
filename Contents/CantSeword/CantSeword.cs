using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace MatterRecord.Contents.CantSeword
{
    public class CantSeword : ModItem
    {
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Melee;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = Item.useAnimation = 18;
            Item.width = Item.height = 80;
            Item.damage = 20;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 1, 0);
            Item.shoot = ModContent.ProjectileType<CantSewordProj>();
            Item.shootSpeed = 16;
            Item.noMelee = true;
            Item.channel = true;
            base.SetDefaults();
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] < 1;
        public override void Load()
        {
            On_PopupText.NewText_PopupTextContext_Item_int_bool_bool += CantSewordModify;
            base.Load();
        }

        private int CantSewordModify(On_PopupText.orig_NewText_PopupTextContext_Item_int_bool_bool orig, PopupTextContext context, Item newItem, int stack, bool noStack, bool longText)
        {
            if (newItem?.ModItem is CantSeword)
                return -1;
            return orig.Invoke(context, newItem, stack, noStack, longText);
        }
    }
    public class CantSewordProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 28;
            base.SetStaticDefaults();
        }
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Arkhalis);
            Projectile.aiStyle = -1;
            base.SetDefaults();
        }
        public override void AI()
        {
            var projectile = Projectile;
            Player player = Main.player[projectile.owner];
            float num = (float)Math.PI / 2f;
            Vector2 vector = player.RotatedRelativePoint(player.MountedCenter);
            int num2 = 2;
            float num3 = 0f;
            num = 0f;
            if (projectile.spriteDirection == -1)
                num = (float)Math.PI;

            if (++projectile.frame >= 28)
                projectile.frame = 0;

            projectile.soundDelay--;
            if (projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item1, projectile.Center);
                projectile.soundDelay = 12;
            }

            if (Main.myPlayer == projectile.owner)
            {
                if (player.channel && !player.noItems && !player.CCed)
                {
                    float num42 = 1f;
                    if (player.inventory[player.selectedItem].shoot == projectile.type)
                        num42 = player.inventory[player.selectedItem].shootSpeed * projectile.scale;

                    Vector2 vec = Main.MouseWorld - vector;
                    vec.Normalize();
                    if (vec.HasNaNs())
                        vec = Vector2.UnitX * player.direction;

                    vec *= num42;
                    if (vec.X != projectile.velocity.X || vec.Y != projectile.velocity.Y)
                        projectile.netUpdate = true;

                    projectile.velocity = vec;
                }
                else
                {
                    projectile.Kill();
                }
            }

            Vector2 vector20 = projectile.Center + projectile.velocity * 3f;
            Lighting.AddLight(vector20, 0.8f, 0.8f, 0.8f);
            if (Main.rand.NextBool(3))
            {
                int num43 = Dust.NewDust(vector20 - projectile.Size / 2f, projectile.width, projectile.height, 63, projectile.velocity.X, projectile.velocity.Y, 100, default(Color), 2f);
                Main.dust[num43].noGravity = true;
                Main.dust[num43].position -= projectile.velocity;
            }

            projectile.position = player.RotatedRelativePoint(player.MountedCenter, reverseRotation: false, addGfxOffY: false) - projectile.Size / 2f;
            projectile.rotation = projectile.velocity.ToRotation() + num;
            projectile.spriteDirection = projectile.direction;
            projectile.timeLeft = 2;
            player.ChangeDir(projectile.direction);
            player.heldProj = projectile.whoAmI;
            player.SetDummyItemTime(num2);
            player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(projectile.velocity.Y * (float)projectile.direction, projectile.velocity.X * (float)projectile.direction) + num3);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            //Main.NewText(Projectile.frame);
            //Main.spriteBatch.Draw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 64 * Projectile.frame, 68, 64), lightColor, Projectile.rotation, new Vector2(), 1, 0, 0);
            //return false;
            lightColor *= .25f;
            return base.PreDraw(ref lightColor);
        }
    }
    public class CantSewordGrass : GlobalTile
    {
        private static readonly int[] grassTypes = [3, 24, 61, 110, 201, 529, 637, 73, 74, 113];

        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            bool flag = grassTypes.Contains(Framing.GetTileSafely(i, j).TileType);
            if (!flag || !Main.rand.NextBool(5000)) return;
            int number = Item.NewItem(new EntitySource_ItemUse(Main.LocalPlayer, Main.LocalPlayer.HeldItem), i * 16, j * 16, 16, 16, ModContent.ItemType<CantSeword>());
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
            base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
        }
    }
}
