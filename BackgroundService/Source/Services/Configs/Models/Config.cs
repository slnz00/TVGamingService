namespace BackgroundService.Source.Services.Configs.Models
{
    internal class Config
    {
        public EnvironmentConfig PC { get; set; }
        public EnvironmentConfig TV { get; set; }
        public ThirdPartyConfig ThirdParty { get; set; }
    }

    internal class DisplayResolutionConfig
    {
        public short Width { get; set; }
        public short Height { get; set; }
    }

    internal class EnvironmentConfig
    {
        public SoundDeviceConfig SoundDevice { get; set; }
        public DisplayConfig Display { get; set; }
        public string WallpaperPath { get; set; }
    }

    internal class SoundDeviceConfig
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
        public AppConfig Playnite { get; set; }
        public AppConfig DS4Windows { get; set; }
        public AppConfig Steam { get; set; }
        public AppConfig EpicGames { get; set; }
    }

    internal class AppConfig
    {
        public string ProcessName { get; set; }

        public string Path { get; set; }
    }
}
