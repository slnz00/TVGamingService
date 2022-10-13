using System.Diagnostics;
using TVGamingService.Providers;
using TVGamingService.Utils;

namespace TVGamingService.Services.Apps
{
    internal class DS4WindowsService : BaseService
    {
        public DS4WindowsService(ServiceProvider services) : base(services) { }

        public void OpenDS4Windows()
        {
            var config = Services.Config.GetConfig();

            Logger.Debug("Opening DS4Windows");
            ProcessUtils.StartProcess(config.Apps.DS4Windows.Path);
            Logger.Debug("DS4Windows opened");
        }

        public void CloseDS4Windows(bool forceClose = false)
        {
            var config = Services.Config.GetConfig();

            if (forceClose)
            {
                Logger.Debug("Forcefully closing DS4Windows");
                ProcessUtils.CloseProcess(config.Apps.DS4Windows.ProcessName, true);

                return;
            }

            Logger.Debug("Gracefully shutting down DS4Windows");
            ProcessUtils.StartProcess(config.Apps.DS4Windows.Path, "-command shutdown", ProcessWindowStyle.Hidden, true);
        }
    }
}
