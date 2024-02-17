using BackgroundService.Source.Common;
using BackgroundService.Source.Providers;
using Core.Configs;
using Core.Utils;
using System.Diagnostics;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class DS4WindowsService : Service
    {
        public bool Enabled => DS4WindowsConfig != null;

        private ProcessWatcher Watcher { get; set; } = null;

        private AppConfig DS4WindowsConfig => Services.Config.GetConfig().ThirdParty.DS4Windows;

        private readonly object threadLock = new object();

        public DS4WindowsService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            UpdateProcessWatcher();

            Services.Config.ConfigWatcher.OnChanged(() =>
            {
                UpdateProcessWatcher();
            });
        }

        public void OpenDS4Windows()
        {
            Logger.Info("Opening DS4Windows");

            if (!Enabled)
            {
                Logger.Debug("DS4Windows is disabled, skipping...");
                return;
            }

            var fullPath = FSUtils.GetAbsolutePath(DS4WindowsConfig.Path);

            ProcessUtils.StartProcess(fullPath);

            StartWatcher();
        }

        public void CloseDS4Windows(bool forceClose = false)
        {
            if (!Enabled)
            {
                Logger.Debug("DS4Windows is disabled, skipping...");
                return;
            }

            Logger.Info("Closing DS4Windows");

            StopWatcher();

            if (forceClose)
            {
                ProcessUtils.CloseProcess(DS4WindowsConfig.ProcessName, true);
                Logger.Debug("DS4Windows is forcefully closed");

                return;
            }

            ProcessUtils.StartProcess(FSUtils.GetAbsolutePath(DS4WindowsConfig.Path), "-command shutdown", ProcessWindowStyle.Hidden, true);
            Logger.Debug("DS4Windows is gracefully stopped");
        }

        private void UpdateProcessWatcher()
        {
            lock (threadLock)
            {
                if (Enabled && Watcher == null)
                {
                    Watcher = new ProcessWatcher(DS4WindowsConfig.ProcessName, new ProcessWatcher.Events
                    {
                        OnProcessClosed = () => ReopenOnUnexpectedExit()
                    });
                }
                else if (!Enabled && Watcher != null)
                {
                    Watcher.Stop();
                    Watcher = null;
                }
            }
        }

        private void ReopenOnUnexpectedExit()
        {
            Logger.Error("DS4Windows closed unexpectedly");

            OpenDS4Windows();
        }

        private void StartWatcher()
        {
            if (Watcher.IsWatcherRunning)
            {
                return;
            }

            bool alreadyRunning = Process.GetProcessesByName(DS4WindowsConfig.ProcessName).Length != 0;
            if (!alreadyRunning)
            {
                Logger.Error("Failed to start DS4Windows process watcher, DS4Windows is not running");
                return;
            }

            Watcher.Start();
        }

        private void StopWatcher()
        {
            if (!Watcher.IsWatcherRunning)
            {
                return;
            }

            Watcher.Stop();
        }
    }
}
