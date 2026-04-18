using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Xna.Framework;

namespace MatterRecord.Contents.Recorder.Dialogue
{
    public class RecorderLocalPlayer : ModPlayer
    {
        private static readonly string[] CheatModNames = new[]
        {
            "HEROsMod", "CheatSheet", "DragonLens"
        };

        private static string DataFilePath => Path.Combine(Main.SavePath, "Mods", "MatterRecord", "LocalData.json");

        public LocalTriggerData LocalData { get; private set; }
        private List<DialogueModule> _modules;

        public bool AnyModuleActive => _modules.Any(m => m.IsActive);

        public override void Initialize()
        {
            _modules = new List<DialogueModule>();
            var moduleType = typeof(DialogueModule);
            var assembly = moduleType.Assembly;
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && moduleType.IsAssignableFrom(type))
                {
                    var ctor = type.GetConstructor(new[] { typeof(RecorderLocalPlayer) });
                    if (ctor != null)
                    {
                        var module = (DialogueModule)ctor.Invoke(new object[] { this });
                        _modules.Add(module);
                    }
                    else
                    {
                        Mod.Logger.Warn($"Dialogue module {type.Name} missing constructor (RecorderLocalPlayer) - skipped");
                    }
                }
            }
        }

        public override void OnEnterWorld()
        {
            LocalData = LoadData();

            foreach (var module in _modules)
                module.OnEnterWorld(Player, LocalData);

            // 仅在多人的时候将本地数据发给服务器
            if (Main.netMode is NetmodeID.MultiplayerClient)
                ExtraLifeSync.Get(LocalData.ExtraLife).Send();
        }

        public override void PostUpdate()
        {
            if (Main.netMode == NetmodeID.Server) return;
            LocalData ??= LoadData();

            foreach (var module in _modules)
                module.PostUpdate(Player, LocalData);
        }

        public override void OnHurt(Player.HurtInfo info)
        {
            if (Main.netMode == NetmodeID.Server) return;
            LocalData ??= LoadData();

            foreach (var module in _modules)
                module.OnHurt(Player, LocalData, info);
        }

        public void SendMessageWithBubble(string prefix, string content)
        {
            Main.NewText(prefix + content, Color.White);
            NPC recorder = GetFirstRecorder();
            if (recorder?.ModNPC is Recorder modRecorder)
            {
                modRecorder.ChatText = content;
                modRecorder.ChatTimer = 390;
            }
        }

        public NPC GetFirstRecorder()
        {
            int type = ModContent.NPCType<Recorder>();
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == type)
                    return Main.npc[i];
            }
            return null;
        }

        public bool AreCheatModsEnabled()
        {
            return ModLoader.Mods.Any(mod => CheatModNames.Any(name =>
                mod.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        public void SaveData()
        {
            try
            {
                string dir = Path.GetDirectoryName(DataFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(LocalData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DataFilePath, json);
            }
            catch (Exception ex)
            {
                Mod.Logger.Error("Failed to save local data: " + ex.Message);
            }
        }

        private LocalTriggerData LoadData()
        {
            try
            {
                if (File.Exists(DataFilePath))
                {
                    string json = File.ReadAllText(DataFilePath);
                    return JsonSerializer.Deserialize<LocalTriggerData>(json) ?? new LocalTriggerData();
                }
            }
            catch (Exception ex)
            {
                Mod.Logger.Warn("Failed to load local data: " + ex.Message);
            }
            return new LocalTriggerData();
        }
    }
}