using BackgroundService.Source.Controllers.BackupController.Components;
using BackgroundService.Source.Providers;
using Core.Components;
using Core.Configs;
using System;
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

        public bool Initialized { get; private set; } = false;

        public bool Running => eventLoop != null ? eventLoop.IsAlive : false;

        private ServiceProvider Services { get; set; }
        private LoggerProvider Logger { get; set; }

        public BackupController(ServiceProvider services)
        {
            Services = services;
            Logger = new LoggerProvider(GetType().Name);
        }

        public void Initialize()
        {
            lock (threadLock)
            {
                if (Initialized)
                {
                    return;
                }

                Services.Config.ConfigWatcher.OnChanged(() => Reload());

                Initialized = true;
            }
        }

        public void Start()
        {
            lock (threadLock)
            {
                if (Running)
                {
                    return;
                }

                if (!Initialized)
                {
                    Initialize();
                }

                LoadManagers();
                StartEventLoop();
            }
        }

        public void Stop()
        {
            lock (threadLock)
            {
                if (!Running)
                {
                    return;
                }

                StopEventLoop();
                DisposeManagers();
            }
        }

        public void Reload()
        {
            lock (threadLock)
            {
                if (Running)
                {
                    Stop();
                }

                Start();
            }
        }

        private void LoadManagers()
        {
            if (managers != null)
            {
                DisposeManagers();
            }

            managers = GetBackupConfigs()
                .Select(backupConfig => CreateManagerFromConfig(backupConfig))
                .ToList();
        }

        private void DisposeManagers()
        {
            if (managers == null)
            {
                return;
            }

            managers.ForEach(m =>
            {
                try
                {
                    m.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to dispose backup manager: {ex}");
                }
            });

            managers = null;
        }

        private void StartEventLoop()
        {
            if (Running)
            {
                Logger.Error($" Failed to start event loop for backup controller, event loop is already running");

                return;
            }

            eventLoop = ManagedTask.Run(async (ctx) =>
            {
                while (true)
                {
                    ctx.Lock(threadLock, () =>
                    {
                        RunBackups();
                    });

                    await ctx.Delay(EVENT_LOOP_DELAY);
                }
            });
        }

        private void StopEventLoop()
        {
            if (eventLoop != null)
            {
                eventLoop.Cancel();
            }
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
