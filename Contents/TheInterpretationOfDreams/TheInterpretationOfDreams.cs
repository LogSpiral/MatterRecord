using MatterRecord.Contents.Recorder;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent;

namespace MatterRecord.Contents.TheInterpretationOfDreams;

public class TheInterpretationOfDreams : ModItem, IRecordBookItem
{
    ItemRecords IRecordBookItem.RecordType => ItemRecords.TheInterpretationOfDreams;
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.useStyle = ItemUseStyleID.Swing;
        Item.value = Item.sellPrice(0, 5);
        Item.rare = ItemRarityID.LightRed;
        Item.useTurn = true;
        Item.useTime = 18;
        Item.useAnimation = 18;
        Item.width = 24;
        Item.height = 28;
        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;
        Item.scale = 1.15f;
        base.SetDefaults();
    }

    public override void Load()
    {
        On_PlayerSleepingHelper.StartSleeping += SpawnRecordDream;
    }

    private static void SpawnRecordDream(On_PlayerSleepingHelper.orig_StartSleeping orig, ref PlayerSleepingHelper self, Player player, int x, int y)
    {
        orig?.Invoke(ref self, player, x, y);
        if (RecorderSystem.ShouldSpawnRecordItem<TheInterpretationOfDreams>())
        {
            foreach (var item in player.inventory)
                if (item.type == ItemID.BugNet)
                    player.QuickSpawnItem(player.GetSource_Misc("Sleeping"), ModContent.ItemType<TheInterpretationOfDreams>());

        }
    }

    public override void AddRecipes()
    {
        this.RegisterBookRecipe(ItemID.BugNet);
    }
    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer) return true;
        var mplr = player.GetModPlayer<DreamPlayer>();
        if (player.itemAnimation == player.itemAnimationMax / 2)
            foreach (var npc in Main.npc)
            {
                if (!npc.active || !(npc.townNPC || npc.type == NPCID.SkeletonMerchant || npc.type == NPCID.TravellingMerchant)) continue;
                if (npc.ModNPC != null) continue;
                if (npc.type is NPCID.TownDog or NPCID.TownCat or NPCID.TownSlimeBlue or NPCID.TownSlimeCopper or NPCID.TownSlimeGreen
                    or NPCID.TownSlimeOld or NPCID.TownSlimePurple or NPCID.TownSlimeRainbow or NPCID.TownSlimeRed or NPCID.TownSlimeYellow) continue;
                if (npc.Hitbox.Intersects(Utils.CenteredRectangle(player.Center + Vector2.UnitX * 24 * player.direction, new Vector2(60, 96))))
                {
                    if (mplr.todayCheckedNPC.Contains(npc.type))
                    {
                        Main.NewText(this.GetLocalizedValue("NoDreamHint"), Color.DeepPink);
                        continue;
                    }
                    mplr.todayCheckedNPC.Add(npc.type);
                    if (Main.rand.NextBool(10000) && mplr.dreamItemSlots[11].stack == 0)
                    {
                        mplr.dreamItemSlots[11].SetDefaults(ModContent.ItemType<TaijiNoYume.TaijiNoYume>());
                        Main.NewText(" ~ Soul undertone", Color.Gray);
                        //Main.NewText("哇偶这里本来应该扫出个胎儿之梦的但是笨蛋螺线还没实装，你运气真好");
                        continue;
                    }
                Label:
                    switch (Main.rand.Next(10))
                    {
                        case 0:
                            {
                                var targetName = NPCID.Search.GetName(npc.type);
                                var names = typeof(DreamState).GetEnumNames();
                                var index = Array.IndexOf(names, targetName);
                                if (index != -1)
                                {
                                    var state = (DreamState)Enum.GetValues(typeof(DreamState)).GetValue(index);
                                    if (!mplr.CheckUnlock(state))
                                        mplr.UnlockState |= state;
                                    else
                                        goto Label;
                                }
                                switch (npc.type)
                                {
                                    case NPCID.Wizard:
                                        {
                                            var targetItem = mplr.dreamItemSlots[1];
                                            if (targetItem.stack == 10)
                                                goto Label;
                                            if (targetItem.type == ItemID.None)
                                                targetItem.SetDefaults(ModContent.ItemType<WizardDream>());
                                            else
                                                targetItem.stack++;
                                            break;
                                        }
                                    case NPCID.BestiaryGirl:
                                        {
                                            var targetItem = mplr.dreamItemSlots[2];
                                            //if (DreamWorld.UsedZoologistDream)
                                            //    goto Label;
                                            if (targetItem.type != ItemID.None)
                                                goto Label;
                                            targetItem.SetDefaults(ModContent.ItemType<ZoologiseDream>());
                                            break;
                                        }
                                    case NPCID.Golfer:
                                        {
                                            var targetItem = mplr.dreamItemSlots[3];
                                            if (targetItem.type != ItemID.None)
                                                goto Label;
                                            targetItem.SetDefaults(ModContent.ItemType<GolferDream>());
                                            break;
                                        }

                                    case NPCID.Pirate:
                                        {
                                            var targetItem = mplr.dreamItemSlots[4];
                                            if (targetItem.type != ItemID.None)
                                                goto Label;
                                            targetItem.SetDefaults(ModContent.ItemType<PirateDream>());
                                            break;
                                        }
                                    case NPCID.Guide:
                                        {
                                            var targetItem = mplr.dreamItemSlots[5];
                                            if (targetItem.stack == 9999)
                                                goto Label;
                                            if (targetItem.type == ItemID.None)
                                                targetItem.SetDefaults(ModContent.ItemType<GuideDream>());
                                            else
                                                targetItem.stack++;
                                            break;
                                        }
                                    case NPCID.DD2Bartender:
                                        {
                                            var targetItem = mplr.dreamItemSlots[6];
                                            if (targetItem.stack == 9999)
                                                goto Label;
                                            if (targetItem.type == ItemID.None)
                                                targetItem.SetDefaults(ModContent.ItemType<TavernkeepDream>());
                                            else
                                                targetItem.stack++;
                                            break;
                                        }
                                    case NPCID.Clothier:
                                        {
                                            var targetItem = mplr.dreamItemSlots[7];
                                            if (targetItem.stack == 9999)
                                                goto Label;
                                            if (targetItem.type == ItemID.None)
                                                targetItem.SetDefaults(ModContent.ItemType<ClothierDream>());
                                            else
                                                targetItem.stack++;
                                            break;
                                        }
                                    case NPCID.Stylist:
                                        {
                                            var targetItem = mplr.dreamItemSlots[8];
                                            if (targetItem.stack == 9999)
                                                goto Label;
                                            if (targetItem.type == ItemID.None)
                                                targetItem.SetDefaults(ModContent.ItemType<StylistDream>());
                                            else
                                                targetItem.stack++;
                                            break;
                                        }
                                    case NPCID.DyeTrader:
                                        {
                                            var targetItem = mplr.dreamItemSlots[9];
                                            if (targetItem.stack == 9999)
                                                goto Label;
                                            if (targetItem.type == ItemID.None)
                                                targetItem.SetDefaults(ModContent.ItemType<DyeTraderDream>());
                                            else
                                                targetItem.stack++;
                                            break;
                                        }
                                    case NPCID.Cyborg:
                                        {
                                            var targetItem = mplr.dreamItemSlots[10];
                                            if (targetItem.type != ItemID.None)
                                                goto Label;
                                            targetItem.SetDefaults(ModContent.ItemType<CybrogDream>());
                                            break;
                                        }
                                }
                                Main.NewText(this.GetLocalizedValue("SpecialDreamHint"), new Color(255, 255, 127));
                                break;
                            }
                        case < 3:
                            {
                                mplr.LuckyTimer.Add(43200);
                                player.QuickSpawnItem(Item.GetSource_FromThis(), ItemID.GoldCoin);
                                Main.NewText(this.GetLocalizedValue("SweetDreamHint"), new Color(253, 195, 229));
                                break;
                            }
                        case < 5:
                            {
                                mplr.UnluckyTimer.Add(43200);
                                Main.NewText(this.GetLocalizedValue("NightmareHint"), new Color(3, 20, 96));

                                break;
                            }
                        default:
                            {
                                var targetItem = mplr.dreamItemSlots[0];
                                if (targetItem.stack == 9999)
                                    goto Label;
                                if (targetItem.type == ItemID.None)
                                    targetItem.SetDefaults(ModContent.ItemType<BrokenDream>());
                                else
                                    targetItem.stack++;
                                Main.NewText(this.GetLocalizedValue("BrokenDreamHint"), new Color(153, 153, 153));

                                break;
                            }
                    }
                }
            }
        return base.UseItem(player);
    }

    public override void UseStyle(Player player, Rectangle heldItemFrame)
    {
        player.itemRotation += MathHelper.PiOver4 * player.direction;
        player.itemLocation = player.Center - player.itemRotation.ToRotationVector2() * 12 * player.direction;
        //player.itemLocation += new Vector2(-12 * player.direction, 8);
        base.UseStyle(player, heldItemFrame);
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            player.itemTime = player.itemAnimation = 0;
            if (!DreamSlotUI.Active)
                DreamSlotUI.Open();
            else
                DreamSlotUI.Close();

            return false;
        }
        return base.CanUseItem(player);
    }
}