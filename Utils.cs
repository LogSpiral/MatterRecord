using MatterRecord.Contents.DonQuijoteDeLaMancha.Core.Visuals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using static Microsoft.Xna.Framework.Graphics.SpriteBatch;

namespace MatterRecord;
//↓↓赞美裙↓↓
/// <summary>
/// <para>DXTsT自制的粒子ID表</para>
/// <para>制作时间：2017/1/31</para>
/// <para>版权所有：DXTsT 与 四十九落星制作组</para>
/// <para>说明：以下字段带有（！）标识符的说明此粒子效果会在黑暗中自发光</para>
/// <para>带有（.）标识符说明此粒子效果会高亮显示但是不会发光</para>
/// <para>其余Dust全部都不会发光！</para>
///
///
/// <para>赞美裙裙！！！</para>
/// <para>\裙子/\裙子/\裙子/\裙子/\裙子/</para>
/// </summary>
public static class MyDustId
{
    /// <summary>
    /// brown dirt
    /// </summary>
    public const int BrownDirt = 0;

    /// <summary>
    /// grey stone
    /// </summary>
    public const int GreyStone = 1;

    /// <summary>
    /// thick green grass
    /// </summary>
    public const int GreenGrass = 2;

    /// <summary>
    /// thin green grass
    /// </summary>
    public const int ThinGreenGrass = 3;

    /// <summary>
    /// grey pebbles
    /// </summary>
    public const int GreyPebble = 4;

    /// <summary>
    /// red blood
    /// </summary>
    public const int RedBlood = 5;

    /// <summary>
    /// (!)orange fire, emits orange light !WARNING
    /// </summary>
    public const int Fire = 6;

    /// <summary>
    /// brown wood
    /// </summary>
    public const int Wood = 7;

    /// <summary>
    /// purple gems
    /// </summary>
    public const int PurpleGems = 8;

    /// <summary>
    /// orange gems
    /// </summary>
    public const int OrangeGems = 9;

    /// <summary>
    /// yellow gems
    /// </summary>
    public const int YellowGems = 10;

    /// <summary>
    /// white gems
    /// </summary>
    public const int WhiteGems = 11;

    /// <summary>
    /// red gems
    /// </summary>
    public const int RedGems = 12;

    /// <summary>
    /// cyan gems
    /// </summary>
    public const int CyanGems = 13;

    /// <summary>
    /// purple corruption particle with no gravity
    /// </summary>
    public const int CorruptionParticle = 14;

    /// <summary>
    /// (!)white amd blue magic fx, emits pale blue light
    /// </summary>
    public const int BlueMagic = 15;

    /// <summary>
    /// bluish white clouds like hermes boots
    /// </summary>
    public const int WhiteClouds = 16;

    /// <summary>
    /// thin grey material
    /// </summary>
    public const int ThinGrey = 17;

    /// <summary>
    /// thin sickly green material
    /// </summary>
    public const int SicklyGreen = 18;

    /// <summary>
    /// thin yellow material
    /// </summary>
    public const int ThinYellow = 19;

    /// <summary>
    /// (!)white lingering, emits cyan light
    /// </summary>
    public const int WhiteLingering = 20;

    /// <summary>
    /// (!)purple lingering, emits purple light
    /// </summary>
    public const int PurpleLingering = 21;

    /// <summary>
    /// brown material
    /// </summary>
    public const int Brown = 22;

    /// <summary>
    /// orange material
    /// </summary>
    public const int orange = 23;

    /// <summary>
    /// thin brown material
    /// </summary>
    public const int ThinBrown = 24;

    /// <summary>
    /// copper
    /// </summary>
    public const int Copper = 25;

    /// <summary>
    /// iron
    /// </summary>
    public const int iron = 26;

    /// <summary>
    /// (!)purple fx, emits bright purple light
    /// </summary>
    public const int PurpleLight = 27;

    /// <summary>
    /// dull copper
    /// </summary>
    public const int DullCopper = 28;

    /// <summary>
    /// (!)dark blue, emits pale pink light !WARNING
    /// </summary>
    public const int DarkBluePinkLight = 29;

    /// <summary>
    /// silver material
    /// </summary>
    public const int Silver = 30;

    /// <summary>
    /// yellowish white cloud material
    /// </summary>
    public const int Smoke = 31;

    /// <summary>
    /// yellow sand
    /// </summary>
    public const int Sand = 32;

    /// <summary>
    /// water, highly transparent
    /// </summary>
    public const int Water = 33;

    /// <summary>
    /// (!)red fx, emits red light !WARNING
    /// </summary>
    public const int RedLight = 35;

    /// <summary>
    /// muddy pale material
    /// </summary>
    public const int MuddyPale = 36;

    /// <summary>
    /// dark grey material
    /// </summary>
    public const int DarkGrey = 37;

    /// <summary>
    /// muddy brown material
    /// </summary>
    public const int MuddyBrown = 38;

    /// <summary>
    /// bright green jungle grass
    /// </summary>
    public const int JungleGrass = 39;

    /// <summary>
    /// bright green thin grass
    /// </summary>
    public const int ThinGrass = 40;

    /// <summary>
    /// (!)dark blue wandering circles, emits bright cyan light !WARNING
    /// </summary>
    public const int BlueCircle = 41;

    /// <summary>
    /// thin teal material
    /// </summary>
    public const int ThinTeal = 42;

