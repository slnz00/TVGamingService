using System;
using System.Diagnostics;
using System.Threading;
using TVGamingService.Source.Providers;
using TVGamingService.Source.Utils;

namespace TVGamingService.Source.Services.Apps
{
    internal class PlayniteService : BaseService
    {
        public class PlayniteEvents
        {
            public Action onPlayniteClosed = null;
        }

        private readonly object threadLock = new object();
        private Thread watcherThread = null;

        public PlayniteService(ServiceProvider services) : base(services) { }

        public bool IsPlayniteOpen => watcherThread != null && watcherThread.IsAlive;

        public void OpenPlaynite(PlayniteEvents events = null)
        {
            Logger.Debug("Opening Playnite");

            var config = Services.Config.GetConfig();

            ProcessUtils.StartProcess(config.Apps.Playnite.Path);
            StartPlayniteWatcher(events);
        }

        public void ClosePlaynite()
        {
            Logger.Debug("Closing Playnite");

            var config = Services.Config.GetConfig();

            StopPlayniteWatcher();
            ProcessUtils.CloseProcess(config.Apps.Playnite.ProcessName);
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
                        if (Process.GetProcessesByName(config.Apps.Playnite.ProcessName).Length == 0)
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
