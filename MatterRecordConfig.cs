using MatterRecord.Contents.LordOfTheFlies;
using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.UI;
using Terraria.UI.Chat;
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



public class MatterRecordItemHandler : ITagHandler, ILoadable
{
    public class MatterRecordItemSnippet(Item item) : ItemTagHandler.ItemSnippet(item)
    {
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
        {
            RecordLockItem.UnlockStyleInventoryVisualMask = true;
            return base.UniqueDraw(justCheckingString, out size, spriteBatch, position, color, scale);
        }
    }


    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        Item item = new Item();
        if (int.TryParse(text, out var result) && result < ItemLoader.ItemCount)
            item.netDefaults(result);

        // Add support for [i:ModItem.FullName] ([i:ExampleMod/ExampleItem]). Coincidentally support [i:ItemID.FieldName] ([i:GoldBar])
        if (ItemID.Search.TryGetId(text, out result))
            item.netDefaults(result);

        if (item.type <= 0)
            return new TextSnippet(text);

        item.stack = 1;
        // Options happen here, we add MID (=ModItemData) options
        if (options != null)
        {
            // Don't know why all these options are here in vanilla,
            // Since it only assumed one option (stack OR prefix, since prefixed items don't stack)
            string[] array = options.Split(',');
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length == 0)
                    continue;

                switch (array[i][0])
                {
                    case 'd': // MID is present, we will override
                        item = ItemIO.FromBase64(array[i].Substring(1));
                        break;
                    case 's':
                    case 'x':
                        {
                            if (int.TryParse(array[i].Substring(1), out var result3))
                                item.stack = Utils.Clamp(result3, 1, item.maxStack);

                            break;
                        }
                    case 'p':
                        {
                            if (int.TryParse(array[i].Substring(1), out var result2))
                                item.Prefix((byte)Utils.Clamp(result2, 0, PrefixLoader.PrefixCount));

                            break;
                        }
                }
            }
        }

        string text2 = "";
        if (item.stack > 1)
            text2 = " (" + item.stack + ")";

        return new MatterRecordItemSnippet(item)
        {
            Text = "[" + item.AffixName() + text2 + "]",
            CheckForHover = true,
            DeleteWhole = true
        };
    }

    void ILoadable.Load(Mod mod)
    {
        ChatManager.Register<MatterRecordItemHandler>("records");
    }
    void ILoadable.Unload()
    {
    }
}

