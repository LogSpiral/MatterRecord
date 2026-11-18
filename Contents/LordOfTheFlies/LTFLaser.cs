using MatterRecord.Contents.EmeraldTablet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
namespace MatterRecord.Contents.LordOfTheFlies;

public class LTFLaser : ModProjectile
{
    public static float ChargeTime => 60;
    public static float DecayTime => 30;
    public float Timer
    {
        get => Projectile.ai[0];
        set => Projectile.ai[0] = value;
    }

    private enum LaserState
    {
        Charging,
        Charged,
        Decay
    }

    private LaserState State
    {
        get => (LaserState)(int)Projectile.ai[1];
        set => Projectile.ai[1] = (float)value;
    }

    public float Distance
    {
        get => Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }


    public Player Owner => Main.player[Projectile.owner];

    public LordOfTheFliesPlayer LTFPlayer => Owner.GetModPlayer<LordOfTheFliesPlayer>();

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 1;
        Projectile.friendly = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 5;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (State != LaserState.Charged) return false;
        float useless = 0f;
        var unit = Projectile.velocity.SafeNormalize(default);
        var nunit = new Vector2(-unit.Y, unit.X) * Owner.direction;
        var position = Projectile.Center + unit * 40 - nunit * 0;
        return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), position, position + unit * Distance, 16, ref useless);
    }

    public override bool ShouldUpdatePosition() => false;

    private void PlayerUpdates()
    {
        Projectile.Center = Owner.Center;
        if (Projectile.owner == Main.myPlayer)
            Projectile.velocity = Main.MouseWorld - Owner.Center;
        Owner.itemAnimation = 2;
        Owner.itemTime = 2;
        Owner.heldProj = Projectile.whoAmI;
        Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.velocity.ToRotation() - MathHelper.PiOver2);
        Owner.direction = Math.Sign(Projectile.velocity.X);
    }

    private static readonly float[] distanceSampler = new float[3];

    public override void AI()
    {
        PlayerUpdates();
        Projectile.timeLeft = 2;
        switch (State)
        {
            case LaserState.Charging:
                {
                    Timer++;
                    SoundEngine.PlaySound(SoundID.Item15 with { MaxInstances = -1, Pitch = Timer / ChargeTime * 2 - 1f }, Projectile.Center);
                    if (Timer > ChargeTime)
                    {
                        SoundEngine.PlaySound(SoundID.Item68, Projectile.Center);
                        State = LaserState.Charged;
                        Timer = 0;

                        var unit = Projectile.velocity.SafeNormalize(default);
                        var nunit = new Vector2(-unit.Y, unit.X) * Owner.direction;
                        var position = Projectile.Center + unit * 40 - nunit * 0 + new Vector2(0, Owner.gfxOffY);
                        for (int n = 0; n < 10; n++)
                            Dust.NewDustPerfect(position, DustID.TerraBlade, Main.rand.NextVector2Unit() * Main.rand.NextFloat(32));

                        ScreenShakeSystem.Instance.ScreenOffset += -unit * 16f;

                    }
                }
                break;
            case LaserState.Charged:
                {
                    Timer++;
                    SoundEngine.PlaySound(SoundID.Item15 with { MaxInstances = -1, Volume = .25f }, Projectile.Center);
                    Owner.GetModPlayer<MatterRecordPlayer>().strengthOfShake += .1f;

                    var unit = Projectile.velocity.SafeNormalize(default);
                    ScreenShakeSystem.Instance.ScreenOffset += -unit;
                    var nunit = new Vector2(-unit.Y, unit.X) * Owner.direction;
                    var position = Projectile.Center + unit * 40 - nunit * 0 + new Vector2(0, Owner.gfxOffY);
                    Collision.LaserScan(position, unit, 1, 2400, distanceSampler);
                    Distance = (distanceSampler[0] + distanceSampler[1] + distanceSampler[2]) * .33f;

                    if (Timer % 5 == 0)
                        LTFPlayer.ChargingEnergy--;
                    if (Timer % 10 == 0)
                        Dust.NewDustPerfect(position, DustID.TerraBlade, Main.rand.NextVector2Unit() * 4 - unit * Main.rand.NextFloat(48));
                    if (Distance != 2400 && Timer % 5 == 0)
                        Dust.NewDustPerfect(position + unit * Distance, DustID.TerraBlade, Main.rand.NextVector2Unit() * Main.rand.NextFloat(8));
                    if (LTFPlayer.ChargingEnergy <= 0 || !Owner.controlUseItem || !Owner.controlUp)
                    {
                        SoundEngine.PlaySound(SoundID.Item94, Projectile.Center);
                        State = LaserState.Decay;
                        Timer = 0;
                    }
                }
                break;
            case LaserState.Decay:
                {
                    Timer++;
                    SoundEngine.PlaySound(
                        SoundID.Item15 with
                        {
                            MaxInstances = -1,
                            Pitch = -Timer / DecayTime,
                            Volume = .5f
                        },
                        Projectile.Center);
                    if (Timer > DecayTime)
                    {
                        Projectile.Kill();
                    }
                }
                break;
        }
        if (Timer % 5 == 0)
            Projectile.netImportant = true;
        base.AI();
    }
    public override bool PreDraw(ref Color lightColor)
    {
        var tex = TextureAssets.Projectile[Type].Value;
        var unit = Projectile.velocity.SafeNormalize(default);
        var nunit = new Vector2(-unit.Y, unit.X) * Owner.direction;
        var position = Projectile.Center - Main.screenPosition + unit * 40 - nunit * 0 + new Vector2(0, Owner.gfxOffY);
        var rotation = unit.ToRotation();

        static void DrawStar(Texture2D tex, Vector2 position, float rotation, Vector2 scale, float alpha = 1f)
        {
            Main.EntitySpriteDraw(
                tex,
                position,
                null,
                Color.White with { A = 0 } * alpha,
                rotation,
                new Vector2(27),
                scale,
                0,
                0);
        }

        static void DrawLaser(Vector2 position, float rotation, Vector2 scale)
        {
            //Main.EntitySpriteDraw(
            //    TextureAssets.MagicPixel.Value,
            //    position,
            //    new Rectangle(0, 0, 1, 1),
            //    Color.White,
            //    rotation,
            //    new Vector2(0, 0.5f),
            //    scale,
            //    0, 0);

            var spriteBatch = Main.spriteBatch;
            var graphicsDevice = Main.graphics.GraphicsDevice;
            using (new SpriteBatchScope(
                   Main.spriteBatch,
                   SpriteSortMode.Immediate,
                   BlendState.Additive,
                   SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null,
                   Main.GameViewMatrix.TransformationMatrix
                   ))
            {
                var effect = ModAsset.LTFLaserEffect.Value;
                effect.Parameters["uTime"]?.SetValue(-4 * Main.GlobalTimeWrappedHourly);
                effect.Parameters["uXFactor"]?.SetValue(scale.X / 512f);
                graphicsDevice.Textures[1] = ModAsset.LTFHeatMap.Value;
                effect.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(TextureAssets.Extra[ExtrasID.MagicMissileTrailShape].Value,
                    position, null, Color.White, rotation,
                    new Vector2(0, 128), scale / new Vector2(256, 12f), 0, 0);

                effect.Parameters["uTime"]?.SetValue(-2 * Main.GlobalTimeWrappedHourly + 0.5f);
                spriteBatch.Draw(TextureAssets.Extra[ExtrasID.FlameLashTrailShape].Value,
                    position, null, Color.White * .25f, rotation,
                    new Vector2(0, 128), scale / new Vector2(256, 8f), 0, 0);
                spriteBatch.Draw(TextureAssets.Extra[ExtrasID.FlameLashTrailShape].Value,
                    position, null, Color.White * .25f, rotation,
                    new Vector2(0, 128), scale / new Vector2(256, 8f), SpriteEffects.FlipVertically, 0);
            }
        }

        switch (State)
        {
            case LaserState.Charging:
                {
                    float factor = Timer / ChargeTime;

                    float gfactor = factor;

                    factor *= 6;
                    float factor2 = (int)factor;
                    factor %= 1;

                    float factor3 = factor * (1 - factor) * 4 * MathF.Exp(-factor);

                    for (int n = 0; n < factor2 / 2 + 1; n++)
                    {
                        var rot = factor2 * 4 + n * n * factor2 * .5f;
                        DrawStar(
                            tex,
                            position
                            + Vector2.Lerp(
                                rot.ToRotationVector2() * 48,
                                Vector2.Zero,
                                factor * factor),
                            rot,
                            new Vector2(
                                0.25f + 1 / (factor * 4 + 1f),
                                0.25f)
                            * factor3,
                             factor3);
                    }

                    if (gfactor > .75f)
                    {
                        gfactor = gfactor * 4 - 3f;
                        DrawStar(
                            tex,
                            position,
                            gfactor * MathF.PI + rotation,
                            new Vector2(
                                1 / (gfactor + .5f),
                                1 + gfactor) * (1 - gfactor) * 2,
                            gfactor);
                    }
                    break;
                }
            case LaserState.Charged:
                {
                    float factor = MathHelper.Clamp(Timer / 10f, 0, 1);

                    Vector2 scale = new Vector2(0.75f, 1.5f) * factor * (MathF.Sin(Timer) * .125f + 1f);

                    DrawStar(tex, position, rotation, scale, factor);

                    DrawLaser(position, rotation, new Vector2(Distance, 2));

                    if (Distance != 2400f)
                        DrawStar(tex, position + unit * Distance, 0, new Vector2(.75f) * (MathF.Cos(Timer) * .2f + 1f));

                    if (Timer < 10)
                    {
                        DrawStar(tex, position, rotation, scale * factor * 4, MathF.Exp(-factor * 2));
                    }
                    break;
                }
            case LaserState.Decay:
                {
                    float factor = Timer / DecayTime;
                    DrawStar(tex, position, rotation + factor * MathF.PI, new Vector2(0.75f, 1.5f) * MathF.Exp(-factor * 8));

                    if (Timer < 10)
                    {
                        float factor2 = Timer / 10f;
                        DrawStar(
                            tex,
                            position,
                            rotation - factor * MathF.Tau,
                            new Vector2(0.75f, 1.5f + factor) * (1 + factor * 2),
                            1 - factor2);

                        DrawStar(tex, position, rotation, new Vector2(0.75f, 1.5f) * factor2 * factor2 * 3, MathF.Exp(-factor2 * 2));
                    }


                    break;
                }
        }
        return false;
    }
}

