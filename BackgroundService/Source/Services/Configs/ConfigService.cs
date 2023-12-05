using System;
using BackgroundService.Source.Providers;
using Core.Configs;
using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using Core.Components;

namespace BackgroundService.Source.Services.Configs
{
    internal class ConfigService : Service
    {
        public FileChangeWatcher ConfigWatcher { get; private set; }

        private readonly object threadLock = new object();

        private BackgroundServiceConfig config;
        private JobsConfig jobsConfig;

        public ConfigService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            LoadConfigFromFile();
            LoadJobConfigFromFile();
            SetupWatcher();
        }

        public BackgroundServiceConfig GetConfig()
        {
            lock (threadLock)
            {
                if (config == null || !IsInitialized)
                {
                    throw new InvalidOperationException("Cannot get config, config service is not initialized");
                }

                return config;
            }
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

        private void LoadConfigFromFile()
        {
            lock (threadLock)
            {
                Logger.Debug($"Loading config from JSON file: {InternalSettings.PATH_CONFIG}");

                config = BackgroundServiceConfig.ReadFromFile(InternalSettings.PATH_CONFIG);
            }
        }

        private void LoadJobConfigFromFile()
        {
            lock (threadLock)
            {
                Logger.Debug($"Loading jobs config from JSON file: {InternalSettings.PATH_JOBS_CONFIG}");

                jobsConfig = JobsConfig.ReadFromFile(InternalSettings.PATH_JOBS_CONFIG);
            }
        }

        private void SetupWatcher()
        {
            ConfigWatcher = new FileChangeWatcher(InternalSettings.PATH_CONFIG);

            ConfigWatcher.OnChanged(() =>
            {
                LoadConfigFromFile();
                Logger.Info("Configuration file changed, reloading...");
            });
        }
    }
}