    /// <summary>
    /// (!)bright green spores that lingers for a while, emits light green light
    /// </summary>
    public const int GreenSpores = 44;

    /// <summary>
    /// (!)light blue circles, emits purple light
    /// </summary>
    public const int LightBlueCircle = 45;

    /// <summary>
    /// green material with no gravity
    /// </summary>
    public const int GreenMaterial = 46;

    /// <summary>
    /// thin cyan grass
    /// </summary>
    public const int CyanGrass = 47;

    /// <summary>
    /// pink water, highly transparent
    /// </summary>
    public const int PinkWater = 52;

    /// <summary>
    /// grey material
    /// </summary>
    public const int GreyMaterial = 53;

    /// <summary>
    /// black material
    /// </summary>
    public const int BlackMaterial = 54;

    /// <summary>
    /// (!)bright orange thick fx, emits yellow light
    /// </summary>
    public const int OrangeFx = 55;

    /// <summary>
    /// (!)cyan fx, emits pale blue light
    /// </summary>
    public const int CyanFx = 56;

    /// <summary>
    /// (!)small yellow hallowed fx, emis yellow light
    /// </summary>
    public const int YellowHallowFx = 57;

    /// <summary>
    /// (!)hot and pale pink magic fx, emits pink light
    /// </summary>
    public const int PinkMagic = 58;

    /// <summary>
    /// (!)blue torch, emits pure blue light !WARNING
    /// </summary>
    public const int BlueTorch = 59;

    /// <summary>
    /// (!)red torch, emits pure red light !WARNING
    /// </summary>
    public const int RedTorch = 60;

    /// <summary>
    /// (!)green torch, emits pure green light !WARNING
    /// </summary>
    public const int GreenTorch = 61;

    /// <summary>
    /// (!)purple torch, emits purple light !WARNING
    /// </summary>
    public const int PurpleTorch = 62;

    /// <summary>
    /// (!)white torch, emits bright white light !WARNING
    /// </summary>
    public const int WhiteTorch = 63;

    /// <summary>
    /// (!)yellow torch, emits deep yellow light !WARNING
    /// </summary>
    public const int YellowTorch = 64;

    /// <summary>
    /// (!)demon torch, emits pulsating pink/purple light !WARNING
    /// </summary>
    public const int DemonTorch = 65;

    /// <summary>
    /// (!)White transparent !WARNING
    /// </summary>
    public const int WhiteTransparent = 66;

    /// <summary>
    /// (!)cyan ice crystals, emits cyan light
    /// </summary>
    public const int CyanIce = 67;

    /// <summary>
    /// (.)dark cyan ice crystals, emits very faint blue light, glows in disabled gravity
    /// </summary>
    public const int DarkCyanIce = 68;

    /// <summary>
    /// thin pink material
    /// </summary>
    public const int ThinPink = 69;

    /// <summary>
    /// (.)thin transparent purple material, emits faint purple light, glows in disabled gravity
    /// </summary>
    public const int TransparentPurple = 70;

    /// <summary>
    /// (!)transparent pink fx, emits faint pink light
    /// </summary>
    public const int TransparentPinkFx = 71;

    /// <summary>
    /// (!)solid pink fx, emits faint pink light
    /// </summary>
    public const int SolidPinkFx = 72;

    /// <summary>
    /// (!)solid bright pink fx, emits pink light
    /// </summary>
    public const int BrightPinkFx = 73;

    /// <summary>
    /// (!)solid bright green fx, emits green light
    /// </summary>
    public const int BrightGreenFx = 74;

    /// <summary>
    /// (!)green cursed torch !WARNING
    /// </summary>
    public const int CursedFire = 75;

    /// <summary>
    /// snowfall, lasts a long time
    /// </summary>
    public const int Snow = 76;

    /// <summary>
    /// thin grey material
    /// </summary>
    public const int ThinGrey1 = 77;

    /// <summary>
    /// thin copper material
    /// </summary>
    public const int ThinCopper = 78;

    /// <summary>
    /// thin yellow material
    /// </summary>
    public const int ThinYellow1 = 79;

    /// <summary>
    /// ice block material
    /// </summary>
    public const int IceBlock = 80;

    /// <summary>
    /// iron material
    /// </summary>
    public const int Iron = 81;

    /// <summary>
    /// silty material
    /// </summary>
    public const int Silty = 82;

    /// <summary>
    /// sickly green material
    /// </summary>
    public const int SicklyGreen1 = 83;

    /// <summary>
    /// bluish grey material
    /// </summary>
    public const int BluishGrey = 84;

    /// <summary>
    /// thin sandy materiial
    /// </summary>
    public const int ThinSandy = 85;

    /// <summary>
    /// (!)transparent pink material, emits pink light
    /// </summary>
    public const int PinkTrans = 86;

    /// <summary>
    /// (!)transparent yellow material, emits yellow light
    /// </summary>
    public const int YellowTrans = 87;

    /// <summary>
    /// (!)transparent blue material, emits blue light
    /// </summary>
    public const int BlueTrans = 88;

    /// <summary>
    /// (!)transparent green material, emits green light
    /// </summary>
    public const int GreenTrans = 89;

    /// <summary>
    /// (!)transparent red material, emits red light
    /// </summary>
    public const int RedTrans = 90;

    /// <summary>
    /// (!)transparent white material, emits white light
    /// </summary>
    public const int WhiteTrans = 91;

