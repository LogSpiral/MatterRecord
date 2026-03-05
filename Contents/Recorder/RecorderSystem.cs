using MatterRecord.Contents.AliceInWonderland;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;
using Terraria.UI.Chat;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace MatterRecord.Contents.Recorder;

public class RecorderSystem : ModSystem
{
    private Bits64 _itemLockRecords;
    public static void ClearRecord() => Instance?._itemLockRecords = default;
    public static bool CheckUnlock(ItemRecords record) => Instance._itemLockRecords[(int)record];
    public static bool CheckUnlock(IRecordBookItem recordBookItem) => CheckUnlock(recordBookItem.RecordType);
    public static void SetUnlock(ItemRecords record) => Instance?._itemLockRecords[(int)record] = true;
    public static void SetUnlock(IRecordBookItem recordBookItem) => SetUnlock(recordBookItem.RecordType);
    public override void ClearWorld()
    {
        _itemLockRecords = default;
    }
    public override void LoadWorldData(TagCompound tag)
    {
        if (tag.TryGet<ulong>("LR", out var records))
            _itemLockRecords = records;
        else
            _itemLockRecords = default;

        _itemLockRecords[(int)ItemRecords.Faust] = true;
        base.LoadWorldData(tag);
    }
    public override void SaveWorldData(TagCompound tag)
    {
        tag.Add("LR", (ulong)_itemLockRecords);
        base.SaveWorldData(tag);
    }

