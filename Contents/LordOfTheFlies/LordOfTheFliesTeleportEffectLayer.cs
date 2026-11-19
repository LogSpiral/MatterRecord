using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;

namespace MatterRecord.Contents.LordOfTheFlies;

public class LordOfTheFliesTeleportEffectLayer : PlayerDrawLayer
{
    public override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0) return;
        var player = drawInfo.drawPlayer;
        var modPlayer = player.GetModPlayer<LordOfTheFliesTeleportEffectPlayer>();
        if (!modPlayer.TeleportEffectEnabled) return;

        if (modPlayer.TeleportEffectMaskFlag) return;

        var list = drawInfo.DrawDataCache;

        float factor = modPlayer.TeleportFactor;
        float rfactor = 1 - factor;

        bool flip = player.position.X > modPlayer.TeleportStartPosition.X;
        SpriteEffects effect = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        float sign = flip ? 1 : -1;

        list.Add(new DrawData(TextureAssets.Extra[ExtrasID.VortexBlack].Value,
                        player.Center - Main.screenPosition,
                        null,
                        Color.White * rfactor,
                        Main.GlobalTimeWrappedHourly * 16 * sign,
                        new Vector2(30),
                        1 + factor * factor * factor * 8,
                        effect,
                        0));

        list.Add(new DrawData(TextureAssets.Projectile[ProjectileID.VortexVortexLightning].Value,
                                player.Center - Main.screenPosition,
                                null,
                                Color.White with { A = 0 } * .5f * rfactor,
                                Main.GlobalTimeWrappedHourly * 4 * sign,
                                new Vector2(30),
                                1 + factor,
                                effect,
                                0));

        list.Add(new DrawData(TextureAssets.Projectile[ProjectileID.VortexVortexLightning].Value,
                        player.Center - Main.screenPosition,
                        null,
                        Color.White with { A = 0 } * .3f * rfactor,
                        Main.GlobalTimeWrappedHourly * -8 * sign,
                        new Vector2(30),
                        2f + factor * 3f,
                        effect,
                        0));
    }

    public override Position GetDefaultPosition() => PlayerDrawLayers.AfterLastVanillaLayer;
}

public class LordOfTheFliesTeleportEffectPlayer : ModPlayer
{
    public const int TeleportStyleID = 100;

    public static int TeleportTimeMax => 24;

    public int TeleportEffectTimer { get; private set; }

    public bool TeleportEffectEnabled { get; private set; }

    public Vector2 TeleportStartPosition { get; private set; }

    public float TeleportFactor => 1 - TeleportEffectTimer / (float)TeleportTimeMax;

    public override void Load()
    {
        On_Player.Teleport += On_Player_Teleport;
    }

    private void On_Player_Teleport(On_Player.orig_Teleport orig, Player self, Vector2 newPos, int Style, int extraInfo)
    {
        if (Style == TeleportStyleID)
            self.GetModPlayer<LordOfTheFliesTeleportEffectPlayer>().StartTeleportEffect();
        orig?.Invoke(self, newPos, Style, extraInfo);
    }

    public void StartTeleportEffect()
    {
        TeleportEffectTimer = TeleportTimeMax;
        TeleportEffectEnabled = true;
        TeleportStartPosition = Player.position;
    }

    public bool TeleportEffectMaskFlag { get; private set; }

    public override void DrawPlayer(Camera camera)
    {
        // if (!TeleportEffectEnabled) return;
        TeleportEffectMaskFlag = true;


        float factor = TeleportFactor;
        float rfactor = 1 - factor;
        Main.PlayerRenderer.DrawPlayer(camera, Player, TeleportStartPosition + new Vector2(Main.rand.NextFloat(-16, 32) * rfactor + factor * 64, 0), Player.fullRotation, Player.fullRotationOrigin, factor * .5f + .5f, 1);

        Main.PlayerRenderer.DrawPlayer(camera, Player, TeleportStartPosition - new Vector2(Main.rand.NextFloat(-16, 32) * rfactor + factor * 64, 0), Player.fullRotation, Player.fullRotationOrigin, factor * .5f + .5f, 1);


        TeleportEffectMaskFlag = false;
    }

    public override void PreUpdate()
    {
        if (!TeleportEffectEnabled) return;
        TeleportEffectTimer--;
        TeleportEffectEnabled = TeleportEffectTimer > 0;
    }
}