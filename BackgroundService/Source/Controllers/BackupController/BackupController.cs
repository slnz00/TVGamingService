using BackgroundService.Source.Controllers.BackupController.Components;
using BackgroundService.Source.Providers;
using Core.Components;
using Core.Configs;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BackgroundService.Source.Controllers.BackupController
{
    internal class BackupController
    {
        private const int EVENT_LOOP_DELAY = 1000;

        private readonly object threadLock = new object();

        private ManagedTask eventLoop = null;
        private List<BackupManager> managers = null;

        private ServiceProvider Services { get; set; }
        private LoggerProvider Logger { get; set; }

        public BackupController(ServiceProvider services)
        {
            Services = services;
            Logger = new LoggerProvider(GetType().Name);
        }

        public void Initialize()
        {
            LoadManagers();
            StartEventLoop();

            Services.Config.ConfigWatcher.OnChanged(() =>
            {
                lock (threadLock) {
                    StopEventLoop();
                    ClearState();
                    LoadManagers();
                    StartEventLoop();
                }
            });
        }

        private void ClearState()
        {
            eventLoop = null;
            managers = null;
        }

        private void LoadManagers()
        {
            var backupConfigs = GetBackupConfigs();

            if (backupConfigs?.Count == 0)
            {
                Logger.Info("No backups were configured, skipping initialization...");

                return;
            }

            SetupManagers();
        }

        private void StartEventLoop()
        {
            if (eventLoop != null)
            {
                Logger.Error("StartEventLoop method called multiple times, event loop is already started...");

                return;
            }

            eventLoop = ManagedTask.Run(async (ctx) =>
            {
                while (true)
                {
                    RunBackups();

                    await ctx.Delay(EVENT_LOOP_DELAY);
                }
            });
        }

        private void StopEventLoop()
        {
            if (eventLoop != null) {
                eventLoop.Cancel();
            }
        }

        private void SetupManagers()
        {
            if (managers != null)
            {
                Logger.Error("SetupManagers method called multiple times, backup managers are already created...");

                return;
            }

            managers = GetBackupConfigs()
                .Select(backupConfig => CreateManagerFromConfig(backupConfig))
                .ToList();
        }

        private void RunBackups()
        {
            managers.ForEach(manager =>
            {
                if (manager.ShouldRunBackup())
                {
                    manager.RunBackup();
                }
            });
        }

        private BackupManager CreateManagerFromConfig(BackupConfig backupConfig)
        {
            string backupName = backupConfig.Name;
            string originalPath = Path.GetFullPath(backupConfig.Path);
            int backupAmount = backupConfig.Amount ?? InternalSettings.BACKUP_DEFAULT_AMOUNT;

            return new BackupManager(backupName, backupAmount, originalPath);
        }

        private List<BackupConfig> GetBackupConfigs()
        {
            var config = Services.Config.GetConfig();

            return config.Backups;
        }
    }
}
