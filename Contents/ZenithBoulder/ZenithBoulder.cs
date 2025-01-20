using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.ZenithBoulder
{
    public class ZenithBoulder : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ZenithBoulderTile>());

            Item.width = Item.height = 32;
            Item.value = Item.sellPrice(0, 0, 5);
            Item.rare = ItemRarityID.Purple;
            Item.createTile = ModContent.TileType<ZenithBoulderTile>();


            base.SetDefaults();
        }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;

            base.SetStaticDefaults();
        }
        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.LunarBar).AddIngredient(ItemID.Boulder).AddIngredient(ItemID.BouncyBoulder).AddIngredient(ItemID.RollingCactus).AddTile(TileID.HeavyWorkBench).DisableDecraft().Register();
            base.AddRecipes();
        }

    }
    public class ZenithBoulderTile : ModTile
    {
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Projectile.NewProjectile(new EntitySource_TileBreak(i, j), (float)(i * 16) + 15.5f, j * 16 + 16, 0f, 0f, ModContent.ProjectileType<ZenithBoulderProjectile>(), 30, 10f, Main.myPlayer);

            base.KillMultiTile(i, j, frameX, frameY);
        }
        public override bool IsTileDangerous(int i, int j, Player player) => true;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = true;
            DustType = 323; // No dust when mined.

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.CoordinateHeights = [16, 18];
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(152, 171, 198), Language.GetText("MapObject.Trap"));

        }
        public override IEnumerable<Item> GetItemDrops(int i, int j) => null;
    }
    public class ZenithBoulderProjectile : ModProjectile
    {
        public override string Texture => base.Texture.Replace("Projectile", "");

        public override void SetDefaults()
        {
            Projectile.width = 31;
            Projectile.height = 31;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.trap = true;
            base.SetDefaults();
        }
        public override void AI()
        {

            Projectile.localAI[0]++;


            if (Projectile.ai[0] != 0f && Projectile.velocity.Y <= 0f && Projectile.velocity.X == 0f)
            {
                float num218 = 0.5f;
                int i2 = (int)((Projectile.position.X - 8f) / 16f);
                int num219 = (int)(Projectile.position.Y / 16f);
                bool flag9 = false;
                bool flag10 = false;
                if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                    flag9 = true;

                i2 = (int)((Projectile.position.X + (float)Projectile.width + 8f) / 16f);
                if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                    flag10 = true;

                if (flag9)
                {
                    Projectile.velocity.X = num218;
                }
                else if (flag10)
                {
                    Projectile.velocity.X = 0f - num218;
                }
                else
                {
                    i2 = (int)((Projectile.position.X - 8f - 16f) / 16f);
                    num219 = (int)(Projectile.position.Y / 16f);
                    flag9 = false;
                    flag10 = false;
                    if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                        flag9 = true;

                    i2 = (int)((Projectile.position.X + (float)Projectile.width + 8f + 16f) / 16f);
                    if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                        flag10 = true;

                    if (flag9)
                    {
                        Projectile.velocity.X = num218;
                    }
                    else if (flag10)
                    {
                        Projectile.velocity.X = 0f - num218;
                    }
                    else
                    {
                        i2 = (int)((Projectile.position.X - 8f - 32f) / 16f);
                        num219 = (int)(Projectile.position.Y / 16f);
                        flag9 = false;
                        flag10 = false;
                        if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                            flag9 = true;

                        i2 = (int)((Projectile.position.X + (float)Projectile.width + 8f + 32f) / 16f);
                        if (WorldGen.SolidTile(i2, num219) || WorldGen.SolidTile(i2, num219 + 1))
                            flag10 = true;

                        if (!flag9 && !flag10)
                        {
                            if ((int)(Projectile.Center.X / 16f) % 2 == 0)
                                flag9 = true;
                            else
                                flag10 = true;
                        }

                        if (flag9)
                            Projectile.velocity.X = num218;
                        else if (flag10)
                            Projectile.velocity.X = 0f - num218;
                    }
                }
            }

            Projectile.rotation += Projectile.velocity.X * 0.06f;
            Projectile.ai[0] = 1f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            if (Projectile.velocity.Y <= 6f)
            {
                if (Projectile.velocity.X > 0f && Projectile.velocity.X < 7f)
                    Projectile.velocity.X += 0.05f;

                if (Projectile.velocity.X < 0f && Projectile.velocity.X > -7f)
                    Projectile.velocity.X -= 0.05f;
            }
            Projectile.velocity.Y += 0.3f;


            for (int n = Projectile.oldPos.Length - 1; n > 0; n--)
            {
                Projectile.oldPos[n] = Projectile.oldPos[n - 1];
                Projectile.oldRot[n] = Projectile.oldRot[n - 1];
            }
            Projectile.oldPos[0] = Projectile.Center;
            Projectile.oldRot[0] = Projectile.rotation;
            base.AI();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            var tex = ModContent.Request<Texture2D>("MatterRecord/Contents/ZenithBoulder/ZenithBoulder_ExtraLight").Value;
            int m = Projectile.oldPos.Length;
            for (int n = m - 1; n >= 0; n--)
            {
                float fac = Utils.GetLerpValue(0, m - 1, n);
                float scaler = MathHelper.Lerp(1, 0.2f, fac);
                Main.EntitySpriteDraw(tex, Projectile.oldPos[n] - Main.screenPosition + Main.rand.NextVector2Unit() * MathHelper.Lerp(0, Main.rand.NextFloat(0, 8), fac), null, Main.hslToRgb((fac - Main.GlobalTimeWrappedHourly) % 1, 1, 0.95f) * scaler * MathHelper.Clamp(Projectile.localAI[0] / m, 0, 1), Projectile.oldRot[n], new Vector2(16), scaler * 1.2f, 0, 0);//

            }
            return base.PreDraw(ref lightColor);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            float y = Projectile.Center.Y;
            int x = (int)Projectile.Center.X / 16;
            while (y > Main.screenPosition.Y && WorldGen.InWorld(x, (int)y / 16) && !Main.tile[x, (int)y / 16].HasTile)
                y -= 16;



            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center with { Y = y }, Vector2.UnitY * 16, ProjectileID.Boulder, 70, 10f, Main.myPlayer);

            base.OnHitNPC(target, hit, damageDone);
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {

            float y = Projectile.Center.Y;
            int x = (int)Projectile.Center.X / 16;
            while (y > Main.screenPosition.Y && WorldGen.InWorld(x, (int)y / 16) && !Main.tile[x, (int)y / 16].HasTile)
                y -= 16;

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center with { Y = y }, Vector2.UnitY * 16, ProjectileID.Boulder, 70, 10f, Main.myPlayer);

            base.OnHitPlayer(target, info);
        }
        public override bool OnTileCollide(Vector2 lastVelocity)
        {

            float num36 = Math.Abs(lastVelocity.X);
            float num37 = Math.Abs(lastVelocity.Y);
            float num38 = 0.95f;
            float num39 = 0.95f;
            if (num36 < 0.5f)
                num38 = 0.1f;
            else if (num36 < 0.75f)
                num38 = 0.25f;
            else if (num36 < 1f)
                num38 = 0.5f;

            if (num37 < 0.5f)
                num39 = 0.1f;
            else if (num37 < 0.75f)
                num39 = 0.25f;
            else if (num37 < 1f)
                num39 = 0.5f;

            bool flag12 = false;
            if (Projectile.velocity.Y != lastVelocity.Y)
            {
                if (Math.Abs(lastVelocity.Y) > 5f)
                    flag12 = true;

                Projectile.velocity.Y = (0f - lastVelocity.Y) * num39;
            }

            if (Projectile.velocity.X != lastVelocity.X)
            {
                if (Math.Abs(lastVelocity.X) > 5f)
                    flag12 = true;

                Projectile.velocity.X = (0f - lastVelocity.X) * num38;
            }

            if (flag12)
            {
                Projectile.localAI[1] += 1f;
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.RollingCactus, 35, 10f, Main.myPlayer);
            }

            if (Projectile.velocity.Length() < 0.1f && Projectile.localAI[0] > 50f)
                Projectile.Kill();

            if (Projectile.localAI[1] > 20f)
                Projectile.Kill();
            return false;
        }
        public override void OnKill(int timeLeft)
        {

            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int num564 = 0; num564 < 30; num564++)
            {
                int num565 = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 323);
                if (Main.rand.NextBool(2))
                {
                    Dust dust2 = Main.dust[num565];
                    dust2.scale *= 1.4f;
                }

                Projectile.velocity *= 1.9f;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Item.NewItem(Projectile.GetSource_FromThis(), Projectile.Hitbox, new Item(ModContent.ItemType<ZenithBoulder>()));
            base.OnKill(timeLeft);
        }
    }


    public class ZenithBoulderSystem : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int ChestIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Final Cleanup"));

            if (ChestIndex != -1)
            {
                tasks.Insert(ChestIndex, new ZenithBoulderPass("ZenithBoulderSpawn", 100f));

            }
            base.ModifyWorldGenTasks(tasks, ref totalWeight);
        }
    }
    public class ZenithBoulderPass : GenPass
    {
        public ZenithBoulderPass(string str, float value) : base(str, value)
        {

        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "西西弗斯";
            var type = ModContent.TileType<ZenithBoulderTile>();

            List<(int, int)> selectedTiles = [];

            for (int x = 0; x < Main.maxTilesX; x++)
                for (int y = 0; y < Main.maxTilesY; y++)
                    if (Main.tile[x, y].TileType == 138 && !selectedTiles.Contains((x,y)))
                    {

                        selectedTiles.Add((x, y));
                        selectedTiles.Add((x+1, y));
                        selectedTiles.Add((x, y+1));
                        selectedTiles.Add((x+1, y+1));

                        if (WorldGen.genRand.NextBool(WorldGen.noTrapsWorldGen ? 20 : 50)) 
                        {
                            WorldGen.KillTile(x, y);
                            WorldGen.KillTile(x + 1, y);
                            WorldGen.KillTile(x, y + 1);
                            WorldGen.KillTile(x + 1, y + 1);

                            WorldGen.PlaceTile(x, y, type, true, true);
                            WorldGen.PlaceTile(x + 1, y, type, true, true);
                            WorldGen.PlaceTile(x, y + 1, type, true, true);
                            WorldGen.PlaceTile(x + 1, y + 1, type, true, true);
                        }


                    }
            //WorldGen.PlaceTile(x, y, ModContent.TileType<ZenithBoulderTile>(), mute: true,forced:true);
        }
    }
}
