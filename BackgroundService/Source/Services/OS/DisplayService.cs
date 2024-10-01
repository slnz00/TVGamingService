using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using BackgroundService.Source.Services.State.Components;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static Core.WinAPI.DisplayAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class DisplayService : Service
    {
        private class MonitorStatus
        {
            private readonly object threadLock = new object();
            private long? turnedOnAt = null;

            public volatile bool IsTurnedOn = true;
            public long? TurnedOnAt
            {
                get { lock (threadLock) return turnedOnAt; }
                set { lock (threadLock) turnedOnAt = value; }
            }

            public void Update()
            {
                lock (threadLock)
                {
                    bool newStatus = PowerManager.IsMonitorOn;
                    bool isMonitorTurnedOn = !IsTurnedOn && newStatus;

                    if (isMonitorTurnedOn)
                    {
                        TurnedOnAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    }
                    IsTurnedOn = newStatus;
                }
            }

            public long GetTimeUntilMonitorTurnsOn()
            {
                lock (threadLock)
                {
                    if (TurnedOnAt == null) {
                        return 0;
                    }

                    long monitorTurnOnTime = 6000;

                    long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    long turnOnTime = (long)TurnedOnAt + monitorTurnOnTime - now;

                    return turnOnTime > 0 ? turnOnTime : 0;
                }
            }
        }

        private readonly MonitorStatus monitorStatus = new MonitorStatus();

        public DisplayService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            PowerManager.IsMonitorOnChanged += new EventHandler((object sender, EventArgs e) =>
            {
                monitorStatus.Update();
            });
        }

        public bool SwitchToDisplay(string devicePath, string fullName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(devicePath))
                {
                    Logger.Info("No display device path has been provided for switching to display, skipping...");

                    return true;
                }

                Logger.Info($"Switching to display: {fullName}");

                var settings = GetDisplaySettings(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ALL_PATHS);
                var defaultSettings = settings.Clone();
                var availableDisplays = GetAvailableDisplays(settings);

                var displayByPath = GetDisplayByDevicePath(availableDisplays, devicePath);
                var displayByName = GetDisplayByFullName(availableDisplays, fullName);
                var display = displayByPath ?? displayByName;

                if (display == null)
                {
                    Logger.Error($"Failed to switch to display ({fullName}): Display is unavailable");

                    return false;
                }

                var source = GetAvailableSourceForDisplay(settings, display);

                Logger.Debug($"Assigning source for display: {source.id}");

                settings.Reset();
                settings.ActivatePath(source.id, display.TargetInfo.id);

                defaultSettings.ResetPaths();
                defaultSettings.ActivatePath(source.id, display.TargetInfo.id);

                try
                {
                    SaveDisplaySettings(settings);
                }
                catch
                {
                    SaveDisplaySettings(defaultSettings);
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to switch to display ({fullName}): {ex}");

                return false;
            }
        }

        public bool BackupDisplaySettings()
        {
            try
            {
                Logger.Info("Creating backup snapshot from current display settings");

                var settings = GetDisplaySettings(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS);
                var displays = settings.Paths
                    .Select(p => GetDisplayDeviceFromTargetInfo(p.targetInfo))
                    .ToArray();

                var displayPaths = displays.Select(dp => $"'{dp.NameInfo.monitorDevicePath}'").ToArray();

                Logger.Debug($"Displays queried for snapshot: {string.Join(", ", displayPaths)}");

                var snapshot = new DisplaySettingsSnapshot(settings, displays);

                Services.State.Set(States.DisplaySettingsSnapshot, snapshot);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to backup display settings: {ex}");

                return false;
            }
        }

        public bool RestoreDisplaySettings()
        {
            try
            {
                Logger.Info("Restoring display settings from snapshot");

                var snapshot = Services.State.Get<DisplaySettingsSnapshot>(States.DisplaySettingsSnapshot);
                var settings = GetDisplaySettings(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ALL_PATHS);
                var availableDisplays = GetAvailableDisplays(settings);

                if (!ValidateSnapshot(snapshot, availableDisplays))
                {
                    Logger.Error("Failed to restore display settings: Invalid snapshot");

                    return false;
                }

                settings.Reset();

                foreach (var snapshotPath in snapshot.Settings.Paths)
                {
                    var snapshotDisplay = snapshot.GetDisplayForPath(snapshotPath);
                    var sourceId = snapshotPath.sourceInfo.id;
                    var targetId = snapshotPath.targetInfo.id;

                    snapshot.Settings.GetModesForPath(
                        sourceId,
                        targetId,
                        out var snapshotSourceMode,
                        out var snapshotTargetMode
                    );

                    settings.ActivatePath(sourceId, targetId);
                    settings.SetModesForPath(sourceId, targetId, snapshotSourceMode, snapshotTargetMode);
                }

                SaveDisplaySettings(settings);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to restore display settings: {ex}");

                return false;
            }
        }

        private bool ValidateSnapshot(DisplaySettingsSnapshot snapshot, DisplayDevice[] availableDisplays)
        {
            if (snapshot == null)
            {
                Logger.Error($"Failed to validate display settings snapshot: Snapshot is missing");

                return false;
            }

            foreach (var snapshotPath in snapshot.Settings.Paths)
            {
                var snapshotDisplay = snapshot.GetDisplayForPath(snapshotPath);
                var display = GetDisplayByDevicePath(availableDisplays, snapshotDisplay.NameInfo.monitorDevicePath);
                var devicePath = snapshotDisplay.NameInfo.monitorDevicePath;
                var displayUnavailable = display == null || snapshotDisplay.TargetInfo.id != display.TargetInfo.id;

                if (displayUnavailable)
                {
                    Logger.Error($"Failed to validate display settings snapshot: Display is unavailable - {devicePath}");

                    return false;
                }
            }

            return true;
        }

        private DisplaySettings GetDisplaySettings(QUERY_DISPLAY_CONFIG_FLAGS flags)
        {
            EnsureDisplaysAreTurnedOn();

            QueryDisplayConfig(
              flags,
              out var pathsCount,
              out var paths,
              out var modesCount,
              out var modes,
              out var currentTopologyId
            );

            return new DisplaySettings(paths, pathsCount, modes, modesCount, currentTopologyId);
        }

        private void SaveDisplaySettings(DisplaySettings settings)
        {
            var paths = settings.Paths.ToArray();
            var modes = settings.Modes.ToArray();

            var baseFlags = modes.Length == 0 ?
                SET_DISPLAY_CONFIG_FLAGS.SDC_TOPOLOGY_SUPPLIED | SET_DISPLAY_CONFIG_FLAGS.SDC_ALLOW_PATH_ORDER_CHANGES :
                SET_DISPLAY_CONFIG_FLAGS.SDC_USE_SUPPLIED_DISPLAY_CONFIG | SET_DISPLAY_CONFIG_FLAGS.SDC_SAVE_TO_DATABASE | SET_DISPLAY_CONFIG_FLAGS.SDC_ALLOW_CHANGES;

            EnsureDisplaysAreTurnedOn();

            SetDisplayConfig((uint)paths.Length, ref paths, (uint)modes.Length, ref modes, (
                baseFlags | SET_DISPLAY_CONFIG_FLAGS.SDC_APPLY
            ));
        }

        private void ValidateDisplaySettings(DisplaySettings settings)
        {
            var paths = settings.Paths.ToArray();
            var modes = settings.Modes.ToArray();

            EnsureDisplaysAreTurnedOn();

            SetDisplayConfig((uint)paths.Length, ref paths, (uint)modes.Length, ref modes, (
                SET_DISPLAY_CONFIG_FLAGS.SDC_VALIDATE | SET_DISPLAY_CONFIG_FLAGS.SDC_USE_SUPPLIED_DISPLAY_CONFIG | SET_DISPLAY_CONFIG_FLAGS.SDC_ALLOW_CHANGES
            ));
        }

        private DISPLAYCONFIG_PATH_SOURCE_INFO GetAvailableSourceForDisplay(DisplaySettings settings, DisplayDevice display)
        {
            for (int pathIndex = 0; pathIndex < settings.Paths.Count; pathIndex++)
            {
                var currentPath = settings.Paths[pathIndex];

                if (currentPath.targetInfo.id != display.TargetInfo.id)
                {
                    continue;
                }

                try
                {
                    var currentSettings = settings.Clone();
                    currentSettings.ActivatePath(currentPath.sourceInfo.id, currentPath.targetInfo.id);

                    ValidateDisplaySettings(currentSettings);

                    return currentPath.sourceInfo;
                }
                catch { }
            }

            throw new InvalidOperationException("Display does not have a valid source");
        }
        private DisplayDevice GetDisplayByDevicePath(DisplayDevice[] displays, string devicePath)
        {
            return displays
                    .Where(dp => dp.NameInfo.monitorDevicePath == devicePath)
                    .FirstOrDefault();
        }

        private DisplayDevice GetDisplayByFullName(DisplayDevice[] displays, string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return null;
            }

            var results = displays
                .Where(dp => dp.FullName == fullName)
                .ToArray();

            if (results.Length > 1)
            {
                Logger.Warn($"Multiple display devices exist under the same name: {fullName}");
            }
            if (results.Length != 1)
            {
                return null;
            }

            return results[0];
        }

        private DisplayDevice[] GetAvailableDisplays(DisplaySettings settings)
        {
            return settings.Paths
                .Select(path => path.targetInfo)
                .Where(targetInfo => targetInfo.targetAvailable == 1)
                .GroupBy(targetInfo => targetInfo.id)
                .Select(group => group.First())
                .Select(GetDisplayDeviceFromTargetInfo)
                .ToArray();
        }

        private DisplayDevice GetDisplayDeviceFromTargetInfo(DISPLAYCONFIG_PATH_TARGET_INFO targetInfo)
        {
            var preferredMode = new DISPLAYCONFIG_TARGET_PREFERRED_MODE
            {
                header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    id = targetInfo.id,
                    adapterId = targetInfo.adapterId,
                    size = Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_PREFERRED_MODE)),
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE,
                }
            };

            var nameInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME
            {
                header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
                {
                    adapterId = targetInfo.adapterId,
                    id = targetInfo.id,
                    size = Marshal.SizeOf(typeof(DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                    type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME,
                }
            };

            DisplayConfigGetDeviceInfo(ref nameInfo);
            DisplayConfigGetDeviceInfo(ref preferredMode);

            return new DisplayDevice(targetInfo, nameInfo, preferredMode);
        }

        private void EnsureDisplaysAreTurnedOn()
        {
            if (!monitorStatus.IsTurnedOn)
            {
                ushort VK_LEFT_ALT = 0xA4;

                Services.OS.Input.PressKey(VK_LEFT_ALT);
                Thread.Sleep(50);
            }

            var waitAmount = monitorStatus.GetTimeUntilMonitorTurnsOn();

            if (waitAmount != 0) {
                Thread.Sleep((int)waitAmount);
            }
        }
    }
}
