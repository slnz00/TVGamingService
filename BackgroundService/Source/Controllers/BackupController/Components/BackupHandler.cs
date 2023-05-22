using BackgroundService.Source.Providers;
using Core.Utils;
using System;
using System.IO;

namespace BackgroundService.Source.Controllers.BackupController.Components
{
    internal class BackupHandler
    {
        private readonly int backupAmount;
        private readonly string originalPath;
        private readonly string backupBasePath;

        private LoggerProvider Logger { get; set; }

        public BackupHandler(string backupName, int backupAmount, string backupBasePath, string originalPath)
        {
            this.backupAmount = backupAmount;
            this.backupBasePath = backupBasePath;
            this.originalPath = originalPath;

            Logger = new LoggerProvider($"{GetType().Name}:{backupName}");
        }

        public void Backup()
        {
            try
            {
                string backupPath = SetupBackupDirectory();

                FSUtils.CopyDirectory(originalPath, backupPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"Backup failed: {ex}");
            }
        }

        private string SetupBackupDirectory()
        {
            FSUtils.EnsureDirectory(backupBasePath);
            ShiftBackupSlots();

            return GetBackupSlotPath(1);
        }

        private void ShiftBackupSlots()
        {
            string lastSlotPath = GetBackupSlotPath(backupAmount);
            bool allSlotsAreUsed = Directory.Exists(lastSlotPath);
            if (allSlotsAreUsed)
            {
                Directory.Delete(lastSlotPath, true);
            }

            for (int i = backupAmount - 1; i >= 1; i--)
            {
                string currentSlotPath = GetBackupSlotPath(i);
                string shiftedSlotPath = GetBackupSlotPath(i + 1);

                if (Directory.Exists(currentSlotPath))
                {
                    Directory.Move(currentSlotPath, shiftedSlotPath);
                }
            }
        }

        private string GetBackupSlotPath(int slot)
        {
            string paddedSlot = slot.ToString().PadLeft(3, '0');

            return FSUtils.JoinPaths(backupBasePath, paddedSlot);
        }
    }
}
