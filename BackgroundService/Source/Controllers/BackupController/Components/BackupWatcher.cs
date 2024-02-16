using BackgroundService.Source.Providers;
using System;
using System.IO;

namespace BackgroundService.Source.Controllers.BackupController.Components
{
    internal class BackupWatcher : IDisposable
    {
        private readonly string originalPath;
        private readonly Action backupScheduler;

        private FileSystemWatcher directoryWatcher;

        private LoggerProvider Logger { get; set; }

        public BackupWatcher(string backupName, string originalPath, Action backupScheduler)
        {
            this.originalPath = originalPath;
            this.backupScheduler = backupScheduler;

            Logger = new LoggerProvider($"{GetType().Name}:{backupName}");

            SetupDirectoryWatcher();
        }

        public void Dispose()
        {
            directoryWatcher.Dispose();
        }

        private void SetupDirectoryWatcher()
        {
            try
            {
                directoryWatcher = new FileSystemWatcher
                {
                    Path = originalPath,
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = "*.*",
                    EnableRaisingEvents = true,
                    IncludeSubdirectories = true
                };

                directoryWatcher.Changed += new FileSystemEventHandler((object source, FileSystemEventArgs e) =>
                {
                    backupScheduler.Invoke();
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to setup directory watcher: {ex}");
            }
        }
    }
}
