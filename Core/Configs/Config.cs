using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Configs
{
    public class BackgroundServiceConfig
    {
        public static BackgroundServiceConfig ReadFromFile(string filePath)
        {
            string configJson = File.ReadAllText(filePath, Encoding.Default);
            return JsonConvert.DeserializeObject<BackgroundServiceConfig>(configJson);
        }

        public static BackgroundServiceConfig WriteToFile(string filePath)
        {
            throw new NotImplementedException();
        }

        public HotkeysConfig Hotkeys { get; set; }
        public EnvironmentConfig PC { get; set; }
        public EnvironmentConfig TV { get; set; }
        public ThirdPartyConfig ThirdParty { get; set; }
        public List<BackupConfig> Backups { get; set; }
    }

    public class BackupConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int? Amount { get; set; } = null;
    }

    public class HotkeysConfig
    {
        public List<string> SwitchEnvironment { get; set; }
        public List<string> ResetEnvironment { get; set; }
        public List<string> ToggleCursorVisibility { get; set; }
        public List<string> ToggleConsoleVisibility { get; set; }
    }

    public class DisplayResolutionConfig
    {
        public short Width { get; set; }
        public short Height { get; set; }
    }

    public class EnvironmentConfig
    {
        public SoundConfig Sound { get; set; }
        public DisplayConfig Display { get; set; }
        public string WallpaperPath { get; set; }
    }

    public class SoundConfig
    {
        public string DeviceName { get; set; }
    }

    public class DisplayConfig
    {
        public string DevicePath { get; set; }
        public string DeviceName { get; set; }
    }

    public class ThirdPartyConfig
    {
        public AppConfig PlayniteFullscreen { get; set; }
        public AppConfig PlayniteDesktop { get; set; }
        public AppConfig DS4Windows { get; set; }
        public AppConfig Steam { get; set; }
        public AppConfig EpicGames { get; set; }
        public AppConfig BattleNet { get; set; }
    }

    public class AppConfig
    {
        public string ProcessName { get; set; }

        public string Path { get; set; }
    }
}
