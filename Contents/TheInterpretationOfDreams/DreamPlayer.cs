using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    [Flags]
    public enum DreamState
    {
        SkeletonMerchant = 1,
        Princess = 2,
        SantaClaus = 4,
        GoblinTinkerer = 8,
        Merchant = 16,
        Mechanic = 32,
        Demolitionist = 64,
        TaxCollector = 128,
        Steampunker = 256,
        Dryad = 512,
        WitchDoctor = 1024,
        Painter = 2048,
        Truffle = 4096,
        PartyGirl = 8192,
        ArmsDealer = 16384,
        Angler = 32768,
        Nurse = 65536,
        TravelingMerchant = 131072
    }
    public class DreamPlayer : ModPlayer
    {
        //0 - 破碎
        //1 - 巫师
        //2 - 动物学家
        //3 - 高尔夫球手
        //4 - 海盗
        //5 - 向导
        //6 - 酒保
        //7 - 服装商
        //8 - 发型师
        //9 - 染料商
        //10 - 电子人
        //11 - 胎儿之梦
        public Item[] dreamItemSlots = new Item[12];
        public DreamPlayer()
        {
            for (int n = 0; n < 12; n++)
                dreamItemSlots[n] = new Item();
        }
        public List<int> LuckyTimer = [];
        public List<int> UnluckyTimer = [];
        public int WizardDreamCount = 0;
        public DreamState UnlockState;
        public DreamState ActiveState;
        public HashSet<int> todayCheckedNPC = [];
        public override void LoadData(TagCompound tag)
        {
            WizardDreamCount = tag.GetInt(nameof(WizardDreamCount));
            UnlockState = (DreamState)tag.GetInt(nameof(UnlockState));
            ActiveState = (DreamState)tag.GetInt(nameof(ActiveState));
            if (tag.TryGet<List<int>>(nameof(LuckyTimer), out var luckyList))
                LuckyTimer = [.. luckyList];
            if (tag.TryGet<List<int>>(nameof(UnluckyTimer), out var unluckyList))
                UnluckyTimer = [.. unluckyList];

            for (int i = 0; i < 12; i++)
                if (tag.TryGet<TagCompound>(nameof(dreamItemSlots) + "_" + i, out var tagcompound))
                    dreamItemSlots[i] = ItemIO.Load(tagcompound);
            base.LoadData(tag);
        }
        public override void SaveData(TagCompound tag)
        {
            tag[nameof(WizardDreamCount)] = WizardDreamCount;
            tag[nameof(UnlockState)] = (int)UnlockState;
            tag[nameof(ActiveState)] = (int)ActiveState;
            tag[nameof(LuckyTimer)] = LuckyTimer;
            tag[nameof(UnluckyTimer)] = UnluckyTimer;

            for (int i = 0; i < 12; i++)
                tag[nameof(dreamItemSlots) + "_" + i] = ItemIO.Save(dreamItemSlots[i]);
            base.SaveData(tag);
        }
        public override void ModifyLuck(ref float luck)
        {
            luck += (LuckyTimer.Count - UnluckyTimer.Count) * .01f;
            base.ModifyLuck(ref luck);
        }
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            //mult *= (1 - .01f * WizardDreamCount);
            base.ModifyManaCost(item, ref reduce, ref mult);
        }
        public override void UpdateEquips()
        {
            Player.manaCost -= .01f * WizardDreamCount;

            if (CheckActive(DreamState.Dryad))
                Player.AddBuff(BuffID.DryadsWard, 2);

            if (CheckActive(DreamState.SantaClaus))
            {
                //Main.forceXMasForToday = true;
                Main.xMas = true;
            }

            if (CheckActive(DreamState.WitchDoctor))
                Player.maxMinions += 1;


            if (CheckActive(DreamState.Truffle))
                Player.aggro -= 100;
            base.UpdateEquips();
        }
        public override void ResetEffects()
        {
            //UnlockState = ActiveState  = (DreamState)(2 * 131072 - 1);
            //UnlockState = ActiveState = 0;

            if (Main.dayTime && Main.time < 1.0)
            {
                todayCheckedNPC.Clear();

                WorldGen.SpawnTravelNPC();
            }
            int count = LuckyTimer.Count;
            for (int i = 0; i < count; i++)
                LuckyTimer[i]--;
            LuckyTimer.RemoveAll(i => i <= 0);
            count = UnluckyTimer.Count;
            for (int i = 0; i < count; i++)
                UnluckyTimer[i]--;
            UnluckyTimer.RemoveAll(i => i <= 0);
            base.ResetEffects();
        }

        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (CheckActive(DreamState.ArmsDealer) && Main.rand.Next(100) < 15)
                return false;
            return base.CanConsumeAmmo(weapon, ammo);
        }

        public bool CheckUnlock(DreamState flag) => (int)(flag & UnlockState) > 0;

        public bool CheckActive(DreamState flag) => (int)(flag & ActiveState) > 0;

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!CheckActive(DreamState.Demolitionist) || proj.owner != Player.whoAmI) return;
            var type = proj.type;
            if (type == 30 || type == 397 || type == 517 || type == 28 || type == 37 || type == 516 || type == 29 || type == 470 || type == 637 || type == 108 || type == 281 || type == 588 || type == 519 || type == 773 || type == 183 || type == 181 || type == 566 || type == 1002)
                modifiers.Cancel();
            base.ModifyHitByProjectile(proj, ref modifiers);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!CheckActive(DreamState.TaxCollector) || !target.townNPC || target.type == NPCID.TaxCollector) return;

            int num36 = 71;
            if (Main.rand.NextBool(10))
                num36 = 72;

            if (Main.rand.NextBool(100))
                num36 = 73;
            int num37 = Item.NewItem(Entity.GetSource_OnHit(target), (int)target.position.X, (int)target.position.Y, target.width, target.height, num36);
            Main.item[num37].stack = Main.rand.Next(1, 11);
            Main.item[num37].velocity.Y = Main.rand.Next(-20, 1) * 0.2f;
            Main.item[num37].velocity.X = Main.rand.Next(10, 31) * 0.2f * hit.HitDirection;
            Main.item[num37].timeLeftInWhichTheItemCannotBeTakenByEnemies = 60;
            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(148, -1, -1, null, num37);


            base.OnHitNPC(target, hit, damageDone);
        }
    }

    public static class DreamHelper
    {
        public static bool CheckDreamActive(this Player player, DreamState flag) => player.GetModPlayer<DreamPlayer>().CheckActive(flag);
    }
}