file readonly struct SpriteBatchScope : IDisposable
{
    private readonly SpriteBatch _spriteBatch;
    private readonly SpriteSortMode _sortMode;
    private readonly BlendState _blendState;
    private readonly SamplerState _samplerState;
    private readonly DepthStencilState _depthStencilState;
    private readonly RasterizerState _rasterizerState;
    private readonly Effect _effect;
    private readonly Matrix _matrix;

    public SpriteBatchScope(
        SpriteBatch spriteBatch,
        SpriteSortMode mode,
        BlendState? blendState,
        SamplerState? samplerState,
        DepthStencilState? depthStencilState,
        RasterizerState? rasterizerState,
        Effect? effect,
        in Matrix matrix)
    {
        _spriteBatch = spriteBatch;
        _sortMode = spriteBatch.sortMode;
        _blendState = spriteBatch.blendState;
        _samplerState = spriteBatch.samplerState;
        _depthStencilState = spriteBatch.depthStencilState;
        _rasterizerState = spriteBatch.rasterizerState;
        _effect = spriteBatch.customEffect;
        _matrix = spriteBatch.transformMatrix;
        spriteBatch.End();
        spriteBatch.Begin(
            mode,
            blendState ?? _blendState,
            samplerState ?? samplerState,
            depthStencilState ?? depthStencilState,
            rasterizerState ?? _rasterizerState,
            effect ?? _effect,
            matrix == default ? _matrix : matrix);
    }

    public SpriteBatchScope() :
    this(Main.spriteBatch,
         SpriteSortMode.Deferred,
         BlendState.AlphaBlend,
         SamplerState.LinearClamp,
         DepthStencilState.None,
         RasterizerState.CullCounterClockwise,
         null,
         Matrix.Identity)
    { }


    public SpriteBatchScope(SpriteBatch spriteBatch) :
        this(spriteBatch,
             SpriteSortMode.Deferred,
             BlendState.AlphaBlend,
             SamplerState.LinearClamp,
             DepthStencilState.None,
             RasterizerState.CullCounterClockwise,
             null,
             Matrix.Identity)
    { }

    public SpriteBatchScope(
        SpriteBatch spriteBatch,
        SpriteSortMode mode,
        BlendState blendState) :
        this(spriteBatch,
             mode,
             blendState,
             SamplerState.LinearClamp,
             DepthStencilState.None,
             RasterizerState.CullCounterClockwise,
             null,
             Matrix.Identity)
    {

    }

    public SpriteBatchScope(
        SpriteBatch spriteBatch,
        SpriteSortMode mode,
        BlendState blendState,
        SamplerState samplerState,
        DepthStencilState depthStencilState,
        RasterizerState rasterizerState) :
        this(spriteBatch,
             mode,
             blendState,
             samplerState,
             depthStencilState,
             rasterizerState,
             null,
             Matrix.Identity)
    {

    }

    public SpriteBatchScope(
        SpriteBatch spriteBatch,
        SpriteSortMode mode,
        BlendState blendState,
        SamplerState samplerState,
        DepthStencilState depthStencilState,
        RasterizerState rasterizerState,
        Effect effect) :
        this(spriteBatch,
             mode,
             blendState,
             samplerState,
             depthStencilState,
             rasterizerState,
             effect,
             Matrix.Identity)
    {

    }

    void IDisposable.Dispose()
    {
        _spriteBatch.End();
        _spriteBatch.Begin(
            _sortMode,
            _blendState,
            _samplerState,
            _depthStencilState,
            _rasterizerState,
            _effect,
            _matrix);
    }
}
