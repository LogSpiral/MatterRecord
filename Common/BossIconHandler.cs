using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using Terraria.UI.Chat;

namespace MatterRecord.Common;

public class BossIconHandler : ITagHandler, ILoadable
{
    public class BossIconHandlerSnippet() : TextSnippet()
    {
        public required Asset<Texture2D> Icon { get; init; }
        public override bool UniqueDraw(bool justCheckingString, out Vector2 size, 
            SpriteBatch spriteBatch, Vector2 position = default, Color color = default, float scale = 1)
        {
            size = new Vector2(20 * scale);
            if (justCheckingString || (color.R == 0 && color.G == 0 && color.B == 0)) return true;
            spriteBatch?.Draw(Icon.Value, position, null, Color.White, 0, default, scale * .75f, 0, 0);
            return true;
        }
        public override float GetStringLength(DynamicSpriteFont font) => Scale * 20;
    }


    TextSnippet ITagHandler.Parse(string text, Color baseColor, string options)
    {
        var path = $"Terraria/Images/NPC_Head_Boss_{text}";
        if (!ModContent.HasAsset(path))
            return new TextSnippet(text);
        return new BossIconHandlerSnippet() { Icon = ModContent.Request<Texture2D>(path) };
    }

    void ILoadable.Load(Mod mod)
    {
        ChatManager.Register<BossIconHandler>("boss");
    }
    void ILoadable.Unload()
    {
    }
}

