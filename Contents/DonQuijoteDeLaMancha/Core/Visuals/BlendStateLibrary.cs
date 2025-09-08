using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;

public class BlendStateLibrary:ILoadable
{
    public static BlendState AllOne { get; private set; }
    public static BlendState InverseColor { get; private set; }
    public static BlendState SoftAdditive { get; private set; }//from yiyang233
    public static BlendState NonPremultipliedFullAlpha { get; private set; }

    private static void InitializeBlendStates()
    {
        AllOne = new BlendState
        {
            Name = "LogSpiralLibraryMod.AllOne",
            ColorSourceBlend = Blend.One,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
        };
        InverseColor = new BlendState()
        {
            Name = "LogSpiralLibraryMod.InverseColor",
            ColorDestinationBlend = Blend.InverseSourceColor,
            ColorSourceBlend = Blend.InverseDestinationColor,
            AlphaDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.Zero
        };
        SoftAdditive = new BlendState()
        {
            Name = "LogSpiralLibraryMod.SoftAdditve",
            ColorDestinationBlend = Blend.One,
            ColorSourceBlend = Blend.InverseDestinationColor,
            AlphaDestinationBlend = Blend.One,
            AlphaSourceBlend = Blend.SourceAlpha
        };
        NonPremultipliedFullAlpha = new BlendState()
        {
            Name = "LogSpiralLibraryMod.NonPremultipliedFullAlpha",
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha
        };
    }

    public void Load(Mod mod)
    {
        InitializeBlendStates();
    }

    public void Unload()
    {
    }
}
