using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.ZenithBoulder.ZenithBoulderEx;

public class ZenithBoulderExSystem : ModSystem
{

    public static ZenithBoulderExSystem Instance { get; private set; }
    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }

    private bool _useOffset;
    private Vector2 _screenOffset;
    public Vector2 ScreenOffset
    {
        get => _screenOffset;
        set
        {
            _useOffset = true;
            _screenOffset = value;
        }
    }
    public override void ModifyScreenPosition()
    {
        if (!_useOffset) return;
        if (_screenOffset.Length() > 32)
            _screenOffset = _screenOffset.SafeNormalize(default) * 32;
        _screenOffset *= 0.8f;
        Main.screenPosition += _screenOffset;
        if (_screenOffset.LengthSquared() < 1)
        {
            _screenOffset = default;
            _useOffset = false;
        }
    }
}
