using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace TVGamingService
{
    internal class Config
    {

        private static readonly string CONFIG_PATH = Utils.GetFullPath("config.json");

        public EnvironmentConfig PC { get; set; }
        public EnvironmentConfig TV { get; set; }
        public AppConfig Playnite { get; set; }
        public AppConfig DS4Windows { get; set; }
        

        public static Config LoadFromFile () {
            string configJson = File.ReadAllText(CONFIG_PATH);
            return JsonConvert.DeserializeObject<Config>(configJson); ;
        }
    }
    internal class DisplayResolutionConfig
    {
        public short Width { get; set; }
        public short Height { get; set; }
    }

    internal class EnvironmentConfig
    {
        public string SoundDevice { get; set; }
        public string DisplayModel { get; set; }
        public DisplayResolutionConfig DisplayResolution { get; set; }
    }

    internal class AppConfig
    {
        public string Path { get; set; }
    }
}
