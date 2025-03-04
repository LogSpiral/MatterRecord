using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;

namespace MatterRecord.Contents.TheInterpretationOfDreams
{
    [Flags]
    public enum DreamState
    {
        SkeletonMerchant = 1,
        Princess = 2,
        SantaClaus = 4,
        aaaaaaa = 8,
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
        Nurse = 65536
    }
    public class DreamPlayer : ModPlayer
    {
        public List<int> LuckyTimer = [];
        public List<int> UnluckyTimer = [];
        public int WizardDreamCount = 0;
        public DreamState UnlockState;
        public DreamState ActiveState;

        public override void LoadData(TagCompound tag)
        {
            WizardDreamCount = tag.GetInt(nameof(WizardDreamCount));
            UnlockState = (DreamState)tag.GetInt(nameof(UnlockState));
            ActiveState = (DreamState)tag.GetInt(nameof(ActiveState));
            if (tag.TryGet<List<int>>(nameof(LuckyTimer), out var luckyList))
                LuckyTimer = [.. luckyList];
            if (tag.TryGet<List<int>>(nameof(UnluckyTimer), out var unluckyList))
                UnluckyTimer = [.. unluckyList];
            base.LoadData(tag);
        }
        public override void SaveData(TagCompound tag)
        {
            tag[nameof(WizardDreamCount)] = WizardDreamCount;
            tag[nameof(UnlockState)] = (int)UnlockState;
            tag[nameof(ActiveState)] = (int)ActiveState;
            tag[nameof(LuckyTimer)] = LuckyTimer;
            tag[nameof(UnluckyTimer)] = UnluckyTimer;
            base.SaveData(tag);
        }
        public override void ModifyLuck(ref float luck)
        {
            luck += (LuckyTimer.Count - UnluckyTimer.Count) * .01f;
            base.ModifyLuck(ref luck);
        }
        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            mult *= (1 - .1f * WizardDreamCount);
            base.ModifyManaCost(item, ref reduce, ref mult);
        }
        public override void ResetEffects()
        {
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
    }
}
