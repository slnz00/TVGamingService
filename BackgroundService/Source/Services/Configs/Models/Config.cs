using System.Collections.Generic;

namespace BackgroundService.Source.Services.Configs.Models
{
    internal class Config
    {
        public HotkeysConfig Hotkeys { get; set; }
        public EnvironmentConfig PC { get; set; }
        public EnvironmentConfig TV { get; set; }
        public ThirdPartyConfig ThirdParty { get; set; }
        public List<BackupConfig> Backups { get; set; }
    }

    internal class BackupConfig
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int? Amount { get; set; } = null;
    }

    internal class HotkeysConfig
    {
        public List<string> SwitchEnvironment { get; set; }
        public List<string> ResetEnvironment { get; set; }
        public List<string> ResetDisplay { get; set; }
        public List<string> ToggleCursorVisibility { get; set; }
        public List<string> ToggleConsoleVisibility { get; set; }
    }

    internal class DisplayResolutionConfig
    {
        public short Width { get; set; }
        public short Height { get; set; }
    }

    internal class EnvironmentConfig
    {
        public SoundConfig Sound { get; set; }
        public DisplayConfig Display { get; set; }
        public string WallpaperPath { get; set; }
    }

    internal class SoundConfig
    {
        public string DeviceName { get; set; }
    }

    internal class DisplayConfig
    {
        public string DeviceName { get; set; }
        public DisplayResolutionConfig Resolution { get; set; }
        public int RefreshRate { get; set; }
    }

    internal class ThirdPartyConfig
    {
        public AppConfig PlayniteFullscreen { get; set; }
        public AppConfig PlayniteDesktop { get; set; }
        public AppConfig DS4Windows { get; set; }
        public AppConfig Steam { get; set; }
        public AppConfig EpicGames { get; set; }
        public AppConfig BattleNet { get; set; }
    }

    internal class AppConfig
    {
        public string ProcessName { get; set; }

        public string Path { get; set; }
    }
}
