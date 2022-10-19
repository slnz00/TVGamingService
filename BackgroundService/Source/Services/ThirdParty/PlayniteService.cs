using System;
using System.Diagnostics;
using System.IO;
using BackgroundService.Source.Common;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using Core.Utils;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class PlayniteService : Service
    {
        public class PlayniteEvents
        {
            public Action OnPlayniteClosed { get; set; } = null;
        }

        private ProcessWatcher watcher = null;
        private AppConfig playniteConfig;

        public bool IsPlayniteOpen => watcher != null && watcher.IsProcessOpen;

        public PlayniteService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            playniteConfig = Services.Config.GetConfig().ThirdParty.Playnite;
        }

        public void OpenPlaynite(PlayniteEvents events = null)
        {
            Logger.Debug("Opening Playnite");

            var playnitePath = Path.GetFullPath(playniteConfig.Path);
            var playniteDir = Path.GetDirectoryName(playnitePath);

            ProcessUtils.StartProcess(playnitePath, "", ProcessWindowStyle.Normal, false, (startInfo) =>
            {
                startInfo.WorkingDirectory = playniteDir;
            });

            StartWatcher(events);
        }

        public void ClosePlaynite()
        {
            Logger.Debug("Closing Playnite");

            StopWatcher();
            ProcessUtils.CloseProcess(playniteConfig.ProcessName);
        }

        private void StartWatcher(PlayniteEvents events)
        {
            StopWatcher();

            watcher = new ProcessWatcher(playniteConfig.ProcessName, new ProcessWatcher.Events
            {
                OnProcessClosed = events.OnPlayniteClosed
            });

            watcher.Start();
        }

        private void StopWatcher()
        {
            if (watcher == null)
            {
                return;
            }

            watcher.Stop();
            watcher = null;
        }
    }
}