    /// <summary>
    /// (!)transparent cyan material, emits cyan light
    /// </summary>
    public const int CyanTrans = 92;

    /// <summary>
    /// thin dark green grass
    /// </summary>
    public const int DarkGrass = 93;

    /// <summary>
    /// thin pale dark green grass
    /// </summary>
    public const int PaleDarkGrass = 94;

    /// <summary>
    /// thin dark red grass
    /// </summary>
    public const int DarkRedGrass = 95;

    /// <summary>
    /// thin blackish green grass
    /// </summary>
    public const int BlackGreenGrass = 96;

    /// <summary>
    /// thin dark red grass
    /// </summary>
    public const int DarkRedGrass1 = 97;

    /// <summary>
    /// purple water, highly transparent
    /// </summary>
    public const int PurpleWater = 98;

    /// <summary>
    /// cyan water, highly transparent
    /// </summary>
    public const int CyanWater = 99;

    /// <summary>
    /// pink water, highly transparent
    /// </summary>
    public const int PinkWater1 = 100;

    /// <summary>
    /// cyan water, highly transparent
    /// </summary>
    public const int CyanWater1 = 101;

    /// <summary>
    /// orange water, highly transparent
    /// </summary>
    public const int OrangeWater = 102;

    /// <summary>
    /// dark blue water, highly transparent
    /// </summary>
    public const int DarkBlueWater = 103;

    /// <summary>
    /// hot pink water, highly transparent
    /// </summary>
    public const int HotPinkWater = 104;

    /// <summary>
    /// red water, highly transparent
    /// </summary>
    public const int RedWater = 105;

    /// <summary>
    /// (.)transparent red/green/blue material, glows in the dark
    /// </summary>
    public const int RgbMaterial = 106;

    /// <summary>
    /// (!)short green powder, emits green light
    /// </summary>
    public const int GreenFXPowder = 107;

    /// <summary>
    /// light pale purple round material
    /// </summary>
    public const int PurpleRound = 108;

    /// <summary>
    /// black material
    /// </summary>
    public const int BlackMaterial1 = 109;

    /// <summary>
    /// (.)bright green bubbles, emits very faint green light
    /// </summary>
    public const int GreenBubble = 110;

    /// <summary>
    /// (.)bright cyan bubbles, emits very faint cyan light
    /// </summary>
    public const int CyanBubble = 111;

    /// <summary>
    /// (.)bright pink bubbles, emits very faint pink light
    /// </summary>
    public const int PinkBubble = 112;

    /// <summary>
    /// (.)blue ice crystals, glows in the dark
    /// </summary>
    public const int BlueIce = 113;

    /// <summary>
    /// (.)bright pink/yellow bubbles, emits very faint pink light
    /// </summary>
    public const int PinkYellowBubble = 114;

    /// <summary>
    /// red grass
    /// </summary>
    public const int RedGrass = 115;

    /// <summary>
    /// blueish green grass
    /// </summary>
    public const int BlueGreenGrass = 116;

    /// <summary>
    /// red grass
    /// </summary>
    public const int RedGrass1 = 117;

    /// <summary>
    /// purple gems
    /// </summary>
    public const int PurpleGems1 = 118;

    /// <summary>
    /// pink gems
    /// </summary>
    public const int PinkGems = 119;

    /// <summary>
    /// pale pink gems
    /// </summary>
    public const int PalePinkGems = 120;

    /// <summary>
    /// thin grey material
    /// </summary>
    public const int ThinGrey2 = 121;

    /// <summary>
    /// thin iron material
    /// </summary>
    public const int ThinIron = 122;

    /// <summary>
    /// hot pink bubble material
    /// </summary>
    public const int HotPinkBubble = 123;

    /// <summary>
    /// yellowish white bubbles
    /// </summary>
    public const int YellowWhiteBubble = 124;

    /// <summary>
    /// thin red material
    /// </summary>
    public const int ThinRed = 125;

    /// <summary>
    /// thin grey material
    /// </summary>
    public const int ThinGrey3 = 126;

    /// <summary>
    /// (!)reddish orange fire, emits orange light
    /// </summary>
    public const int OrangeFire = 127;

    /// <summary>
    /// green gems
    /// </summary>
    public const int GreenGems = 128;

    /// <summary>
    /// thin brown material
    /// </summary>
    public const int ThinBrown1 = 129;

    /// <summary>
    /// (!)trailing red falling fireworks, emits red light
    /// </summary>
    public const int TrailingRed = 130;

    /// <summary>
    /// (!)trailing green rising fireworks, emits green light
    /// </summary>
    public const int TrailingGreen = 131;

    /// <summary>
    /// (!)trailing cyan falling fireworks, emits cyan light
    /// </summary>
    public const int TrailingCyan = 132;

    /// <summary>
    /// (!)trailing yellow falling fireworks, emits cyan light
    /// </summary>
    public const int TrailingYellow = 133;

    /// <summary>
    /// trailing pink falling fireworks
    /// </summary>
    public const int TrailingPink = 134;

    /// <summary>
    /// (!)cyan ice torch, emits cyan light !WARNING
    /// </summary>
    public const int IceTorch = 135;

    /// <summary>
    /// red material
    /// </summary>
    public const int Red = 136;

    /// <summary>
    /// bright blue/cyan material
    /// </summary>
    public const int BrightCyan = 137;

    /// <summary>
    /// bright orange/brown material
    /// </summary>
    public const int BrightOrange = 138;

