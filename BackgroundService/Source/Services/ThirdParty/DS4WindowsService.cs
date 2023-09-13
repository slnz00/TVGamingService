using System.Diagnostics;
using System.IO;
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

        private bool enabled;

        public DS4WindowsService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            ds4WindowsConfig = Services.Config.GetConfig().ThirdParty.DS4Windows;
            enabled = ds4WindowsConfig != null;

            watcher = new ProcessWatcher(ds4WindowsConfig.ProcessName, new ProcessWatcher.Events
            {
                OnProcessClosed = () => ReopenOnUnexpectedExit()
            });
        }

        public void OpenDS4Windows()
        {
            Logger.Info("Opening DS4Windows");

            if (!enabled)
            {
                Logger.Debug("DS4Windows is disabled, skipping...");
                return;
            }

            var fullPath = FSUtils.GetAbsolutePath(ds4WindowsConfig.Path);

            ProcessUtils.StartProcess(fullPath);

            StartWatcher();
        }

        public void CloseDS4Windows(bool forceClose = false)
        {
            if (!enabled)
            {
                Logger.Debug("DS4Windows is disabled, skipping...");
                return;
            }

            Logger.Info("Closing DS4Windows");

            StopWatcher();

            if (forceClose)
            {
                ProcessUtils.CloseProcess(ds4WindowsConfig.ProcessName, true);
                Logger.Debug("DS4Windows is forcefully closed");

                return;
            }

            ProcessUtils.StartProcess(FSUtils.GetAbsolutePath(ds4WindowsConfig.Path), "-command shutdown", ProcessWindowStyle.Hidden, true);
            Logger.Debug("DS4Windows is gracefully stopped");
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
