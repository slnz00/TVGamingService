using Newtonsoft.Json;
using System;
using System.IO;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using System.Text;
using BackgroundService.Source.Controllers.EnvironmentControllers.Models;

namespace BackgroundService.Source.Services.Configs
{
    internal class ConfigService : Service
    {
        private readonly string CONFIG_PATH = InternalSettings.PATH_CONFIG;
        private readonly string JOB_CONFIG_PATH = InternalSettings.PATH_JOBS_CONFIG;

        private Config config;
        private JobsConfig jobsConfig;

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

        public EnvironmentConfig GetConfigForEnvironment(Environments environment)
        {
            switch (environment)
            {
                case Environments.PC:
                    return GetConfig().PC;
                case Environments.TV:
                    return GetConfig().TV;
                default:
                    throw new InvalidOperationException("Unimplemented environment configuration");
            }
        }

        public JobsConfig GetJobsConfig()
        {
            if (jobsConfig == null || !IsInitialized)
            {
                throw new InvalidOperationException("Cannot get jobs config, config service is not initialized");
            }

            return jobsConfig;
        }

        public EnvironmentJobsConfig GetJobsConfigForEnvironment(Environments environment)
        {

            switch (environment)
            {
                case Environments.PC:
                    return GetJobsConfig().PC;
                case Environments.TV:
                    return GetJobsConfig().TV;
                default:
                    throw new InvalidOperationException("Unimplemented environment configuration");
            }
        }

        public void LoadConfigFromFile()
        {
            Logger.Debug($"Loading config from JSON file: {CONFIG_PATH}");

            string configJson = File.ReadAllText(CONFIG_PATH, Encoding.Default);
            config = JsonConvert.DeserializeObject<Config>(configJson);

            Logger.Debug($"Loading jobs config from JSON file: {JOB_CONFIG_PATH}");

            string jobsConfigJson = File.ReadAllText(JOB_CONFIG_PATH, Encoding.Default);
            jobsConfig = JsonConvert.DeserializeObject<JobsConfig>(jobsConfigJson);
        }
    }
}