    /// <summary>
    /// cyan confetti
    /// </summary>
    public const int CyanConfetti = 139;

    /// <summary>
    /// green confetti
    /// </summary>
    public const int GreenConfetti = 140;

    /// <summary>
    /// pink confetti
    /// </summary>
    public const int PinkConfetti = 141;

    /// <summary>
    /// yellow confetti
    /// </summary>
    public const int YellowConfetti = 142;

    /// <summary>
    /// light grey stone
    /// </summary>
    public const int LightGreyStone = 143;

    /// <summary>
    /// vivid copper stone
    /// </summary>
    public const int CopperStone = 144;

    /// <summary>
    /// pink stone
    /// </summary>
    public const int PinkStone = 145;

    /// <summary>
    /// green/brown material mix
    /// </summary>
    public const int GreenBrown = 146;

    /// <summary>
    /// orange material
    /// </summary>
    public const int Orange = 147;

    /// <summary>
    /// desaturated red material
    /// </summary>
    public const int RedDesaturated = 148;

    /// <summary>
    /// white material
    /// </summary>
    public const int White = 149;

    /// <summary>
    /// black/yellow/bluishwhite material
    /// </summary>
    public const int BlackYellowBluishwhite = 150;

    /// <summary>
    /// thin white material
    /// </summary>
    public const int ThinWhite = 151;

    /// <summary>
    /// (!)bright orange bubbles !WARNING
    /// </summary>
    public const int OrangeBubble = 152;

    /// <summary>
    /// bright orange bubble material
    /// </summary>
    public const int OrangeBubbleMaterial = 153;

    /// <summary>
    /// pale blue thin material
    /// </summary>
    public const int BlueThin = 154;

    /// <summary>
    /// thin dark brown material
    /// </summary>
    public const int DarkBrown = 155;

    /// <summary>
    /// (!)bright blue/white bubble material, emits pale blue light
    /// </summary>
    public const int BlueWhiteBubble = 156;

    /// <summary>
    /// (.)thin green fx, glows in the dark
    /// </summary>
    public const int GreenFx = 157;

    /// <summary>
    /// (!)orange fire, emits orange light !WARNING
    /// </summary>
    public const int OrangeFire1 = 158;

    /// <summary>
    /// (!)flickering yellow fx, emits yellow light !WARNING
    /// </summary>
    public const int YellowFx = 159;

    /// <summary>
    /// (!)shortlived cyan fx, emits bright cyan light
    /// </summary>
    public const int CyanShortFx = 160;

    /// <summary>
    /// cyan material
    /// </summary>
    public const int CyanMaterial = 161;

    /// <summary>
    /// (!)shortlived orange fx, emits bright orange light
    /// </summary>
    public const int OrangeShortFx = 162;

    /// <summary>
    /// (.)bright green thin material, glows in the dark
    /// </summary>
    public const int BrightGreen = 163;

    /// <summary>
    /// (!)flickering pink fx, emits hot pink light !WARNING
    /// </summary>
    public const int PinkFx = 164;

    /// <summary>
    /// white/blue bubble material
    /// </summary>
    public const int WhiteBlueBubble = 165;

    /// <summary>
    /// thin bright pink material
    /// </summary>
    public const int PinkThinBright = 166;

    /// <summary>
    /// thin green material
    /// </summary>
    public const int ThinGreen = 167;

    /// <summary>
    /// !bright pink bubbles !WARNING
    /// </summary>
    public const int PinkBrightBubble = 168;

    /// <summary>
    /// (!)yellow fx, emits deep yellow light !WARNING
    /// </summary>
    public const int YellowFx1 = 169;

    /// <summary>
    /// (.)thin orange fx, emits faint white light
    /// </summary>
    public const int Ichor = 170;

    /// <summary>
    /// bright purple bubble material
    /// </summary>
    public const int PurpleBubble = 171;

    /// <summary>
    /// (.)light blue particles, emits faint blue light
    /// </summary>
    public const int BlueParticle = 172;

    /// <summary>
    /// (!)shortlived purple fx, emits bright purple light
    /// </summary>
    public const int PurpleShortFx = 173;

    /// <summary>
    /// (!)bright orange bubble material, emits reddish orange light
    /// </summary>
    public const int OrangeFire2 = 174;

    /// <summary>
    /// (.)shortlived white fx, glows in the dark
    /// </summary>
    public const int WhiteShortFx = 175;

    /// <summary>
    /// light blue particles
    /// </summary>
    public const int LightBlueParticle = 176;

    /// <summary>
    /// light pink particles
    /// </summary>
    public const int LightPinkParticle = 177;

    /// <summary>
    /// light green particles
    /// </summary>
    public const int LightGreenParticle = 178;

    /// <summary>
    /// light purple particles
    /// </summary>
    public const int LightPurpleParticle = 179;

    /// <summary>
    /// (!)light cyan particles, glows in the dark
    /// </summary>
    public const int LightCyanParticle = 180;

    /// <summary>
    /// (.)light cyan/pink bubble material, glows in the dark
    /// </summary>
    public const int CyanPinkBubble = 181;

    /// <summary>
    /// (.)light red bubble material, barely emits red light
    /// </summary>
    public const int RedBubble = 182;

    /// <summary>
    /// (.)transparent red bubble material, glows in the dark
    /// </summary>
    public const int RedTransBubble = 183;

