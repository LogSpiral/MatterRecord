namespace MatterRecord.Contents.Recorder.Dialogue
{
    public class LocalTriggerData
    {
        public int CheatCounter { get; set; } = 0;
        public string CheatWorldName { get; set; } = "";
        public int TuningCounter { get; set; } = 0;
        public int LifeCrystalCounter { get; set; } = 0;
        public int CharlieCounter { get; set; } = 0;
        public int ExtraLife { get; set; } = 0;  // 本地累计贡献的生命值
    }
}