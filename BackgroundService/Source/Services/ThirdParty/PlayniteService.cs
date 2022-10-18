using System;
using System.Diagnostics;
using System.Threading;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using Core.Utils;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class PlayniteService : Service
    {
        public class PlayniteEvents
        {
            public Action onPlayniteClosed = null;
        }

        private readonly object threadLock = new object();
        private Thread watcherThread = null;
        private AppConfig playniteConfig;

        public bool IsPlayniteOpen => watcherThread != null && watcherThread.IsAlive;

        public PlayniteService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            playniteConfig = Services.Config.GetConfig().ThirdParty.Playnite;
        }

        public void OpenPlaynite(PlayniteEvents events = null)
        {
            Logger.Debug("Opening Playnite");

            var config = Services.Config.GetConfig();

            ProcessUtils.StartProcess(playniteConfig.Path);
            StartPlayniteWatcher(events);
        }

        public void ClosePlaynite()
        {
            Logger.Debug("Closing Playnite");

            var config = Services.Config.GetConfig();

            StopPlayniteWatcher();
            ProcessUtils.CloseProcess(playniteConfig.ProcessName);
        }

        private void StartPlayniteWatcher(PlayniteEvents events)
        {
            var config = Services.Config.GetConfig();

            Logger.Debug("Starting Playnite watcher");

            StopPlayniteWatcher();

            watcherThread = new Thread(() =>
            {
                while (true)
                {
                    lock (threadLock)
                    {
                        if (Process.GetProcessesByName(playniteConfig.ProcessName).Length == 0)
                        {
                            events?.onPlayniteClosed?.Invoke();
                            break;
                        }
                    }

                    Thread.Sleep(1000);
                }
            });

            watcherThread.Start();
        }

        private void StopPlayniteWatcher()
        {
            if (IsPlayniteOpen)
            {
                Logger.Debug("Stopping Playnite watcher");

                watcherThread.Abort();
            }
        }
    }
}
