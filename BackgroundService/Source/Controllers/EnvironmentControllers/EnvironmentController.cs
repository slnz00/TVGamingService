using System;
using BackgroundService.Source.Controllers.Models;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;

namespace BackgroundService.Source.Controllers.EnvironmentControllers
{
    internal abstract class EnvironmentController
    {
        protected readonly ServiceProvider Services;
        protected readonly LoggerProvider Logger;
        public readonly EnvironmentConfig Config;
        public readonly Environments Environment;
        public readonly MainController MainController;

        public string EnvironmentName => Enum.GetName(typeof(Environments), Environment);

        public EnvironmentController(Environments environment, EnvironmentConfig config, MainController mainController, ServiceProvider services)
        {
            Services = services;
            Environment = environment;
            Config = config;
            Logger = new LoggerProvider(GetType().Name);
            MainController = mainController;
        }

        public void Setup()
        {
            OnSetup();

            // TODO: Run jobs
        }

        public void Reset()
        {
            OnReset();
        }

        public void Teardown()
        {
            OnTeardown();

            // TODO: Stop jobs
        }

        protected abstract void OnSetup();

        protected abstract void OnTeardown();

        protected abstract void OnReset();
    }
}