    public static RecorderSystem Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }


    public static bool ShouldSpawnRecordItem<T>() where T : ModItem
    {
        int recordType = ModContent.ItemType<T>();
        if (Main.dedServ)
        {
            foreach (var player in Main.player)
            {
                if (!player.active) continue;
                Item[][] inventories =
                [
                    player.inventory,
                    player.armor,
                    player.dye,
                    player.miscEquips,
                    player.miscDyes,
                    [player.trashItem],
                    player.bank.item,
                    player.bank2.item,
                    player.bank3.item,
                    player.bank4.item
                ];
                foreach (var inventory in inventories)
                    foreach (var item in inventory)
                    {
                        if (item.type == recordType)
                            return false;
                    }
            }
        }
        else
        {
            var player = Main.LocalPlayer;
            Item[][] inventories =
            [
                player.inventory,
                player.armor,
                player.dye,
                player.miscEquips,
                player.miscDyes,
                [player.trashItem],
                player.bank.item,
                player.bank2.item,
                player.bank3.item,
                player.bank4.item
            ];
            foreach (var inventory in inventories)
                foreach (var item in inventory)
                {
                    if (item.type == recordType)
                        return false;
                }
        }
        return true;
    }
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        int GuideIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Guide"));

        if (GuideIndex != -1)
        {
            tasks.Insert(GuideIndex + 1, new RecorderSpawnPass());
        }
    }
    public static void StartUnlockAnimation(int itemType)
    {
        AnimationItemType = itemType;
        AnimationActive = true;
        AnimationTimer = 0;
    }
    private static int AnimationItemType;
    private static bool AnimationActive;
    private static int AnimationTimer;
    private const int AnimationTimerMax = 300;
    private static readonly RecordUnlockAnimationEffect AnimationEffect = new();
    public static Action OnEndAnimation { get; set; }
    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Add(new LegacyGameInterfaceLayer("MatterRecord: RewardAnimation", delegate
        {
            // 半秒淡入，出现书本
            // 二秒蓄力抽搐
            // 半秒转换
            // 半秒停歇
            // 一秒字幕
            // 半秒淡出
            if (!AnimationActive) return false;

            AnimationTimer++;
            if (AnimationTimer > AnimationTimerMax)
            {
                AnimationActive = false;
                OnEndAnimation?.Invoke();
                return false;
            }
            var spb = Main.spriteBatch;
            var center = new Vector2(Main.screenWidth, Main.screenHeight) * .5f;
            AnimationEffect.Update();
            const float backLight = 0.5f;
            switch (AnimationTimer)
            {
                case <= 30:
                    float timer = MathHelper.SmoothStep(0, 1, AnimationTimer / 30f);
                    spb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * (timer * backLight));
                    AnimationEffect.Draw(spb);
                    spb.Draw(
                        TextureAssets.Item[ItemID.SpellTome].Value,
                        center,
                        null,
                        Color.White * timer,
                        0,
                        TextureAssets.Item[ItemID.SpellTome].Size() * .5f,
                        4f,
                        0,
                        0);
                    break;
                case <= 150:
                    timer = MathHelper.SmoothStep(0, 1, (AnimationTimer - 30f) / 120f);
                    spb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * backLight);
                    AnimationEffect.Draw(spb);
                    spb.Draw(
                        TextureAssets.Item[ItemID.SpellTome].Value,
                        center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, timer * 32),
                        null,
                        Color.White,
                        0,
                        TextureAssets.Item[ItemID.SpellTome].Size() * .5f,
                        4f,
                        0,
                        0);
                    if (AnimationTimer < 135)
                        SoundEngine.PlaySound(SoundID.Item45 with { MaxInstances = -1, Volume = timer * 2, Pitch = timer }, null);
                    if (AnimationTimer == 150)
                    {
                        SoundEngine.PlaySound(SoundID.Research with { pitch = -0.5f, Volume = 1.5f });
                        SoundEngine.PlaySound(SoundID.ResearchComplete);
                        SoundEngine.PlaySound(SoundID.Item38, null);
                        ParticleOrchestraSettings settings = new()
                        {
                            IndexOfPlayerWhoInvokedThis = (byte)Main.myPlayer
                        };
                        for (int n = 0; n < 10; n++)
                        {
                            settings.PositionInWorld = Main.LocalPlayer.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, Main.rand.NextFloat(0, 256)) + new Vector2(0, 256);
                            settings.MovementVector = new Vector2(0, Main.rand.NextFloat(-64, -8));
                            ParticleOrchestrator.Spawn_PrincessWeapon(settings);
                        }
                        AnimationEffect.Initialize();
                    }
                    break;
                case <= 270:
                    timer = MathHelper.SmoothStep(1, 0, (AnimationTimer - 150f) / 30f);
                    timer *= timer;
                    spb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * backLight);
                    AnimationEffect.Draw(spb);
                    Texture2D itemTexture = TextureAssets.Item[AnimationItemType].Value;
                    spb.Draw(
                        itemTexture,
                        center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, timer * 32),
                        null,
                        Color.White,
                        0,
                        itemTexture.Size() * .5f,
                        4f,
                        0,
                        0);
                    Color light = Color.White with { A = 0 } * timer;//;* ((1 - MathF.Cos(timer * MathHelper.TwoPi)) * .5f);
                    spb.Draw(
                        itemTexture,
                        center,
                        null,
                        light,
                        0,
                        itemTexture.Size() * .5f,
                        4f * (2 - timer),
                        0,
                        0);
                    Texture2D sharpTear = TextureAssets.Extra[ExtrasID.SharpTears].Value;
                    spb.Draw(
                        sharpTear,
                        center,
                        null,
                        light * .25f,
                        Main.GlobalTimeWrappedHourly,
                        sharpTear.Size() * .5f,
                        new Vector2(2, 8),
                        0,
                        0
                        );
                    spb.Draw(
                        sharpTear,
                        center,
                        null,
                        light * .125f,
                        -2 * Main.GlobalTimeWrappedHourly,
                        sharpTear.Size() * .5f,
                        new Vector2(2, 8) * 1.5f,
                        0,
                        0
                        );
                    string text = $"已解锁 {ContentSamples.ItemsByType[AnimationItemType].Name}";
                    float timerText = MathHelper.SmoothStep(0, 1, (AnimationTimer - 210f) / 30f);
                    ChatManager.DrawColorCodedString(
                        spb,
                        FontAssets.DeathText.Value,
                        text,
                        center + new Vector2(0, 240),
                        Color.White * timerText,
                        0,
                        FontAssets.DeathText.Value.MeasureString(text) * .5f, Vector2.One);
                    if (AnimationTimer > 210)
                    {
                        float sfxTimer = MathHelper.SmoothStep(0, 1, (AnimationTimer - 210f) / 60f);
                        SoundEngine.PlaySound(SoundID.Item45 with { MaxInstances = -1, Pitch = -sfxTimer, Volume = 1 - sfxTimer });
                    }
                    break;
                default:
                    timer = MathHelper.SmoothStep(1, 0, (AnimationTimer - 270f) / 30f);
                    spb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * (backLight * timer));
                    AnimationEffect.Draw(spb);
                    spb.Draw(
                        TextureAssets.Item[AnimationItemType].Value,
                        center,
                        null,
                        Color.White * timer,
                        0,
                        TextureAssets.Item[AnimationItemType].Size() * .5f,
                        4f,
                        0,
                        0);

                    text = $"已解锁 {ContentSamples.ItemsByType[AnimationItemType].Name}";
                    timerText = MathHelper.SmoothStep(0, 1, (AnimationTimer - 210f) / 30f);
                    ChatManager.DrawColorCodedString(
                        spb,
                        FontAssets.DeathText.Value,
                        text,
                        center + new Vector2(0, 240),
                        Color.White * timer,
                        0,
                        FontAssets.DeathText.Value.MeasureString(text) * .5f, Vector2.One);
                    break;
            }
            return true;
        }, InterfaceScaleType.Game));
    }

}
public class RecorderSpawnPass : GenPass
{
    public RecorderSpawnPass() : base("Recorder", 1)
    {
    }

