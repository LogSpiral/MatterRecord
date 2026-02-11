using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.TheoryOfFreedom;

public class TheTheoryOfFreedomPlayer : ModPlayer
{
    public bool EquippedTOF = false;
    public bool CanHookPlatform;

    public override void SaveData(TagCompound tag)
    {
        tag.Add(nameof(CanHookPlatform), CanHookPlatform);
        base.SaveData(tag);
    }

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet(nameof(CanHookPlatform), out bool value))
            CanHookPlatform = value;
        base.LoadData(tag);
    }

    public override void ResetEffects()
    {
        EquippedTOF = false;
        base.ResetEffects();
    }

    public override void PreUpdate()
    {
        flyTimeCache = Player.wingTime;
        base.PreUpdate();
    }

    public float flyTimeCache;
    public List<Point> TargetTileCoords { get; } = [];
    private static ModKeybind CanHookPlatformSwitch { get; set; }

    public override void Load()
    {
        CanHookPlatformSwitch = KeybindLoader.RegisterKeybind(Mod, "CanHookPlatform", Keys.L);
        base.Load();
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        if (CanHookPlatformSwitch.JustPressed)
        {
            CanHookPlatform = !CanHookPlatform;
            Main.NewText(Language.GetTextValue($"Mods.{nameof(MatterRecord)}.Items.{nameof(TheoryOfFreedom)}.{(CanHookPlatform ? "CanHookOnPlatform" : "CantHookOnPlatform")}"), CanHookPlatform ? Color.Lime : Color.Green);
            if (Main.netMode == NetmodeID.MultiplayerClient)
                SyncPlayer(-1, Player.whoAmI, false);
        }
        base.ProcessTriggers(triggersSet);
    }

    public override void CopyClientState(ModPlayer targetCopy)
    {
        TheTheoryOfFreedomPlayer clone = (TheTheoryOfFreedomPlayer)targetCopy;
        clone.CanHookPlatform = CanHookPlatform;
    }

    public override void SendClientChanges(ModPlayer clientPlayer)
    {
        TheTheoryOfFreedomPlayer clone = (TheTheoryOfFreedomPlayer)clientPlayer;

        if (CanHookPlatform != clone.CanHookPlatform)
            SyncPlayer(toWho: -1, fromWho: Main.myPlayer, newPlayer: false);
    }

    public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
    {
        TheoryOfFreedomHookPlatformAbilitySync.Get(Player.whoAmI, CanHookPlatform).Send(toWho, fromWho);
    }
}