    /// <summary>
    /// sickly pale greenish grey particles that stay in place
    /// </summary>
    public const int GreenishGreyParticle = 184;

    /// <summary>
    /// (!)light cyan crystal material, emits cyan light
    /// </summary>
    public const int CyanCrystal = 185;

    /// <summary>
    /// pale dark blue smoke
    /// </summary>
    public const int DarkBlueSmoke = 186;

    /// <summary>
    /// (!)light cyan particles, emits cyan light
    /// </summary>
    public const int LightCyanParticle1 = 187;

    /// <summary>
    /// bright green bubbles
    /// </summary>
    public const int GreenBubble1 = 188;

    /// <summary>
    /// thin orange material
    /// </summary>
    public const int OrangeMaterial = 189;

    /// <summary>
    /// thin gold material
    /// </summary>
    public const int GoldMaterial = 190;

    /// <summary>
    /// black flakes
    /// </summary>
    public const int BlackFlakes = 191;

    /// <summary>
    /// snow material
    /// </summary>
    public const int SnowMaterial = 192;

    /// <summary>
    /// green material
    /// </summary>
    public const int GreenMaterial1 = 193;

    /// <summary>
    /// thin brown material
    /// </summary>
    public const int BrownMaterial = 194;

    /// <summary>
    /// thin black material
    /// </summary>
    public const int BlackMaterial2 = 195;

    /// <summary>
    /// thin green material
    /// </summary>
    public const int ThinGreen1 = 196;

    /// <summary>
    /// (.)thin bright cyan material, glows in the dark
    /// </summary>
    public const int BrightCyanMaterial = 197;

    /// <summary>
    /// black/white particles
    /// </summary>
    public const int BlackWhiteParticle = 198;

    /// <summary>
    /// pale purple/black/grey particles
    /// </summary>
    public const int PurpleBlackGrey = 199;

    /// <summary>
    /// pink particles
    /// </summary>
    public const int PinkParticle = 200;

    /// <summary>
    /// light pink particles
    /// </summary>
    public const int LightPinkParticle1 = 201;

    /// <summary>
    /// light cyan particles
    /// </summary>
    public const int LightCyanParticle2 = 202;

    /// <summary>
    /// grey particles
    /// </summary>
    public const int GreyParticle = 203;

    /// <summary>
    /// (.)white particles, glows in the dark
    /// </summary>
    public const int WhiteParticle = 204;

    /// <summary>
    /// (.)thin pink material, barely emits pink light
    /// </summary>
    public const int ThinPinkMaterial = 205;

    /// <summary>
    /// (!)shortlived cyan fx, emits bright blue light
    /// </summary>
    public const int CyanShortFx1 = 206;

    /// <summary>
    /// thin brown material
    /// </summary>
    public const int BrownMaterial1 = 207;

    /// <summary>
    /// orange stone
    /// </summary>
    public const int OrangeStone = 208;

    /// <summary>
    /// pale green stone
    /// </summary>
    public const int PaleGreenStone = 209;

    /// <summary>
    /// off white material
    /// </summary>
    public const int OffWhite = 210;

    /// <summary>
    /// bright blue particles
    /// </summary>
    public const int BrightBlueParticle = 211;

    /// <summary>
    /// white particles
    /// </summary>
    public const int WhiteParticle1 = 212;

    /// <summary>
    /// (.)shortlived tiny white fx, barely emits white light
    /// </summary>
    public const int WhiteShortFx1 = 213;

    /// <summary>
    /// thin pale brown material
    /// </summary>
    public const int Thin = 214;

    /// <summary>
    /// thin khaki material
    /// </summary>
    public const int ThinKhaki = 215;

    /// <summary>
    /// pale pink material
    /// </summary>
    public const int Pale = 216;

    /// <summary>
    /// cyan particles
    /// </summary>
    public const int Cyan = 217;

    /// <summary>
    /// hot pink particles
    /// </summary>
    public const int Hot = 218;

    /// <summary>
    /// (!)trailing red flying fireworks, emits orange light
    /// </summary>
    public const int TrailingRed1 = 219;

    /// <summary>
    /// (!)trailing green flying fireworks, emits green light
    /// </summary>
    public const int TrailingGreen1 = 220;

    /// <summary>
    /// (!)trailing blue flying fireworks, emits pale blue light
    /// </summary>
    public const int TrailingBlue = 221;

    /// <summary>
    /// (!)trailing yellow flying fireworks, emits yellow light
    /// </summary>
    public const int TrailingYellow1 = 222;

    /// <summary>
    /// (.)trailing red flying fireworks, glows in the dark
    /// </summary>
    public const int TrailingRed2 = 223;

    /// <summary>
    /// thin blue material
    /// </summary>
    public const int ThinBlue = 224;

    /// <summary>
    /// orange material
    /// </summary>
    public const int OrangeMaterial1 = 225;

    /// <summary>
    ///
    /// </summary>
    public const int ElectricCyan = 226;

    /// <summary>
    /// (!)Lunar fire!!!
    /// </summary>
    public const int CyanLunarFire = 229;

    /// <summary>
    /// (!)flickering Purple fx, emits Purple light !WARNING
    /// </summary>
    public const int PurpleFx = 230;
}
public static class MySoundID
{
    /// <summary>
    /// 最普通最常见的挥砍效果1
    /// </summary>
    public static SoundStyle SwooshNormal_1 => SoundID.Item1;

