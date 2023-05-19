using BackgroundService.Source.Controllers.BackupController.Components;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using Core.Components;
using Core.Utils;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BackgroundService.Source.Controllers.BackupController
{
    internal class BackupController
    {
        private const int EVENT_LOOP_DELAY = 1000;

        private ManagedTask eventLoopTask = null;
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
            var backupConfigs = GetBackupConfigs();
            bool haveBackupsConfigured = backupConfigs == null || backupConfigs.Count == 0;
            if (!haveBackupsConfigured)
            {
                Logger.Info("No backups were configured, skipping initialization...");

                return;
            }

            SetupManagers();
            StartEventLoop();
        }

        private void StartEventLoop()
        {
            if (eventLoopTask != null)
            {
                Logger.Error("StartEventLoop method called multiple times, event loop is already started...");

                return;
            }

            eventLoopTask = ManagedTask.Run(async (ctx) => {
                while (true) {
                    RunBackups();

                    await ctx.Delay(EVENT_LOOP_DELAY);
                }
            });
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

        private void RunBackups() {
            managers.ForEach(manager =>
            {
                if (manager.ShouldRunBackup()) {
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
