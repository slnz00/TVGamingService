using System.Diagnostics;
using System.IO;
using System.Threading;
using BackgroundService.Source.Common;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using Core.Utils;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class DS4WindowsService : Service
    {
        private ProcessWatcher watcher;

        private AppConfig ds4WindowsConfig;

        public DS4WindowsService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            ds4WindowsConfig = Services.Config.GetConfig().ThirdParty.DS4Windows;

            watcher = new ProcessWatcher(ds4WindowsConfig.ProcessName, new ProcessWatcher.Events
            {
                OnProcessClosed = () => ReopenOnUnexpectedExit()
            });
        }

        public void OpenDS4Windows()
        {
            Logger.Debug("Opening DS4Windows");
            ProcessUtils.StartProcess(ds4WindowsConfig.Path);

            StartWatcher();
        }

        public void CloseDS4Windows(bool forceClose = false)
        {
            StopWatcher();

            if (forceClose)
            {
                Logger.Debug("Forcefully closing DS4Windows");
                ProcessUtils.CloseProcess(ds4WindowsConfig.ProcessName, true);

                return;
            }

            Logger.Debug("Gracefully shutting down DS4Windows");
            ProcessUtils.StartProcess(Path.GetFullPath(ds4WindowsConfig.Path), "-command shutdown", ProcessWindowStyle.Hidden, true);
        }

        private void ReopenOnUnexpectedExit()
        {
            Logger.Error("DS4Windows closed unexpectedly");

            OpenDS4Windows();
        }

        private void StartWatcher()
        {
            if (watcher.IsWatcherRunning)
            {
                return;
            }

            // Should only start watcher when DS4Windows is already running:
            bool alreadyRunning = Process.GetProcessesByName(ds4WindowsConfig.ProcessName).Length != 0;
            if (!alreadyRunning)
            {
                Logger.Error("Failed to start DS4Windows process watcher, DS4Windows is not running");
                return;
            }

            watcher.Start();
        }

        private void StopWatcher()
        {
            if (!watcher.IsWatcherRunning)
            {
                return;
            }

            watcher.Stop();
        }
    }
}