    /// <summary>
    /// 吃饼干般的嘎嘣声
    /// </summary>
    public static SoundStyle Eat => SoundID.Item2;

    /// <summary>
    /// 喝药水
    /// </summary>
    public static SoundStyle Drink => SoundID.Item3;

    /// <summary>
    /// 使用魔法水晶之类的声音
    /// </summary>
    public static SoundStyle MagicShiny => SoundID.Item4;

    /// <summary>
    /// 发射箭矢声
    /// </summary>
    public static SoundStyle ShootArrow => SoundID.Item5;

    /// <summary>
    /// 魔镜传送等
    /// </summary>
    public static SoundStyle Teleport => SoundID.Item6;

    /// <summary>
    /// 回旋镖旋转 吹叶机使用
    /// </summary>
    public static SoundStyle BoomerangRotating => SoundID.Item7;

    /// <summary>
    /// 法杖使用音效
    /// </summary>
    public static SoundStyle MagicStaff => SoundID.Item8;

    /// <summary>
    /// 落星等
    /// </summary>
    public static SoundStyle FallingStar => SoundID.Item9;

    /// <summary>
    /// 弹幕撞击物块声
    /// </summary>
    public static SoundStyle ProjectileHit => SoundID.Item10;

    /// <summary>
    /// 火枪等
    /// </summary>
    public static SoundStyle Gun => SoundID.Item11;

    /// <summary>
    /// 单独激光
    /// </summary>
    public static SoundStyle LaserBeam => SoundID.Item12;

    /// <summary>
    /// 火箭靴喷气 棱镜等
    /// </summary>
    public static SoundStyle RocketBoots1 => SoundID.Item13;

    /// <summary>
    /// 爆炸
    /// </summary>
    public static SoundStyle Explosion => SoundID.Item14;

    /// <summary>
    /// 光剑等
    /// </summary>
    public static SoundStyle Phasesaber => SoundID.Item15;

    /// <summary>
    /// 整蛊坐垫等...
    /// </summary>
    public static SoundStyle Suck => SoundID.Item16;

    /// <summary>
    /// 蜜蜂发射针刺等
    /// </summary>
    public static SoundStyle ShootStinger => SoundID.Item17;

    /// <summary>
    /// 最普通最常见的挥砍效果2
    /// </summary>
    public static SoundStyle SwooshNormal_2 => SoundID.Item18;

    /// <summary>
    /// 最普通最常见的挥砍效果3
    /// </summary>
    public static SoundStyle SwooshNormal_3 => SoundID.Item19;

    /// <summary>
    /// 火焰花 火鞭之类小型火魔法法杖
    /// </summary>
    public static SoundStyle FireStaff => SoundID.Item20;

    /// <summary>
    /// 水箭
    /// </summary>
    public static SoundStyle WaterBolt => SoundID.Item21;

    /// <summary>
    /// 链锯和电钻1
    /// </summary>
    public static SoundStyle SawAndDrill_1 => SoundID.Item22;

    /// <summary>
    /// 链锯和电钻2
    /// </summary>
    public static SoundStyle SawAndDrill_2 => SoundID.Item23;

    /// <summary>
    /// 闪电靴之类
    /// </summary>
    public static SoundStyle RocketBoots2 => SoundID.Item24;

    /// <summary>
    /// 召唤坐骑 冰精灵等
    /// </summary>
    public static SoundStyle MagicSummon => SoundID.Item25;

    /// <summary>
    /// 竖琴
    /// </summary>
    public static SoundStyle Harb => SoundID.Item26;

    /// <summary>
    /// 破碎冰块
    /// </summary>
    public static SoundStyle IceBroken => SoundID.Item27;

    /// <summary>
    /// 彩虹法杖 冰杖等
    /// </summary>
    public static SoundStyle MagicStaff2 => SoundID.Item28;

    /// <summary>
    /// 魔法球等
    /// </summary>
    public static SoundStyle MagicSphere => SoundID.Item29;

    /// <summary>
    /// 镰刀
    /// </summary>
    public static SoundStyle Scythe => SoundID.Item71;
}

