using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria.Localization;
using Terraria.ObjectData;

namespace MatterRecord.Contents.LittlePrince;

public class LittlePrincePlayer : ModPlayer
{
    public bool EquippedRose;

    public override void ResetEffects()
    {
        EquippedRose = false;
        base.ResetEffects();
    }

    public override void Load()
    {
        On_Player.DropCoins += PrinceBanDropCoins;
        On_Player.DropTombstone += PrinceBanDropTombstone;
        base.Load();
    }
    private static void PrinceBanDropTombstone(On_Player.orig_DropTombstone orig, Player self, long coinsOwned, Terraria.Localization.NetworkText deathText, int hitDirection)
    {
        if (self.GetModPlayer<LittlePrincePlayer>().EquippedRose) return;
        orig(self, coinsOwned, deathText, hitDirection);
    }

    public override void UpdateEquips()
    {
        // Player.buffImmune[BuffID.ManaSickness] = EquippedRose;

        base.UpdateEquips();
    }

    private static long PrinceBanDropCoins(On_Player.orig_DropCoins orig, Player self)
    {
        if (self.GetModPlayer<LittlePrincePlayer>().EquippedRose)
        {
            self.lostCoins = 0L;
            self.lostCoinString = "";
            return 0L;
        }
        return orig(self);
    }

    public override void Unload()
    {
        On_Player.DropCoins -= PrinceBanDropCoins;
        On_Player.DropTombstone -= PrinceBanDropTombstone;
        base.Unload();
    }
}

public class LittlePrince : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.LittlePrince;
    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.AbigailsFlower);
        base.AddRecipes();
    }
    //public override string Texture => $"Terraria/Images/Item_{ItemID.JungleRose}";
    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 27;
        Item.rare = ItemRarityID.Yellow;
        Item.value = Item.sellPrice(0, 1);
        Item.accessory = true;
        base.SetDefaults();
    }

    public override void UpdateEquip(Player player)
    {
        player.GetModPlayer<LittlePrincePlayer>().EquippedRose = true;
        base.UpdateEquip(player);
    }
}

public class LittlePrinceRose : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileObsidianKill[Type] = true;
        //Main.tileCut[Type] = true;
        Main.tileNoFail[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;
        TileID.Sets.IgnoredInHouseScore[Type] = true;
        TileID.Sets.IgnoredByGrowingSaplings[Type] = true;

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(128, 128, 128), name);

        TileObjectData.newTile.CopyFrom(TileObjectData.StyleAlch);
        TileObjectData.newTile.AnchorValidTiles = [
            TileID.Grass,
            TileID.HallowedGrass,
            TileID.CorruptGrass,
            TileID.AshGrass,
            TileID.CrimsonGrass,
            TileID.MushroomGrass,
            TileID.GolfGrass,
            TileID.GolfGrassHallowed
        ];
        TileObjectData.newTile.AnchorAlternateTiles = [
            TileID.ClayPot,
            TileID.PlanterBox
        ];
        TileObjectData.addTile(Type);

        HitSound = SoundID.Grass;
        DustType = DustID.Grass;
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        return [new Item(ModContent.ItemType<LittlePrince>())];
    }
}

public class LittlePrinceRoseSpawn : GlobalTile
{
    private static bool HasValidGroundForAbigailsFlowerBelowSpot(int x, int y)
    {
        if (!WorldGen.InWorld(x, y, 2))
            return false;
        Tile tile = Framing.GetTileSafely(x, y + 1);
        if (tile == null || !tile.HasTile)
            return false;

        ushort type = tile.TileType;
        if (type < 0)
            return false;

        if (type != TileID.MushroomGrass && type != TileID.AshGrass && !TileID.Sets.Conversion.Grass[type])
            return false;
        return WorldGen.SolidTileAllowBottomSlope(x, y + 1);
    }

    private static bool NoNearbyAbigailsFlower(int i, int j)
    {
        int num = Utils.Clamp(i - 120, 10, Main.maxTilesX - 1 - 10);
        int num2 = Utils.Clamp(i + 120, 10, Main.maxTilesX - 1 - 10);
        int num3 = Utils.Clamp(j - 120, 10, Main.maxTilesY - 1 - 10);
        int num4 = Utils.Clamp(j + 120, 10, Main.maxTilesY - 1 - 10);
        for (int k = num; k <= num2; k++)
        {
            for (int l = num3; l <= num4; l++)
            {
                Tile tile = Framing.GetTileSafely(k, l);
                if (tile.HasTile && tile.TileType == ModContent.TileType<LittlePrinceRose>())
                    return false;
            }
        }

        return true;
    }

    public override void RandomUpdate(int i, int j, int type)
    {
        if (type != TileID.Tombstones) return;
        Vector2 spt = new Vector2(Main.spawnTileX, Main.spawnTileY);
        var l = Vector2.Distance(new Vector2(i, j), spt);
        var m = 0f;
        for (int n = 0; n < 4; n++)
            m = Math.Max(Vector2.Distance(new Vector2(Main.maxTilesX, Main.maxTilesY) * new Vector2(n % 2, n / 2), spt), m);
        int chance = (int)MathHelper.Lerp(5, 60, l / m);
        int times = (int)MathHelper.Lerp(4, 1, l / m);

        for (int k = 0; k < times; k++)
        {
            if (!Main.rand.NextBool(chance)) continue;
            int num2 = WorldGen.genRand.Next(Math.Max(10, i - 10), Math.Min(Main.maxTilesX - 10, i + 10));
            int num3 = WorldGen.genRand.Next(Math.Max(10, j - 10), Math.Min(Main.maxTilesY - 10, j + 10));
            if (HasValidGroundForAbigailsFlowerBelowSpot(num2, num3) && NoNearbyAbigailsFlower(num2, num3) && RecorderSystem.ShouldSpawnRecordItem<LittlePrince>() && WorldGen.PlaceTile(num2, num3, ModContent.TileType<LittlePrinceRose>(), mute: true))
            {
                if (Main.dedServ && Framing.GetTileSafely(num2, num3) != null && Framing.GetTileSafely(num2, num3).HasTile)
                    NetMessage.SendTileSquare(-1, num2, num3);
            }
        }
    }
}