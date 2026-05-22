using System;
using System.IO;
using System.Text.Json;
using Terraria;
using Terraria.ModLoader;

namespace MatterRecord.Contents.EasterEgg
{
    public class EasterEggSystem : ModSystem
    {
        private static string SavePath => Path.Combine(Main.SavePath, "Mods", "MatterRecord");
        private static string FilePath => Path.Combine(SavePath, "EasterEgg.json");
        private static EasterEggData _data = new EasterEggData();

        public static EasterEggData GetData() => _data;
        public static void Save() => SaveData();

        public override void Load()
        {
            Directory.CreateDirectory(SavePath);
            if (File.Exists(FilePath))
            {
                try
                {
                    string json = File.ReadAllText(FilePath);
                    var loaded = JsonSerializer.Deserialize<EasterEggData>(json);
                    if (loaded != null)
                        _data = loaded;
                }
                catch
                {
                    _data = new EasterEggData();
                }
            }
            else
            {
                _data = new EasterEggData();
            }

            // 每年6月15日强制触发火把神彩蛋
            var today = DateTime.Today;
            if (today.Month == 6 && today.Day == 15)
            {
                _data.TorchGodEasterEggTriggered = true;
                Save();
            }
        }

        public override void Unload()
        {
            Save();
        }

        private static void SaveData()
        {
            try
            {
                string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }

        // 重置火把神彩蛋触发标志（触发后调用）
        public static void ResetTorchGodTrigger()
        {
            _data.TorchGodEasterEggTriggered = false;
            Save();
        }
    }
}