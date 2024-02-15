using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Configs
{
    public class Config
    {
        public static Config ReadFromFile(string filePath)
        {
            string configJson = File.ReadAllText(filePath, Encoding.Default);

            return JsonConvert.DeserializeObject<Config>(configJson, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });
        }

        public static void WriteToFile(Config config, string filePath)
        {
            var configJson = JsonConvert.SerializeObject(config, Formatting.Indented);

            File.WriteAllText(filePath, configJson, Encoding.Default);
        }

        public HotkeysConfig Hotkeys = new HotkeysConfig();
        public GameEnvironmentConfig GameEnvironment = new GameEnvironmentConfig();
        public ThirdPartyConfig ThirdParty = new ThirdPartyConfig();
        public List<BackupConfig> Backups = new List<BackupConfig>();
        public List<string> GameConfigPaths = new List<string>();
    }

    public class BackupConfig
    {
        public string Name;
        public string Path;
        public int? Amount = null;
    }

    public class HotkeysConfig
    {
        public List<string> SwitchEnvironment = new List<string> { "Alt", "NumPad0" };
        public List<string> ResetEnvironment = new List<string> { "Alt", "NumPad1" };
        public List<string> ToggleCursorVisibility = new List<string> { "Alt", "NumPad8" };
        public List<string> ToggleConsoleVisibility = new List<string> { "Alt", "NumPad9" };
    }

    public class GameEnvironmentConfig
    {
        public AudioConfig Audio = new AudioConfig();
        public DisplayConfig Display = new DisplayConfig();
        public string WallpaperPath = "";
    }

    public class AudioConfig
    {
        public string OutputDeviceName = "";
        public string InputDeviceName = "";
    }

    public class DisplayConfig
    {
        public string DevicePath = "";
        public string DeviceName = "";
    }

    public class ThirdPartyConfig
    {
        public AppConfig PlayniteFullscreen = new AppConfig()
        {
            ProcessName = "Playnite.FullscreenApp"
        };
        public AppConfig PlayniteDesktop = new AppConfig()
        {
            ProcessName = "Playnite.DesktopApp"
        };
        public AppConfig DS4Windows = null;
        public AppConfig Steam = null;
        public AppConfig EpicGames = null;
        public AppConfig BattleNet = null;
    }

    public class AppConfig
    {
        public string ProcessName = "";
        public string Path = "";
    }
}
