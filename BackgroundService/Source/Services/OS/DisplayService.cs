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
        private readonly DisplaysStatus displaysStatus = new DisplaysStatus();

        public DisplayService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            PowerManager.IsMonitorOnChanged += new EventHandler((object sender, EventArgs e) =>
            {
                try
                {
                    displaysStatus.Update(
                        () => GetDisplaySettings(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS)
                    );
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to update displays status: {ex}");
                }
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

                EnsureDisplaysAreTurnedOn();

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

                settings.ResetPaths();
                settings.ActivatePath(source.id, display.TargetInfo.id, display.TargetInfo.adapterId);

                defaultSettings.Reset();
                defaultSettings.ActivatePath(source.id, display.TargetInfo.id, display.TargetInfo.adapterId);
                defaultSettings.KeepOnlyActivePaths();
                
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

                EnsureDisplaysAreTurnedOn();

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

                EnsureDisplaysAreTurnedOn();

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
                    var adapterId = snapshotPath.targetInfo.adapterId;

                    snapshot.Settings.GetModesForPath(
                        sourceId,
                        targetId,
                        adapterId,
                        out var snapshotSourceMode,
                        out var snapshotTargetMode
                    );

                    settings.ActivatePath(sourceId, targetId, adapterId);
                    settings.SetModesForPath(sourceId, targetId, adapterId, snapshotSourceMode, snapshotTargetMode);
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

            SetDisplayConfig((uint)paths.Length, paths, (uint)modes.Length, modes.Length > 0 ? modes : null, (
                baseFlags | SET_DISPLAY_CONFIG_FLAGS.SDC_APPLY
            ));
        }

        private void ValidateDisplaySettings(DisplaySettings settings)
        {
            var paths = settings.Paths.ToArray();
            var modes = settings.Modes.ToArray();

            SetDisplayConfig((uint)paths.Length, paths, (uint)modes.Length, modes.Length > 0 ? modes : null, (
                SET_DISPLAY_CONFIG_FLAGS.SDC_VALIDATE | SET_DISPLAY_CONFIG_FLAGS.SDC_USE_SUPPLIED_DISPLAY_CONFIG | SET_DISPLAY_CONFIG_FLAGS.SDC_ALLOW_CHANGES
            ));
        }

        private DISPLAYCONFIG_PATH_SOURCE_INFO GetAvailableSourceForDisplay(DisplaySettings settings, DisplayDevice display)
        {
            for (int pathIndex = 0; pathIndex < settings.Paths.Count; pathIndex++)
            {
                var currentPath = settings.Paths[pathIndex];

                var isSameTarget =
                   currentPath.targetInfo.id == display.TargetInfo.id &&
                   currentPath.targetInfo.adapterId.LowPart == display.TargetInfo.adapterId.LowPart &&
                   currentPath.targetInfo.adapterId.HighPart == display.TargetInfo.adapterId.HighPart;

                if (!isSameTarget || currentPath.targetInfo.targetAvailable == 0)
                {
                    continue;
                }

                try
                {
                    var currentSettings = settings.Clone();
                    currentSettings.ActivatePath(currentPath.sourceInfo.id, currentPath.targetInfo.id, currentPath.targetInfo.adapterId);

                    ValidateDisplaySettings(currentSettings);

                    return currentPath.sourceInfo;
                }
                catch { }
            }

            throw new InvalidOperationException("Display does not have a valid source");
        }

        private DisplayDevice GetDisplayByDevicePath(DisplayDevice[] displays, string devicePath)
        {
            if (string.IsNullOrWhiteSpace(devicePath))
            {
                return null;
            }

            return displays
                .Where(dp => string.Equals(
                    dp.NameInfo.monitorDevicePath,
                    devicePath,
                    StringComparison.OrdinalIgnoreCase))
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
                .Where(path => path.targetInfo.targetAvailable == 1)
                .Select(path => path.targetInfo)
                .GroupBy(targetInfo => new
                {
                    targetInfo.adapterId.LowPart,
                    targetInfo.adapterId.HighPart,
                    targetInfo.id
                })
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
            var state = displaysStatus.GetState();

            if (!state.TurnedOn)
            {
                ushort VK_LEFT_ALT = 0xA4;

                Services.OS.Input.PressKey(VK_LEFT_ALT);
                Thread.Sleep(50);
            }

            state = displaysStatus.GetState();

            Func<long> now = () => DateTimeOffset.Now.ToUnixTimeMilliseconds();

            if (state.ReadyAt == null || now() >= state.ReadyAt)
            {
                return;
            }

            Thread.Sleep(2500);

            while (now() < state.ReadyAt)
            {
                var turnOnPaths = state.SettingWhenTurnedOn.GetPathMap();
                var currentPaths = GetDisplaySettings(QUERY_DISPLAY_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS).GetPathMap();
                var pathsChanged = turnOnPaths.Count != currentPaths.Count || turnOnPaths.Except(currentPaths).Any();

                if (!pathsChanged) {
                    break;
                }

                Thread.Sleep(250);
            }
        }
    }
}
