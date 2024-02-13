using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using Core.Utils;
using Microsoft.Win32;
using BackgroundService.Source.Services.State.Components;

using static Core.WinAPI.VirtualDesktop.VirtualDesktopAPIW10;
using static Core.WinAPI.DesktopAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class DesktopServiceW10 : DesktopService
    {
        private static readonly string VIRTUAL_DESKTOP_PATH = InternalSettings.PATH_VIRTUAL_DESKTOP_W10;

        private IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
        private IApplicationViewCollection ApplicationViewCollection;

        public DesktopServiceW10(ServiceProvider services) : base(services)
        {
            var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));

            VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(Guids.CLSID_VirtualDesktopManagerInternal, typeof(IVirtualDesktopManagerInternal).GUID);
            ApplicationViewCollection = (IApplicationViewCollection)shell.QueryService(typeof(IApplicationViewCollection).GUID, typeof(IApplicationViewCollection).GUID);
        }

        public override void CreateAndSwitchToDesktop(string desktopName)
        {
            Logger.Info($"Switching desktop to: {desktopName}");

            ExecVirtualDesktopBinary($"/New /Name:{desktopName} /Switch");
        }

        public override void RemoveDesktop(string desktopName)
        {
            Logger.Info($"Removing desktop: {desktopName}");

            ExecVirtualDesktopBinary($"/Remove:{desktopName}");
        }

        public override void ChangeWallpaper(string wallpaperPath)
        {
            if (string.IsNullOrWhiteSpace(wallpaperPath))
            {
                Logger.Debug("Change wallpaper: No wallpaper path has been specified, skipping...");

                return;
            }

            Logger.Info($"Changing wallpaper to: {wallpaperPath}");

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(DESKTOP_REGISTRY, true))
            {
                // Fill desktop:
                key.SetValue("WallpaperStyle", "10");
                key.SetValue("TileWallpaper", "0");

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wallpaperPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }

        public override void ChangeWallpaperOnCurrentDesktop(string wallpaperPath)
        {
            throw new NotSupportedException($"Not supported on Windows 10, use {nameof(ChangeWallpaper)} method instead");
        }

        public override bool BackupWallpaperSettings()
        {
            try
            {
                Logger.Info("Creating backup snapshot from current wallpaper settings");

                var snapshot = new WallpaperSettingsSnapshot();

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(DESKTOP_REGISTRY, false))
                {
                    snapshot.WallpaperStyle = (string)key.GetValue("WallpaperStyle");
                    snapshot.TileWallpaper = (string)key.GetValue("TileWallpaper");
                    snapshot.WallpaperPath = (string)key.GetValue("WallPaper");
                }

                Services.State.Set(States.WallpaperSettingsSnapshot, snapshot);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to backup wallpaper settings: {ex}");

                return false;
            }

            return true;
        }

        public override bool RestoreWallpaperSettings()
        {
            try
            {
                Logger.Info("Restoring wallpaper settings from snapshot");

                var snapshot = Services.State.Get<WallpaperSettingsSnapshot>(States.WallpaperSettingsSnapshot);

                if (snapshot == null)
                {
                    Logger.Error("Failed to restore wallpaper settings: Snapshot not found in state");

                    return false;
                }

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(DESKTOP_REGISTRY, true))
                {
                    key.SetValue("WallpaperStyle", snapshot.WallpaperStyle);
                    key.SetValue("TileWallpaper", snapshot.TileWallpaper);
                }

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, snapshot.WallpaperPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to restore wallpaper settings: {ex}");

                return false;
            }

            return true;
        }

        public override string GetCurrentDesktopName()
        {
            var currentDesktop = VirtualDesktopManagerInternal.GetCurrentDesktop();

            return GetDesktopName(currentDesktop);
        }

        public override List<WindowComponent> GetWindowsOnDesktop(string desktopName)
        {
            try
            {
                var views = GetAllApplicationViews();
                var desktops = GetAllDesktops();
                var windows = new List<WindowComponent>();

                views.ForEach(view =>
                {
                    view.GetThumbnailWindow(out var windowHandle);
                    view.GetVirtualDesktopId(out var desktopId);

                    var desktop = desktops.Find(d => d.GetId().CompareTo(desktopId) == 0);

                    var isOnDesktop = desktop != null && GetDesktopName(desktop) == desktopName;
                    var isVisible = IsViewVisible(view);

                    if (isVisible && isOnDesktop)
                    {
                        windows.Add(new WindowComponent("Window", windowHandle));
                    }
                });

                return windows;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get windows on desktop (name: {desktopName}): {ex}");
            }

            return new List<WindowComponent>();
        }

        private List<IVirtualDesktop> GetAllDesktops()
        {
            VirtualDesktopManagerInternal.GetDesktops(out IObjectArray desktopsObj);

            return CastAndReleaseObjectArray<IVirtualDesktop>(desktopsObj);
        }

        private List<IApplicationView> GetAllApplicationViews()
        {
            ApplicationViewCollection.GetViews(out IObjectArray viewsObj);

            return CastAndReleaseObjectArray<IApplicationView>(viewsObj);
        }

        private List<T> CastAndReleaseObjectArray<T>(IObjectArray array)
        {
            try
            {
                array.GetCount(out int count);
                var list = new List<T>(count);

                for (int index = 0; index < count; index++)
                {
                    array.GetAt(index, typeof(T).GUID, out object value);

                    list.Add((T)value);
                }

                return list;
            }
            finally
            {
                Marshal.ReleaseComObject(array);
            }
        }

        private string GetDesktopName(IVirtualDesktop desktop)
        {
            var desktopId = desktop.GetId();

            try
            {
                string registryKey = @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\VirtualDesktops\Desktops\{" + desktopId.ToString() + "}";

                object registryValue = Registry.GetValue(registryKey, "Name", null);
                return registryValue != null ? (string)registryValue : "";
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get desktop name for [GUID: {desktopId}]: {ex}");
                return "";
            }
        }

        private bool IsViewVisible(IApplicationView view)
        {
            try
            {
                view.GetVisibility(out var visibility);
                return visibility == 1;
            }
            catch
            {
                Logger.Warn("Failed to get visibility for view");
                return false;
            }
        }

        private void ExecVirtualDesktopBinary(string args)
        {
            Logger.Debug($"Exec virtual desktop, args: {args}");

            ProcessUtils.StartProcess(VIRTUAL_DESKTOP_PATH, args, ProcessWindowStyle.Hidden, true);
        }
    }
}
