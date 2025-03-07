using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader.IO;
using System.Reflection;
using MonoMod.Cil;
using Stubble.Core.Exceptions;
using Terraria.ID;
using Terraria.Audio;
using Terraria.Net;
using Microsoft.Xna.Framework;
using System.Drawing;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public class DreamWorld : ModSystem
{
    public static bool UsedZoologistDream;
    public static List<int> availableDyeId = [];
    public override void PostSetupContent()
    {


        availableDyeId.Clear();
        Dictionary<int, int> dict = (Dictionary<int, int>)typeof(ArmorShaderDataSet).GetField("_shaderLookupDictionary", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(GameShaders.Armor);
        foreach (var key in dict.Keys)
            availableDyeId.Add(key);
        base.PostSetupContent();
    }
    public override void Load()
    {
        MonoModHooks.Modify(typeof(NPCShopDatabase).GetMethod("RegisterSkeletonMerchant", BindingFlags.Static | BindingFlags.NonPublic), ilContext =>
        {
            var cursor = new ILCursor(ilContext);
            if (!cursor.TryGotoNext(i => i.MatchCallvirt(typeof(AbstractNPCShop).GetMethod("Register", BindingFlags.Instance | BindingFlags.Public))))
                return;
            var conditionFld = typeof(NPCShop.Entry).GetField("conditions", BindingFlags.Instance | BindingFlags.NonPublic);
            cursor.EmitDelegate<Func<NPCShop, NPCShop>>(shop =>
            {
                foreach (var entry in shop.Entries)
                {
                    var list = conditionFld.GetValue(entry) as List<Condition>;
                    var dummy = new List<Condition>();
                    foreach (var condition in list)
                        dummy.Add(condition.Description.Key == "Conditions.InHardmode" ? condition : new Condition(condition.Description.Key, () => condition.IsMet() || GlobalHasCheck(DreamState.SkeletonMerchant)));

                    list.Clear();
                    foreach (var condition in dummy)
                        list.Add(condition);
                }
                return shop;
            });
        });



        IL_Player.SetTalkNPC += PrincessModify;

        IL_Main.GUIChatDrawInner += NurseModify;

        On_Player.GetItemExpectedPrice += MerchantModify;

        IL_Wiring.MassWireOperationStep += MachanicModify;

        On_Player.GetWingStats += SteamPunkerModify;

        IL_Main.GUIChatDrawInner += AnglerModify;

        On_Player.ApplyCoating += PainterModify;


        On_Main.UpdateTime_SpawnTownNPCs += PartyGirlModify;

        On_WorldGen.TrySpawningTownNPC += PartyGirlModify2;

        MonoModHooks.Modify(typeof(PrefixLoader).GetMethod(nameof(PrefixLoader.Roll), BindingFlags.Static | BindingFlags.Public), ilcontext =>
        {
            var cursor = new ILCursor(ilcontext);
            if (!cursor.TryGotoNext(i => i.MatchLdcR8(out _)))
                return;
            cursor.Remove();
            cursor.EmitLdloc(7);
            cursor.EmitLdarg0();
            cursor.EmitDelegate(GetWeightFromPrefix);

            if (!cursor.TryGotoNext(i => i.MatchLdcR8(out _)))
                return;
            cursor.Remove();
            cursor.EmitLdcI4(84);
            cursor.EmitLdarg0();
            cursor.EmitDelegate(GetWeightFromPrefix);
        });
        base.Load();
    }
    private static double GetWeightFromPrefix(int prefix, Item targetItem)
    {
        if (!Main.LocalPlayer.CheckDreamActive(DreamState.GoblinTinkerer))
            return 1;
        float dmg = 1f;
        float kb = 1f;
        float spd = 1f;
        float size = 1f;
        float shtspd = 1f;
        float mcst = 1f;
        int crt = 0;
        object[] args = [ prefix, dmg, kb, spd, size, shtspd, mcst, crt ];
        var method = typeof(Item).GetMethod("TryGetPrefixStatMultipliersForItem", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(targetItem, args);
        dmg = (float)args[1];
        kb = (float)args[2];
        spd = (float)args[3];
        size = (float)args[4];
        shtspd = (float)args[5];
        mcst = (float)args[6];
        crt = (int)args[7];

        float num = 1f * dmg * (2f - spd) * (2f - mcst) * size * kb * shtspd * (1f + (float)crt * 0.02f);


        if (prefix == 62 || prefix == 69 || prefix == 73 || prefix == 77)
            num *= 1.05f;

        if (prefix == 63 || prefix == 70 || prefix == 74 || prefix == 78 || prefix == 67)
            num *= 1.1f;

        if (prefix == 64 || prefix == 71 || prefix == 75 || prefix == 79 || prefix == 66)
            num *= 1.15f;

        if (prefix == 65 || prefix == 72 || prefix == 76 || prefix == 80 || prefix == 68)
            num *= 1.2f;

        num *= num;

        return Math.Pow(num,2.0);
    }
    private void PartyGirlModify2(On_WorldGen.orig_TrySpawningTownNPC orig, int x, int y)
    {
        orig.Invoke(x, y);
        if (GlobalHasCheck(DreamState.PartyGirl))
            for (int n = 0; n < 3; n++)
                orig.Invoke(Main.rand.Next(10, Main.maxTilesX - 10),
                    Main.rand.Next((int)Main.worldSurface - 1, Main.maxTilesY - 20));

    }

    private void PartyGirlModify(On_Main.orig_UpdateTime_SpawnTownNPCs orig)
    {
        if (GlobalHasCheck(DreamState.PartyGirl) && Main.netMode != NetmodeID.MultiplayerClient && WorldGen.GetWorldUpdateRate() > 0)
            Main.checkForSpawns += 3;
        orig.Invoke();
    }

    private void PainterModify(On_Player.orig_ApplyCoating orig, Player self, int x, int y, bool paintingAWall, bool applyItemAnimation, Item targetItem)
    {
        byte paintCoating = targetItem.paintCoating;
        if (paintingAWall)
        {
            if (WorldGen.paintCoatWall(x, y, paintCoating, broadcast: true))
            {
                if (!self.CheckDreamActive(DreamState.Painter))
                {
                    targetItem.stack--;
                    if (targetItem.stack <= 0)
                        targetItem.SetDefaults();
                }


                if (applyItemAnimation)
                    self.ApplyItemTime(self.inventory[self.selectedItem], self.wallSpeed);
            }
        }
        else if (WorldGen.paintCoatTile(x, y, paintCoating, broadcast: true))
        {
            if (!self.CheckDreamActive(DreamState.Painter))
            {
                targetItem.stack--;
                if (targetItem.stack <= 0)
                    targetItem.SetDefaults();
            }

            if (applyItemAnimation)
                self.ApplyItemTime(self.inventory[self.selectedItem], self.tileSpeed);
        }
    }

    private void AnglerModify(ILContext il)
    {
        var cursor = new ILCursor(il);

        var soundMethod = typeof(SoundEngine).GetMethod(nameof(SoundEngine.PlaySound), BindingFlags.Static | BindingFlags.Public, [typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(float)]);
        for (int n = 0; n < 3; n++)
            if (!cursor.TryGotoNext(i => i.MatchCall(soundMethod)))
                return;

        cursor.Index += 2;
        cursor.EmitDelegate(() =>
        {
            Main.LocalPlayer.GetAnglerReward(Main.npc[Main.LocalPlayer.talkNPC], Main.anglerQuestItemNetIDs[Main.anglerQuest]);
        });
    }

    private Terraria.DataStructures.WingStats SteamPunkerModify(On_Player.orig_GetWingStats orig, Player self, int wingID)
    {
        var state = orig.Invoke(self, wingID);
        if (self.CheckDreamActive(DreamState.Steampunker))
            state.FlyTime += wingID == 4 ? 300 : 120;
        return state;
    }

    private void MachanicModify(ILContext il)
    {
        var cursor = new ILCursor(il);
        static int mechModify(Player plr) => plr.CheckDreamActive(DreamState.Mechanic) ? 0 : 1;

        for (int n = 0; n < 3; n++)
            if (!cursor.TryGotoNext(i => i.MatchRet()))
                return;

        for (int n = 0; n < 4; n++)
        {
            if (!cursor.TryGotoNext(i => i.MatchRet()))
                return;
            if (!cursor.TryGotoNext(i => i.MatchLdcI4(1)))
                return;
            cursor.Remove();
            cursor.EmitLdarg0();
            cursor.EmitDelegate(mechModify);
        }
    }

    private void MerchantModify(On_Player.orig_GetItemExpectedPrice orig, Player self, Item item, out long calcForSelling, out long calcForBuying)
    {
        orig.Invoke(self, item, out calcForSelling, out calcForBuying);
        if (self.CheckDreamActive(DreamState.Merchant))
            calcForSelling = calcForSelling * 13 / 10;
    }

    private void PrincessModify(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchLdcR8(out _)))
            return;

        cursor.Index -= 3;

        cursor.EmitLdarg0();
        cursor.EmitDelegate<Action<Player>>(plr => plr.currentShoppingSettings.PriceAdjustment *= plr.CheckDreamActive(DreamState.Princess) ? 0.9 : 1.0);
    }

    private void NurseModify(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (!cursor.TryGotoNext(i => i.MatchLdsfld(typeof(NPC).GetField(nameof(NPC.downedGolemBoss), BindingFlags.Static | BindingFlags.Public))))
            return;

        for (int n = 0; n < 9; n++)
            if (!cursor.TryGotoNext(i => i.MatchStloc(13)))
                return;

        cursor.EmitDelegate<Func<int, int>>(value => (int)(value * (Main.LocalPlayer.CheckDreamActive(DreamState.Nurse) ? .8f : 1f)));
    }

    static bool GlobalHasCheck(DreamState flag)
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            return Main.LocalPlayer.CheckDreamActive(flag);
        else
        {
            foreach (var plr in Main.player)
            {
                if (plr.active && plr.CheckDreamActive(flag))
                    return true;
            }
        }
        return false;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        UsedZoologistDream = tag.GetBool(nameof(UsedZoologistDream));
        base.LoadWorldData(tag);
    }
    public override void SaveWorldData(TagCompound tag)
    {
        tag[nameof(UsedZoologistDream)] = UsedZoologistDream;
        base.SaveWorldData(tag);
    }
}

public class DreamItemConsumption : GlobalItem
{
    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.type == ItemID.Wire && player.CheckDreamActive(DreamState.Mechanic))
            return false;
        if (item.paint > 0 && player.CheckDreamActive(DreamState.Painter))
            return false;
        return base.ConsumeItem(item, player);
    }

    public override bool CanUseItem(Item item, Player player)
    {
        if (item.type == ItemID.Mushroom && player.CheckDreamActive(DreamState.Truffle))
            return false;
        return base.CanUseItem(item, player);
    }
}
