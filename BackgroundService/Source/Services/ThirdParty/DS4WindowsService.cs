using System.Diagnostics;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using Core.Utils;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class DS4WindowsService : Service
    {
        private AppConfig ds4WindowsConfig;

        public DS4WindowsService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            ds4WindowsConfig = Services.Config.GetConfig().ThirdParty.DS4Windows;
        }

        public void OpenDS4Windows()
        {
            var config = Services.Config.GetConfig();

            Logger.Debug("Opening DS4Windows");
            ProcessUtils.StartProcess(config.ThirdParty.DS4Windows.Path);
            Logger.Debug("DS4Windows opened");
        }

        public void CloseDS4Windows(bool forceClose = false)
        {
            var config = Services.Config.GetConfig();

            if (forceClose)
            {
                Logger.Debug("Forcefully closing DS4Windows");
                ProcessUtils.CloseProcess(ds4WindowsConfig.ProcessName, true);

                return;
            }

            Logger.Debug("Gracefully shutting down DS4Windows");
            ProcessUtils.StartProcess(ds4WindowsConfig.Path, "-command shutdown", ProcessWindowStyle.Hidden, true);
        }
    }
}
