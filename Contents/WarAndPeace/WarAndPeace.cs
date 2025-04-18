using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace MatterRecord.Contents.WarAndPeace;

public class WarAndPeace : ModItem
{
    public override void SetDefaults()
    {
        Item.accessory = true;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(0, 1);
        Item.width = Item.height = 32;
        base.SetDefaults();
    }
    public static bool IsPeace => ((int)(8 * Math.Sin(8 * DateTime.Now.Day)) + 8) % 2 == 1;
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        var dayOfWeek = DateTime.Now.DayOfWeek;
        if (dayOfWeek == DayOfWeek.Sunday)
            player.AddBuff(IsPeace ? ModContent.BuffType<Holiday_Peace>() : ModContent.BuffType<Holiday_War>(), 2);
        else if ((int)dayOfWeek % 2 == 0)
        {
            player.endurance += .1f;
            player.AddBuff(ModContent.BuffType<Peace>(), 2);
        }
        else
        {
            player.AddBuff(ModContent.BuffType<War>(), 2);
            player.GetDamage(DamageClass.Generic) += .1f;
            player.GetDamage(DamageClass.Generic).Flat += 5f;
        }
        base.UpdateAccessory(player, hideVisual);
    }
}
public class WarAndPeaceGlobalItem : GlobalItem
{
    public override void OnConsumeItem(Item item, Player player)
    {
        if (item.type == ItemID.LicenseCat)
            player.QuickSpawnItem(item.GetSource_Misc("CatLicense"), ModContent.ItemType<WarAndPeace>());

        base.OnConsumeItem(item, player);
    }
}
public class WarAndPeacePlayer : ModPlayer
{
    public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
    {
        if (Player.HasBuff<Peace>())
            modifiers.FinalDamage.Flat -= 5;

    }
}
public class War : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    { // This method gets called every frame your buff is active on your player.
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeeded(ref unused, ModContent.ProjectileType<WarCat>(), buffIndex);

    }
}
public class Peace : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    { // This method gets called every frame your buff is active on your player.
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeeded(ref unused, ModContent.ProjectileType<PeaceCat>(), buffIndex);
    }
}
public class Holiday_War : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    { // This method gets called every frame your buff is active on your player.
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeeded(ref unused, ModContent.ProjectileType<WarCat>(), buffIndex);
    }
}
public class Holiday_Peace : ModBuff
{
    public override void SetStaticDefaults()
    {
        Main.buffNoTimeDisplay[Type] = true;
        Main.vanityPet[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    { // This method gets called every frame your buff is active on your player.
        bool unused = false;
        player.BuffHandle_SpawnPetIfNeeded(ref unused, ModContent.ProjectileType<PeaceCat>(), buffIndex);
    }
}
public abstract class CatProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 4;
        Main.projPet[Projectile.type] = true;

        // This code is needed to customize the vanity pet display in the player select screen. Quick explanation:
        // * It uses fluent API syntax, just like Recipe
        // * You start with ProjectileID.Sets.SimpleLoop, specifying the start and end frames as well as the speed, and optionally if it should animate from the end after reaching the end, effectively "bouncing"
        // * To stop the animation if the player is not highlighted/is standing, as done by most grounded pets, add a .WhenNotSelected(0, 0) (you can customize it just like SimpleLoop)
        // * To set offset and direction, use .WithOffset(x, y) and .WithSpriteDirection(-1)
        // * To further customize the behavior and animation of the pet (as its AI does not run), you have access to a few vanilla presets in DelegateMethods.CharacterPreview to use via .WithCode(). You can also make your own, showcased in MinionBossPetProjectile
        ProjectileID.Sets.CharacterPreviewAnimations[Projectile.type] = ProjectileID.Sets.SimpleLoop(0, Main.projFrames[Projectile.type], 6)
            .WithOffset(-10, -20f)
            .WithSpriteDirection(-1)
            .WithCode(DelegateMethods.CharacterPreview.Float);
        base.SetStaticDefaults();
    }
    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.BlackCat); // Copy the stats of the Zephyr Fish

        //AIType = ProjectileID.BlackCat; // Mimic as the Zephyr Fish during AI.

        Projectile.width = 20;
        Projectile.height = 20;
        Projectile.aiStyle = -1;
        base.SetDefaults();
    }
    public override bool PreAI()
    {
        Player player = Main.player[Projectile.owner];

        player.blackCat = false; // Relic from AIType

        return true;
    }
    public void ResetFrameData()
    {
        FrameCounter = 0;
        Frame = 0;
        helpCounter = 0;
    }
    public override void AI()
    {
        #region 玩家检测
        var player = Main.player[Projectile.owner];
        if (!player.active)
        {
            Projectile.active = false;
            return;
        }
        #endregion
        #region 基本量与初始化
        bool goRight = false;
        bool goLeft = false;
        bool jump = false;
        int range = 12;
        #endregion
        #region 位置检测
        float offset = -32 * player.direction;
        float targetX = player.Center.X + offset;
        var cenX = Projectile.Center.X;
        if (targetX < cenX - range)
            goRight = true;
        else if (targetX > cenX + range)
            goLeft = true;
        #endregion
        #region 起飞
        if (player.rocketDelay2 > 0 && state != KoishiState.Recover)
            Projectile.ai[0] = 1f;
        if (Projectile.ai[0] != 0f && state != KoishiState.Recover)
        {
            #region 声明&初始化变量
            float modifyStep = 0.2f;
            int length = 200;
            Projectile.tileCollide = false;
            Vector2 target = player.Center - Projectile.Center;
            float distance = target.Length();
            #endregion
            if (distance > 1440)
            {
                Projectile.Center = player.Center;
            }
            #region 检测并切换状态
            if (distance < length &&//小于最大距离
                player.velocity.Y == 0f &&//玩家站在地面上 
                Projectile.Center.Y <= player.Center.Y && //在玩家中心以上
                !Collision.SolidCollision(Projectile.position, Projectile.width, Projectile.height)//我不到啊，物块碰撞检测？
                )
            {
                //切换至走路状态！
                state = KoishiState.Recover;
                Projectile.ai[0] = 0f;
                Projectile.tileCollide = true;
                ResetFrameData();
                Projectile.localAI[0] = player.Center.Y + (player.mount.Active ? player.mount.HeightBoost - 8 : 8);
                Projectile.localAI[1] = Projectile.Center.Y;
                int k = 8;
                while (k > -1)
                {
                    helpCounter = (int)player.Center.X + k * 16 * player.direction;
                    int i = helpCounter / 16;
                    int n = 0;
                    int j = (int)Projectile.localAI[0] - 128;
                    j /= 16;
                    while (n < 12)
                    {
                        var tile = Main.tile[i, j];
                        if (tile.HasUnactuatedTile && Main.tileSolid[tile.TileType])
                        {
                            //Main.NewText(tile);
                            break;
                        }
                        j++;
                        n++;
                    }
                    if (n != 12 | k == 0)
                    {
                        Projectile.localAI[0] = j * 16 - 12;
                        break;
                    }
                    k--;
                }

                if (Projectile.velocity.Y < -6f)
                    Projectile.velocity.Y = -6f;
                return;
            }
            #endregion
            #region 典中典之速度渐进
            if (distance < 60f)
            {
                target = Projectile.velocity;
            }
            else
            {
                target = target.SafeNormalize(default) * 10f;
            }
            if (Projectile.wet)
            {
                target *= new Vector2(.25f, .1f);
                state = KoishiState.Swim;
            }
            else
            {
                state = KoishiState.Launch;
            }

            if (Projectile.velocity.X < target.X)
            {
                Projectile.velocity.X += modifyStep;
                if (Projectile.velocity.X < 0f)
                    Projectile.velocity.X += modifyStep * 1.5f;
            }
            if (Projectile.velocity.X > target.X)
            {
                Projectile.velocity.X -= modifyStep;
                if (Projectile.velocity.X > 0f)
                    Projectile.velocity.X -= modifyStep * 1.5f;
            }
            if (Projectile.velocity.Y < target.Y)
            {
                Projectile.velocity.Y += modifyStep;
                if (Projectile.velocity.Y < 0f)
                    Projectile.velocity.Y += modifyStep * 1.5f;
            }
            if (Projectile.velocity.Y > target.Y)
            {
                Projectile.velocity.Y -= modifyStep;
                if (Projectile.velocity.Y > 0f)
                    Projectile.velocity.Y -= modifyStep * 1.5f;
            }
            #endregion
            #region 动画处理
            FrameCounter++;
            if (FrameCounter % 4 == 0)
                Frame++;
            Frame %= MaxFrame;
            if (MathF.Abs(Projectile.velocity.X) > .5f)
                Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
            Projectile.rotation = MathHelper.PiOver2 * Projectile.direction * (1 - 1 / (Math.Abs(Projectile.velocity.X) / 24f + 1));
            #endregion
        }
        #endregion
        #region 平地
        else //走路ai
        {
            if (state == KoishiState.Recover)
            {
                //落地
                if (FrameCounter <= 16f)
                {
                    float t = FrameCounter / 16f;
                    //模式切换前记录玩家高度0和弹幕高度1
                    if (Projectile.localAI[1] < Projectile.localAI[0]) //高了
                    {
                        Projectile.Center = Vector2.Lerp(new Vector2(Projectile.Center.X, Projectile.localAI[1]), new Vector2(helpCounter, Projectile.localAI[0]), t * t);
                    }
                    else
                    {
                        Projectile.Center = Vector2.Lerp(new Vector2(Projectile.Center.X, Projectile.localAI[1]), new Vector2(helpCounter, Projectile.localAI[0]), -2 * t * (t - 3 / 2f));
                    }
                    Projectile.velocity.X *= .975f;
                    Projectile.velocity.Y = 0;

                }
                else
                {
                    Projectile.velocity.X *= .925f;
                }
                Projectile.velocity.Y += 0.4f;
                if (Projectile.velocity.Y > 10f)
                    Projectile.velocity.Y = 10f;
                FrameCounter++;
                if (FrameCounter % 4 == 0)
                    Frame++;

                Projectile.rotation *= .25f;
                if (Frame == MaxFrame)
                {
                    Projectile.rotation = 0;
                    state = KoishiState.Idle;
                    ResetFrameData();
                }
            }
            else
            {
                var dis = Vector2.Distance(Projectile.Center, player.Center);
                if (dis > 1440)
                {
                    state = KoishiState.Recover;
                    ResetFrameData();
                    Projectile.localAI[0] = player.Center.Y + (player.mount.Active ? player.mount.HeightBoost - 8 : 8);
                    Projectile.localAI[1] = Projectile.Center.Y - 64;
                }
                else if (dis > 256) Projectile.ai[0] = 1;
                #region 走路/跑步/转向
                float acc = 0.2f;
                float speedMax = 6f;
                if (speedMax < Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y))
                {
                    speedMax = Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y);
                    acc = 0.3f;
                }
                if (goRight)
                {
                    if (Projectile.velocity.X > -3.5)
                        Projectile.velocity.X -= acc;
                    else
                        Projectile.velocity.X -= acc * 0.25f;
                }
                else if (goLeft)
                {
                    if (Projectile.velocity.X < 3.5)
                        Projectile.velocity.X += acc;
                    else
                        Projectile.velocity.X += acc * 0.25f;
                }
                else
                {
                    Projectile.velocity.X *= 0.9f;
                    if (Projectile.velocity.X >= 0f - acc && Projectile.velocity.X <= acc)
                        Projectile.velocity.X = 0f;
                }
                #endregion
                #region 跳跃判定
                if (goRight || goLeft)
                {
                    Point point = (Projectile.Center / 16).ToPoint();
                    if (goRight)
                        point -= new Point(1, 0);

                    if (goLeft)
                        point += new Point(1, 0);

                    point += new Point((int)Projectile.velocity.X, 0);
                    if (WorldGen.SolidTile(point))
                        jump = true;
                }
                #endregion
                Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
                if (Projectile.velocity.Y == 0f) //站在地面上
                {
                    if (jump && state != KoishiState.Flip)
                    {
                        //TODO 加入jump状态
                        int num164 = (int)(Projectile.position.X + Projectile.width / 2) / 16;
                        int num165 = (int)(Projectile.position.Y + Projectile.height) / 16;
                        if (WorldGen.SolidTileAllowBottomSlope(num164, num165) || Main.tile[num164, num165].IsHalfBlock || Main.tile[num164, num165].Slope > 0)
                        {
                            try
                            {
                                num164 = (int)(Projectile.position.X + Projectile.width / 2) / 16;
                                num165 = (int)(Projectile.position.Y + Projectile.height / 2) / 16;
                                if (goRight)
                                    num164--;

                                if (goLeft)
                                    num164++;

                                num164 += (int)Projectile.velocity.X;
                                if (!WorldGen.SolidTile(num164, num165 - 1) && !WorldGen.SolidTile(num164, num165 - 2))
                                    Projectile.velocity.Y = -5.1f;
                                else if (!WorldGen.SolidTile(num164, num165 - 2))
                                    Projectile.velocity.Y = -7.1f;
                                else if (WorldGen.SolidTile(num164, num165 - 5))
                                    Projectile.velocity.Y = -11.1f;
                                else if (WorldGen.SolidTile(num164, num165 - 4))
                                    Projectile.velocity.Y = -10.1f;
                                else
                                    Projectile.velocity.Y = -9.1f;
                            }
                            catch
                            {
                                Projectile.velocity.Y = -9.1f;
                            }
                        }
                        state = KoishiState.Jump;
                        ResetFrameData();
                    }
                }
                #region 速度与朝向处理
                if (Projectile.velocity.X > speedMax)
                    Projectile.velocity.X = speedMax;

                if (Projectile.velocity.X < -speedMax)
                    Projectile.velocity.X = -speedMax;
                #endregion
                #region 动画处理
                //Projectile.spriteDirection = Projectile.direction;
                #region 翻转
                var dVx = Projectile.velocity.X - Projectile.oldVelocity.X;//计算速度变化量
                var tarX = player.Center.X - Projectile.Center.X;
                //Main.NewText((tarX * Projectile.velocity.X < 0, dVx * tarX > 0, state != KoishiState.Flip));
                if (tarX * Projectile.velocity.X < 0 && dVx * tarX > 0 && state != KoishiState.Flip) //
                {
                    state = KoishiState.Flip;
                    ResetFrameData();
                    helpCounter = (int)(Projectile.velocity.X * 128);
                }
                if (state == KoishiState.Flip)
                {
                    //if (dVx * helpCounter > 0) //转身被打断
                    //{
                    //    Main.NewText(dVx);
                    //    state = KoishiState.Walk;
                    //    ResetFrameData();
                    //}
                    var t = Projectile.velocity.X * 128 / helpCounter;//应该是 逐渐从1降低至0的
                    Projectile.spriteDirection = -Math.Sign(helpCounter);
                    if (t <= 0 || t > 1) //已经减小至0，退出转身状态
                    {
                        //Projectile.spriteDirection = Math.Sign(helpCounter);
                        state = KoishiState.Walk;
                        ResetFrameData();

                    }
                    else if (t > 1) //转回去    ////应该不会发生
                    {
                        //state = KoishiState.Walk;
                        //ResetFrameData();
                        t = 1;
                    }
                    //if (helpCounter != 0)
                    Frame = (int)((1 - t) * 9);
                    if (Frame > MaxFrame) { Frame = MaxFrame - 1; }

                }
                #endregion
                #region 跳跃
                else if (state == KoishiState.Jump)
                {
                     Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
                    if (Projectile.velocity.Y != 0)
                    {
                        if (helpCounter < 15)
                            helpCounter++;
                        Frame = helpCounter / 4;
                    }
                    else
                    {
                        //if (helpCounter > 0)
                        //{
                        //    helpCounter--;
                        //}
                        //Frame = 3 - helpCounter / 3;
                        //if (helpCounter == 0) 
                        //{
                        state = KoishiState.Walk;
                        ResetFrameData();
                        //}
                    }
                }
                #endregion
                #region 站立
                else if (Projectile.oldPosition == Projectile.position)
                {
                    Frame = 0;
                    //if ((int)state > 3) //进入直立状态，初始化
                    //{
                    //    state = KoishiState.Idle;
                    //    ResetFrameData();
                    //}
                    //FrameCounter++;
                    //if (FrameCounter % 6 == 0) Frame++;
                    //if (Frame == MaxFrame)
                    //{
                    //    Frame = 0;
                    //    helpCounter++;
                    //    if (helpCounter > 3 && Main.rand.NextBool(3))
                    //    {
                    //        state = (KoishiState)Main.rand.Next(1, 4);
                    //    }
                    //    else
                    //    {
                    //        state = KoishiState.Idle;
                    //    }
                    //}
                }
                #endregion
                #region 移动
                else
                {
                    if (Projectile.velocity.Y != 0) { state = KoishiState.Jump; ResetFrameData(); }
                    int step = 1 + (int)Math.Abs(Projectile.velocity.X * .2f);
                    if (step > 2) step = 2;
                    FrameCounter += step;
                    while (FrameCounter > 3)
                    {
                        FrameCounter -= 4;
                        Frame++;
                    }
                    if (Frame >= MaxFrame) Frame = 0;
                    if (Math.Abs(Projectile.velocity.X) < 4) state = KoishiState.Walk;
                    else state = KoishiState.Run;
                    Projectile.spriteDirection = Math.Sign(Projectile.velocity.X);
                    //if (state == KoishiState.Run || Main.rand.NextBool(3))
                    //{
                    //    var dust = Dust.NewDustPerfect(Projectile.Center + new Vector2(Projectile.velocity.X, 12), DustID.Clentaminator_Blue, default, 0, default, Main.rand.NextFloat(.5f, 1.5f));
                    //    dust.velocity *= .25f;
                    //    dust.shader = GameShaders.Armor.GetShaderFromItemId(player.miscDyes[0].type);
                    //}

                    //}
                }
                #endregion


                #endregion
                #region 迫真重力
                Projectile.velocity.Y += 0.4f;
                if (Projectile.velocity.Y > 10f)
                    Projectile.velocity.Y = 10f;
                #endregion
            }
        }
        #endregion

        if (Frame >= MaxFrame) Frame = MaxFrame - 1;
        if (Frame < 0) Frame = 0;
        Projectile.oldPosition = Projectile.position;
        if (MathF.Abs(Projectile.velocity.X) < .5f)
        {
            Projectile.spriteDirection = player.direction;
        }

    }
    public enum KoishiState
    {
        Idle,
        IdleA,
        IdleB,
        IdleC,
        Walk,
        Run,
        Flip,//
        Jump,
        Launch,
        Recover,
        Swim,
        Sleep
    }
    public KoishiState state;
    public int helpCounter;
    public int MaxFrame => 4;
    public int FrameCounter
    {
        get => Projectile.frameCounter;
        set => Projectile.frameCounter = value;
    }
    public int Frame
    {
        get => Projectile.frame;
        set => Projectile.frame = value;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition - Vector2.UnitY * 4,
            new Rectangle(0, 28 * Projectile.frame, 36, 28), lightColor, Projectile.rotation, new Vector2(18, 14), 1, Projectile.spriteDirection == -1 ? Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally : 0, 0);
        return false;
    }
}
public class WarCat : CatProj
{

    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.


        base.AI();
        if (!player.dead && (player.HasBuff(ModContent.BuffType<War>()) || player.HasBuff(ModContent.BuffType<Holiday_War>())))
            Projectile.timeLeft = 2;
    }
}
public class PeaceCat : CatProj
{
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];

        // Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
        if (!player.dead && (player.HasBuff(ModContent.BuffType<Peace>()) || player.HasBuff(ModContent.BuffType<Holiday_Peace>())))
            Projectile.timeLeft = 2;

        base.AI();
    }
}