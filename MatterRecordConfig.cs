using MatterRecord.Contents.LordOfTheFlies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.UI;
using Terraria.UI;
namespace MatterRecord;
public class MatterRecordConfig : ModConfig
{
    public static MatterRecordConfig Instance => ModContent.GetInstance<MatterRecordConfig>();
    public override ConfigScope Mode => ConfigScope.ServerSide;
    public bool DonQuijoteSlashActive = false;

    [CustomModConfigItem(typeof(LordOfTheFliesResetElement))]
    public object LordOfTheFliesUIReset;

    private class LordOfTheFliesResetElement : ConfigElement<object>
    {
        public override void DrawSelf(SpriteBatch spriteBatch)
        {
            var dimensions = GetDimensions();
            float num = dimensions.Width + 1f;
            var pos = new Vector2(dimensions.X, dimensions.Y);
            var color = IsMouseHovering ? UICommon.DefaultUIBlue : UICommon.DefaultUIBlue.MultiplyRGBA(new Color(180, 180, 180));
            DrawPanel2(spriteBatch, pos, TextureAssets.SettingsPanel.Value, num, dimensions.Height, color);

            base.DrawSelf(spriteBatch);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            LordOfTheFilesSystem.SetOffsetValue(default);
            SoundEngine.PlaySound(SoundID.MaxMana);
        }
    }
}