public static class DrawingMethods 
{
    public static void DrawLine(
    this SpriteBatch spriteBatch,
    Vector2 start,
    Vector2 end,
    Color color,
    float width = 4f,
    bool offset = false,
    Vector2 drawOffset = default)
    {
        if (offset)
        {
            end += start;
        }

        spriteBatch.Draw(
            TextureAssets.MagicPixel.Value,
            (start + end) * .5f + drawOffset,
            new Rectangle(0, 0, 1, 1),
            color,
            (end - start).ToRotation(),
            new Vector2(.5f, .5f),
            new Vector2((start - end).Length(), width),
            0, 0);
    }
    public static CustomVertexInfo[] CreateTriList(CustomVertexInfo[] source, Vector2 center, float scaler, bool addedCenter = false, bool createNormalGraph = false)
    {
        var length = source.Length;
        CustomVertexInfo[] triangleList = new CustomVertexInfo[3 * length - 6];
        for (int i = 0; i < length - 2; i += 2)
        {
            triangleList[3 * i] = source[i];
            triangleList[3 * i + 1] = source[i + 2];
            triangleList[3 * i + 2] = source[i + 1];

            triangleList[3 * i + 3] = source[i + 1];
            triangleList[3 * i + 4] = source[i + 2];
            triangleList[3 * i + 5] = source[i + 3];

            //if (createNormalGraph)
            //{
            //    for (int j = 0; j < 2; j++)
            //        for (int k = 0; k < 3; k++)
            //            triangleList[3 * i + k + 3 * j].Color = Vec2NormalColor(source[i + 2 + j].Position - source[i + j].Position);
            //}
        }
        for (int n = 0; n < triangleList.Length; n++)
        {
            var vertex = triangleList[n];
            if (addedCenter)
            {
                if (scaler != 1) vertex.Position = (vertex.Position - center) * scaler + center;
            }
            else
            {
                if (scaler != 1) vertex.Position *= scaler;
                vertex.Position += center;
            }
            triangleList[n] = vertex;
        }
        return triangleList;
    }
    public static CustomVertexInfo[] GetItemVertexes(Vector2 origin, float rotationStandard, float rotationOffset, float rotationDirection, Texture2D texture, float KValue, float size, Vector2 drawCen, bool flip, float alpha = 1f, Rectangle? frame = null)
    {
        Rectangle realFrame = frame ?? new Rectangle(0, 0, texture.Width, texture.Height);
        //对数据进行矩阵变换吧！
        Matrix matrix =
        Matrix.CreateTranslation(-origin.X, origin.Y - 1, 0) *          //把变换中心平移到传入的origin上，这里我应该是为了方便改成数学常用的坐标系下的origin了(?)
            Matrix.CreateScale(realFrame.Width, realFrame.Height, 1) *      //缩放到图片原本的正常比例
            Matrix.CreateRotationZ(rotationStandard) *                          //先进行一个旋转操作
            Matrix.CreateScale(1, flip ? 1 : -1, 1) *
            Matrix.CreateRotationZ(rotationOffset) *
            Matrix.CreateScale(1, 1 / KValue, 1) *                      //压扁来有一种横批的感觉(??)
            Matrix.CreateRotationZ(rotationDirection) *                       //朝向旋转量，我用这个的时候是这个固定，上面那个从小变大，形成一种纸片挥砍的动态感(x
            Matrix.CreateScale(size);                                   //单纯大小缩放
        Vector2[] vecs = new Vector2[4];
        for (int i = 0; i < 4; i++)
            vecs[i] = Vector2.Transform(new Vector2(i % 2, i / 2), matrix);//生成单位正方形四个顶点
        CustomVertexInfo[] c = new CustomVertexInfo[6];//两个三角形，六个顶点
        Vector2 startCoord = realFrame.TopLeft() / texture.Size();
        Vector2 endCoord = realFrame.BottomRight() / texture.Size();
        c[0] = new CustomVertexInfo(vecs[0] + drawCen, new Vector3(startCoord.X, endCoord.Y, alpha));
        c[1] = new CustomVertexInfo(vecs[1] + drawCen, new Vector3(endCoord, alpha));
        c[2] = new CustomVertexInfo(vecs[2] + drawCen, new Vector3(startCoord, alpha));
        c[3] = c[1];
        c[4] = new CustomVertexInfo(vecs[3] + drawCen, new Vector3(endCoord.X, startCoord.Y, alpha));
        //c[4] = new CustomVertexInfo(vecs[3] + drawCen, new Vector3(startCoord.Y,endCoord.X, alpha));

        c[5] = c[2];
        return c;
    }

