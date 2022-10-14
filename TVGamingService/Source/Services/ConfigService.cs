using Newtonsoft.Json;
using System;
using System.IO;
using TVGamingService.Source.Models;
using TVGamingService.Source.Providers;

namespace TVGamingService.Source.Services
{
    internal class ConfigService : BaseService
    {
        private readonly string CONFIG_PATH = InternalSettings.PATH_CONFIG;

        private Config config;

        public ConfigService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            LoadConfigFromFile();
        }

        public Config GetConfig()
        {
            if (config == null || !IsInitialized)
            {
                throw new InvalidOperationException("Cannot get config, config service is not initialized");
            }

            return config;
        }

        public void LoadConfigFromFile()
        {
            Logger.Debug($"Loading config from JSON file: {CONFIG_PATH}");

            string configJson = File.ReadAllText(CONFIG_PATH);
            config = JsonConvert.DeserializeObject<Config>(configJson);
        }
    }
}
