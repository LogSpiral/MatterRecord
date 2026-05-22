namespace MatterRecord.Contents.EasterEgg
{
    public class EasterEggData
    {
        // 火把神彩蛋是否已触发（仅针对非6月15日的额外触发）
        public bool TorchGodEasterEggTriggered { get; set; } = false;

        // 复活节彩蛋当天是否已经触发过（每天重置）
        public bool EasterEggTriggeredToday { get; set; } = false;

        // 记录上次触发的日期，用于跨天重置
        public int LastEasterTriggerYear { get; set; } = 0;
        public int LastEasterTriggerMonth { get; set; } = 0;
        public int LastEasterTriggerDay { get; set; } = 0;

        // 农历新年烟花彩蛋：记录上次触发的新年（农历年，如2026）
        public int LastNewYearFireworkYear { get; set; } = 0;
    }
}