    extension(SpriteBatch spriteBatch)
    {
        public unsafe void VertexDraw(
            Texture2D texture,
            Vector2 position0,
            Vector2 position1,
            Vector2 position2,
            Vector2 position3,

            Vector2 texcoord0,
            Vector2 texcoord1,
            Vector2 texcoord2,
            Vector2 texcoord3,

            Color color0,
            Color color1,
            Color color2,
            Color color3,

            float depth
        )
        {
            if (spriteBatch.numSprites >= spriteBatch.vertexInfo.Length)
            {
                if (spriteBatch.vertexInfo.Length >= MAX_ARRAYSIZE)
                    spriteBatch.FlushBatch();
                else
                {
                    int newMax = Math.Min(spriteBatch.vertexInfo.Length * 2, MAX_ARRAYSIZE);
                    Array.Resize(ref spriteBatch.vertexInfo, newMax);
                    Array.Resize(ref spriteBatch.textureInfo, newMax);
                    Array.Resize(ref spriteBatch.spriteInfos, newMax);
                    Array.Resize(ref spriteBatch.sortedSpriteInfos, newMax);
                }
            }

            if (spriteBatch.sortMode == SpriteSortMode.Immediate)
            {
                int offset;
                fixed (VertexPositionColorTexture4* sprite = &spriteBatch.vertexInfo[0])
                {
                    sprite->Position0 = new Vector3(position0, depth);
                    sprite->Position1 = new Vector3(position1, depth);
                    sprite->Position2 = new Vector3(position2, depth);
                    sprite->Position3 = new Vector3(position3, depth);

                    sprite->Color0 = color0;
                    sprite->Color1 = color1;
                    sprite->Color2 = color2;
                    sprite->Color3 = color3;

                    sprite->TextureCoordinate0 = texcoord0;
                    sprite->TextureCoordinate1 = texcoord1;
                    sprite->TextureCoordinate2 = texcoord2;
                    sprite->TextureCoordinate3 = texcoord3;

                    if (spriteBatch.supportsNoOverwrite)
                        offset = spriteBatch.UpdateVertexBuffer(0, 1);
                    else
                    {
                        offset = 0;
                        spriteBatch.vertexBuffer.SetDataPointerEXT(
                            0,
                            (nint)sprite,
                            VertexPositionColorTexture4.RealStride,
                            SetDataOptions.None
                        );
                    }
                }
                spriteBatch.DrawPrimitives(texture, offset, 1);
            }
            else if (spriteBatch.sortMode == SpriteSortMode.Deferred)
            {
                fixed (VertexPositionColorTexture4* sprite = &spriteBatch.vertexInfo[spriteBatch.numSprites])
                {
                    sprite->Position0 = new Vector3(position0, depth);
                    sprite->Position1 = new Vector3(position1, depth);
                    sprite->Position2 = new Vector3(position2, depth);
                    sprite->Position3 = new Vector3(position3, depth);

                    sprite->Color0 = color0;
                    sprite->Color1 = color1;
                    sprite->Color2 = color2;
                    sprite->Color3 = color3;

                    sprite->TextureCoordinate0 = texcoord0;
                    sprite->TextureCoordinate1 = texcoord1;
                    sprite->TextureCoordinate2 = texcoord2;
                    sprite->TextureCoordinate3 = texcoord3;
                }

                spriteBatch.textureInfo[spriteBatch.numSprites] = texture;
                spriteBatch.numSprites += 1;
            }
            else
            {
                throw new NotImplementedException($"Mode:{SpriteSortMode.Texture}, {SpriteSortMode.FrontToBack}, {SpriteSortMode.BackToFront} Not Support.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="texture">要绘制的贴图</param>
        /// <param name="position">绘制锚点，绘制帧上origin点所在处</param>
        /// <param name="frame">从贴图上裁切下来的绘制帧</param>
        /// <param name="color">给贴图染的颜色，默认是乘算</param>
        /// <param name="rotationStandard">贴图的标准朝向，会以这个为轴进行翻转</param>
        /// <param name="rotationOffset">缩放前旋转量</param>
        /// <param name="rotationDirection">x轴正方向朝向</param>
        /// <param name="origin">绘制帧锚点</param>
        /// <param name="scaler">缩放系数</param>
        /// <param name="flip">是否翻转</param>
        public void Draw(
            Texture2D texture,
            Vector2 position,
            Rectangle? frame,
            Color color,
            float rotationStandard,
            float rotationOffset,
            float rotationDirection,
            Vector2 origin,
            Vector2 scaler,
            bool flip)
        {
            Rectangle realFrame = frame ?? new Rectangle(0, 0, texture.Width, texture.Height);
            origin /= realFrame.Size();
            Vector2[] vecs = new Vector2[4];
            for (int i = 0; i < 4; i++)
            {
                var v = new Vector2(i % 2, i / 2);
                v += new Vector2(-origin.X, origin.Y - 1);      // 把绘制锚点平移绘制帧锚点的位置
                v *= realFrame.Size();                          // 变换到帧大小
                v = v.RotatedBy(-rotationStandard);             // 使得x正方向朝向指定标准方向
                if (flip)
                    v.Y *= -1;                                  // 以标准方向为轴翻转
                v = v.RotatedBy(rotationOffset);                // 旋转偏移量
                v *= scaler;                                    // 缩放
                v = v.RotatedBy(rotationDirection);             // 椭圆朝向旋转量
                vecs[i] = v;
            }

            Vector2 startCoord = realFrame.TopLeft() / texture.Size();
            Vector2 endCoord = realFrame.BottomRight() / texture.Size();

            if (flip)
                spriteBatch.VertexDraw(texture, vecs[0] + position, vecs[2] + position, vecs[1] + position, vecs[3] + position,
                     new Vector2(startCoord.X, endCoord.Y), startCoord, endCoord, new(endCoord.X, startCoord.Y),
                    color, color, color, color, 0
                    );
            else
                spriteBatch.VertexDraw(texture, vecs[0] + position, vecs[1] + position, vecs[2] + position, vecs[3] + position,
                    new Vector2(startCoord.X, endCoord.Y), endCoord, startCoord, new(endCoord.X, startCoord.Y),
                    color, color, color, color, 0
                    );
        }

    }
}
public static class MiscMethods 
{
    public static Dust FastDust(Vector2 Center, Vector2 velocity, Color color, float scaler, int? shaderID = null)
    {
        var hsl = Main.rgbToHsl(color);//Color.MediumPurple
        var dustColor = Color.Lerp(Main.hslToRgb(Vector3.Clamp(hsl * new Vector3(1, 2, Main.rand.NextFloat(0.85f, 1.15f)), default, Vector3.One)), Color.White, Main.rand.NextFloat(0, 0.3f));
        Dust dust = Dust.NewDustPerfect(Center, 278, velocity, 0, dustColor, 1f);
        dust.scale = 0.4f + Main.rand.NextFloat(-1, 1) * 0.1f;
        dust.scale *= Main.rand.NextFloat(1, 2f) * scaler;
        dust.fadeIn = 0.4f + Main.rand.NextFloat() * 0.3f;
        dust.fadeIn *= .5f;
        dust.noGravity = true;
        if (shaderID is > 0)
            dust.shader = GameShaders.Armor._shaderData[shaderID.Value - 1];
        return dust;
    }

    public static Dust FastDust(Vector2 Center, Vector2 velocity, Color color) => FastDust(Center, velocity, color, 1f);
}