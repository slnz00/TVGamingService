using Core;
using Core.Utils;
using System;

namespace BackgroundService.Source.Controllers.BackupController.Components
{
    internal class BackupManager : IDisposable
    {
        private const long SCHEDULE_DELAY = 1500;

        private readonly object threadLock = new object();

        public long? RunBackupAt { get; private set; }

        public string BackupName { get; private set; }

        public int BackupAmount { get; private set; }

        public string OriginalPath { get; private set; }

        public string BackupBasePath { get; private set; }

        public BackupWatcher Watcher { get; private set; }

        public BackupHandler Handler { get; private set; }

        public BackupManager(string backupName, int backupAmount, string originalPath)
        {
            BackupName = backupName;
            BackupAmount = backupAmount;
            OriginalPath = originalPath;
            BackupBasePath = FSUtils.JoinPaths(SharedSettings.Paths.DataBackups, backupName);
            Watcher = new BackupWatcher(BackupName, OriginalPath, () => ScheduleBackup());
            Handler = new BackupHandler(BackupName, BackupAmount, BackupBasePath, OriginalPath);
        }

        public void Dispose()
        {
            Watcher.Dispose();
        }

        public void RunBackup()
        {
            lock (threadLock)
            {
                RunBackupAt = null;

                Handler.Backup();
            }
        }

        public bool ShouldRunBackup()
        {
            lock (threadLock)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (RunBackupAt == null || RunBackupAt > now)
                {
                    return false;
                }

                return true;
            }
        }

        public void ScheduleBackup(long? time = null)
        {
            lock (threadLock)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (time == null)
                {
                    time = now + SCHEDULE_DELAY;
                }

                RunBackupAt = time;
            }
        }
    }
}