    public override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
    {
        progress.Set(1.0);

        progress.Message = Language.GetTextValue("Mods.MatterRecord.NPCs.Recorder.Spawn");

        int num297 = NPC.NewNPC(new EntitySource_WorldGen(), Main.spawnTileX * 16, Main.spawnTileY * 16, ModContent.NPCType<Recorder>());
        Main.npc[num297].homeTileX = Main.spawnTileX;
        Main.npc[num297].homeTileY = Main.spawnTileY;
        Main.npc[num297].direction = 1;
        Main.npc[num297].homeless = true;
    }
}

public class RecordUnlockPill : ModItem
{
    public override void SetDefaults()
    {
        Item.useTime = Item.useAnimation = 15;
        Item.rare = ItemRarityID.Master;
        Item.useStyle = ItemUseStyleID.DrinkLiquid;
        base.SetDefaults();
    }
    public override bool? UseItem(Player player)
    {
        RecorderSystem.StartUnlockAnimation(ItemID.Boulder);
        return null;
    }
    public override string Texture => $"Terraria/Images/Item_{ItemID.LihzahrdPowerCell}";

    public override Color? GetAlpha(Color lightColor) => Main.OurFavoriteColor;
}

public class RecordUnlockAnimationEffect
{
    private class DustGlow
    {
        public Vector2 Position { get; private set; }
        public Vector2 Velocity { get; private set; }
        public float Scale { get; private set; }

        private float Rotation { get; set; }

        public void Initialize(
            Vector2 position,
            Vector2 velocity,
            float scale
            )
        {
            Position = position;
            Velocity = velocity;
            Scale = scale;
        }

        public void InitializeByRandom()
        {
            Position = new Vector2(Main.screenWidth, Main.screenHeight) * .5f;
            Velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(0, 64);
            Scale = Main.rand.NextFloat(2);
            Rotation = Velocity.ToRotation();
        }

        public void Update()
        {
            Position += Velocity;
            Velocity *= .85f;
            Scale *= .9f;
        }

        public void Draw(SpriteBatch spb)
        {
            spb.Draw(ModAsset.DustGlow2.Value, Position, null, Color.White with { A = 0 }, Rotation, ModAsset.DustGlow.Size() * .5f, new Vector2(Velocity.Length() / 32f + 1f, 1 - Velocity.Length() / 64f) * Scale, 0, 0);
        }
    }

    private List<DustGlow> Dusts { get; } = [];

    private bool _activate;

    public void Initialize()
    {
        Dusts.Clear();
        int max = Main.rand.Next(30, 50);
        for (int n = 0; n < max; n++)
        {
            var dust = new DustGlow();
            dust.InitializeByRandom();
            Dusts.Add(dust);
        }
        _activate = true;
    }

    public void Update()
    {
        if (!_activate) return;
        HashSet<DustGlow> remove = [];
        foreach (var dust in Dusts)
        {
            dust.Update();
            if (dust.Scale < 0.01f)
                remove.Add(dust);
        }
        foreach (var rem in remove)
            Dusts.Remove(rem);
        if (Dusts.Count == 0)
            _activate = false;
    }

    public void Draw(SpriteBatch spb)
    {
        if (!_activate) return;
        foreach (var dust in Dusts)
            dust.Draw(spb);
    }
}