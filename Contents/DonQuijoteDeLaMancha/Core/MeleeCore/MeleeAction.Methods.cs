using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core;


public partial class MeleeAction // : ModType
{
    public void Update()
    {
        // 开始单次循环
        if (Timer <= 0 && (Counter < CounterMax || CounterMax == 0))
        {
            if (Counter == 0)
                OnActive();
            OnStartSingle();
            var result = (int)(StandardInfo.standardTimer * ModifyData.TimeScaler / CounterMax);
            TimerMax = Timer = result;
            Counter++;
            if (Attacktive)
                OnStartAttack();
        }

        bool oldValue = Attacktive;

        UpdateStatus(Owner is Player plr && (plr.controlUseItem || plr.controlUseTile));
        if (Attacktive) OnAttack();
        else OnCharge();

        if (!oldValue && Attacktive)
        {
            OnStartAttack();
        }
        if (oldValue && !Attacktive)
        {
            OnEndAttack();
        }

        // 结束单次循环
        if (Timer <= 0)
        {
            OnEndSingle();
            if (Attacktive)
                OnEndAttack();

            if (IsCompleted)
                OnDeactive();
        }
    }

    public virtual void UpdateStatus(bool triggered)
    {
        Timer--;
        switch (Owner)
        {
            case Player player:
                {
                    player.itemTime = 2;
                    player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, CompositeArmRotation);
                    break;
                }
        }
    }

    public void Initialize()
    {
        Counter = 0;
        Timer = 0;
    }

    public virtual void OnActive()
    {
        Projectile.ownerHitCheck = OwnerHitCheek;
    }

    public virtual void OnDeactive()
    {
    }

    public virtual void OnStartSingle()
    {
        if (Projectile.owner != Main.myPlayer) return;
        switch (Owner)
        {
            case Player player:
                {
                    //SoundEngine.PlaySound(SoundID.Item71);
                    var tarpos = Main.MouseWorld;
                    player.direction = Math.Sign(tarpos.X - player.Center.X);
                    Rotation = (tarpos - Owner.Center).ToRotation();//TODO 给其它实体用的时候也有传入方向的手段
                    break;
                }
        }
        Projectile.netUpdate = true;
    }

    public virtual void OnEndSingle()
    {
    }

    public virtual void OnCharge()
    {
    }

    public virtual void OnStartAttack()
    {
    }

    public virtual void OnAttack()
    {
    }

    public virtual void OnEndAttack()
    {
    }

    public virtual bool Collide(Rectangle rectangle)
    {
        if (!Attacktive) return false;
        /*float point1 = 0f;
        return Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), Projectile.Center,
                targetedVector + Projectile.Center, 48f, ref point1);*/
        float t = fTimer;
        float sc = 1;
        if (Owner is Player plr)
            sc = plr.GetAdjustedItemScale(plr.HeldItem);
        for (int n = 0; n < 5; n++)
        {
            fTimer = t + n * .2f;
            Vector2 finalOrigin = OffsetOrigin + StandardInfo.standardOrigin;
            float finalRotation = OffsetRotation + StandardInfo.standardRotation;
            Vector2 drawCen = OffsetCenter + Owner.Center;

            float k = 1f;
            if (StandardInfo.VertexStandard.scaler > 0)
            {
                k = StandardInfo.VertexStandard.scaler / TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value.Size().Length();
            }
            CustomVertexInfo[] c = DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, OffsetRotation, Rotation, TextureAssets.Item[Main.LocalPlayer.HeldItem.type].Value, KValue, OffsetSize * ModifyData.Size * sc * k, drawCen, !Flip);

            float point = 0f;
            //Vector2 tar = c[4].Position - drawCen;
            if (Collision.CheckAABBvLineCollision(rectangle.TopLeft(), rectangle.Size(), c[0].Position,
                c[4].Position, 48f, ref point))
            {
                fTimer = t;
                return true;
            }
        }
        fTimer = t;

        return false;
    }

    public virtual void OnHitEntity(Entity victim, int damageDone, object[] context)
    {
        Projectile.localNPCHitCooldown = Math.Clamp(TimerMax / 2, 1, 514);

        //if (OnHitTargetDelegate != null && OnHitTargetDelegate.Key != SequenceSystem.NoneDelegateKey)
        //{
        //    SequenceSystem.elementDelegates[OnHitTargetDelegate.Key].Invoke(this);
        //}

        if (Owner is Player player)
            damageDone /= MathHelper.Clamp(player.GetWeaponDamage(player.HeldItem), 1, int.MaxValue);
        float delta = Main.rand.NextFloat(0.85f, 1.15f) * MathF.Log(damageDone + 1);

        if (Main.LocalPlayer.GetModPlayer<MatterRecordPlayer>().strengthOfShake < 4f)
            Main.LocalPlayer.GetModPlayer<MatterRecordPlayer>().strengthOfShake += delta;

        for (int n = 0; n < 30 * delta * (StandardInfo.dustAmount + .2f); n++)
            MiscMethods.FastDust(victim.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 16f), Main.rand.NextVector2Unit() * Main.rand.NextFloat(Main.rand.NextFloat(0, 8), 16), StandardInfo.standardColor);
    }

    /// <summary>
    /// 辅助用的量，指向末端
    /// </summary>
    public Vector2 targetedVector;

    public virtual CustomVertexInfo[] GetWeaponVertex(Texture2D texture, float alpha)
    {
        Vector2 finalOrigin = OffsetOrigin + StandardInfo.standardOrigin;
        //float finalRotation = offsetRotation + standardInfo.standardRotation;
        Vector2 drawCen = OffsetCenter + Owner.Center;
        float sc = 1;
        if (Owner is Player plr)
        {
            sc = plr.GetAdjustedItemScale(plr.HeldItem);
            drawCen += plr.gfxOffY * Vector2.UnitY;
        }
        return DrawingMethods.GetItemVertexes(finalOrigin, StandardInfo.standardRotation, OffsetRotation, Rotation, texture, KValue, OffsetSize * ModifyData.Size * sc, drawCen, Flip, alpha, StandardInfo.frame);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        #region 好久前的绘制代码，直接搬过来用用试试

        if (Owner == null)
        {
            return;
        }
        //List<CustomVertexInfo> vertexInfos = new List<CustomVertexInfo>();
        //float origFTimer = fTimer;
        //for (int n = 0; n < 10; n++)
        //{
        //    fTimer += .1f;
        //    vertexInfos.AddRange(CustomVertexInfos(texture, 1 - n / 10f));
        //}
        //fTimer = origFTimer;
        //CustomVertexInfo[] c = vertexInfos.ToArray();
        CustomVertexInfo[] c = GetWeaponVertex(texture, 1f);
        Effect ItemEffect = ModAsset.ItemGlowEffectEX.Value;
        if (ItemEffect == null) return;
        SamplerState sampler = SamplerState.AnisotropicWrap;
        var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, 0, 1);
        var model = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0));
        var trans = Main.GameViewMatrix != null ? Main.GameViewMatrix.TransformationMatrix : Matrix.Identity;
        RasterizerState originalState = Main.graphics.GraphicsDevice.RasterizerState;
        Matrix result = model * trans * projection;
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);
        ItemEffect.Parameters["uTransform"].SetValue(result);
        ItemEffect.Parameters["uTime"].SetValue((float)Main.GlobalTimeWrappedHourly % 1);
        ItemEffect.Parameters["uItemColor"].SetValue(Lighting.GetColor((Owner.Center + OffsetCenter).ToTileCoordinates()).ToVector4());
        ItemEffect.Parameters["uItemGlowColor"].SetValue(Vector4.One);
        if (StandardInfo.frame != null)
        {
            Rectangle frame = StandardInfo.frame.Value;
            Vector2 size = texture.Size();
            ItemEffect.Parameters["uItemFrame"].SetValue(new Vector4(frame.TopLeft() / size, frame.Width / size.X, frame.Height / size.Y));
        }
        else
            ItemEffect.Parameters["uItemFrame"].SetValue(new Vector4(0, 0, 1, 1));
        Main.graphics.GraphicsDevice.Textures[0] = texture;
        Main.graphics.GraphicsDevice.Textures[1] = ModAsset.Misc_0.Value;
        Main.graphics.GraphicsDevice.Textures[2] = ModAsset.BaseTex_15.Value;
        Main.graphics.GraphicsDevice.Textures[3] = StandardInfo.standardGlowTexture;
        Main.graphics.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        Main.graphics.GraphicsDevice.SamplerStates[1] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[2] = sampler;
        Main.graphics.GraphicsDevice.SamplerStates[3] = sampler;
        ItemEffect.CurrentTechnique.Passes[0].Apply();
        for (int n = 0; n < c.Length; n++) c[n].Color = StandardInfo.standardColor * StandardInfo.extraLight;
        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, c, 0, c.Length / 3);
        Main.graphics.GraphicsDevice.RasterizerState = originalState;
        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, sampler, DepthStencilState.Default, RasterizerState.CullNone, null, trans);

        #endregion 好久前的绘制代码，直接搬过来用用试试

        targetedVector = c[4].Position - (OffsetCenter + Owner.Center);
        //if (standardInfo.vertexStandard.scaler > 0)
        //{
        //    targetedVector.Normalize();
        //    targetedVector *= standardInfo.vertexStandard.scaler * offsetSize * ModifyData.actionOffsetSize * sc;
        //}

        #region 显示弹幕碰撞区域

        //spriteBatch.DrawLine(Projectile.Center, targetedVector, Color.Red, 4, true, -Main.screenPosition);
        //spriteBatch.Draw(TextureAssets.MagicPixel.Value, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.Cyan, 0, new Vector2(.5f), 8, 0, 0);

        #endregion 显示弹幕碰撞区域
    }
}