using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;

namespace MatterRecord.Contents.EternalWine;

public class EternalWinePlayer : ModPlayer
{
    private float _lifeDebt;
    private float _lifeDebtMax;


    public void SetLifeDebt(float debt, float maxDebt)
    {
        _lifeDebt = debt;
        _lifeDebtMax = maxDebt;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["Debt"] = _lifeDebt;
        tag["MaxDebt"] = _lifeDebtMax;
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        _lifeDebt = tag.Get<float>("Debt");
        _lifeDebtMax = tag.Get<float>("MaxDebt");
        base.LoadData(tag);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        EternalWineSync.Get(Player.whoAmI, _lifeDebt, _lifeDebtMax).Send(toWho, fromWho);
    }

    public override void Load()
    {
        IL_Player.UpdateLifeRegen += LifeDebtPaying;
        base.Load();
    }

    private void LifeDebtPaying(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        for (int n = 0; n < 3; n++)
            if (!cursor.TryGotoNext(i => i.MatchLdcI4(120)))
                return;

        cursor.Index--;
        cursor.EmitDelegate<Action<Player>>(plr =>
        {
            var mplr = plr.GetModPlayer<EternalWinePlayer>();
            while (mplr._lifeDebt > 0 && plr.lifeRegenCount >= 120)
            {
                plr.lifeRegenCount -= 120;
                mplr._lifeDebt--;
            }
        });
        cursor.EmitLdarg0();
    }

    public override void Unload()
    {
        IL_Player.UpdateLifeRegen -= LifeDebtPaying;

        base.Unload();
    }

    public override void ResetEffects()
    {
        if (_lifeDebt <= 0 && _lifeDebtMax != -1)
        {
            _lifeDebt = _lifeDebtMax = -1;
            SoundEngine.PlaySound(SoundID.Item4, Player.Center);
        }
        if (_lifeDebt > 0)
            Player.AddBuff(ModContent.BuffType<LifeRegenStagnant>(), 2);
        base.ResetEffects();
    }

    public override void UpdateDead()
    {
        _lifeDebt = _lifeDebtMax = -1;
        base.UpdateDead();
    }

    public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
    {
        if (_lifeDebtMax > 0 && Main.myPlayer == Player.whoAmI && !Player.DeadOrGhost)
        {
            Vector2 cen = Player.Center + Player.gfxOffY * Vector2.UnitY - Main.screenPosition - new Vector2(16, 128);
            // var direction = Player.gravDir < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            var direction = SpriteEffects.None;
            drawInfo.DrawDataCache.Add(
                new DrawData(
                    ModAsset.LifeRegenStagnant_Recover.Value,
                    cen,
                    null,
                    Color.White,
                    0,
                    new Vector2(),
                    1f,
                    direction));

            drawInfo.DrawDataCache.Add(
                new DrawData(
                    ModAsset.LifeRegenStagnant.Value,
                    cen, // + (Player.gravDir < 0 ? Vector2.UnitY * (int)(32 - 32f * LifeDebt / LifeDebtMax) : Vector2.Zero)
                    new Rectangle(0, 0, 32, (int)(32f * _lifeDebt / _lifeDebtMax)),
                    Color.White,
                    0,
                    new Vector2(),
                    1f,
                    direction));

            string text = $"{Language.GetTextValue("Mods.MatterRecord.Items.EternalWine.LifeDebt")}{_lifeDebt}/{_lifeDebtMax}";
            ChatManager.DrawColorCodedStringWithShadow(
                Main.spriteBatch,
                FontAssets.MouseText.Value,
                text,
                cen + new Vector2(16, 48),
                Color.White,
                Color.Black,
                0,
                FontAssets.MouseText.Value.MeasureString(text) * .5f,
                Vector2.One);

            //Main.spriteBatch.DrawString(FontAssets.MouseText.Value,, cen + Vector2.UnitY * 48, Color.White);
        }
        base.ModifyDrawInfo(ref drawInfo);
    }
}
