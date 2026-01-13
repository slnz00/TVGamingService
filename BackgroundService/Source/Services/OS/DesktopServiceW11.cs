using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using static Core.WinAPI.VirtualDesktop.VirtualDesktopAPIW11;
using static Core.WinAPI.WindowAPI;

namespace BackgroundService.Source.Services.OS
{
    internal class DesktopServiceW11 : DesktopService
    {
        private readonly IVirtualDesktopManager VirtualDesktopManager;
        private readonly IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
        private readonly IApplicationViewCollection ApplicationViewCollection;

        private readonly double buildNumber = OSUtils.GetCurrentWindowsBuildNumber();

        private bool OutdatedVersion => buildNumber < 22631.3085;

        public DesktopServiceW11(ServiceProvider services) : base(services)
        {
            if (OutdatedVersion)
            {
                throw new PlatformNotSupportedException(
                    "The VirtualDesktop implementation is not supported on this version of Windows. Please upgrade your operating system."
                );
            }

            var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));

            ApplicationViewCollection = (IApplicationViewCollection)shell.QueryService(typeof(IApplicationViewCollection).GUID, typeof(IApplicationViewCollection).GUID);

            VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(
                Guids.CLSID_VirtualDesktopManagerInternal,
                typeof(IVirtualDesktopManagerInternal).GUID
            );

            VirtualDesktopManager = (IVirtualDesktopManager)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_VirtualDesktopManager));
        }

        public override void CreateAndSwitchToDesktop(string desktopName)
        {
            Logger.Info($"Switching desktop to: {desktopName}");

            var desktop = VirtualDesktopManagerInternal.CreateDesktop();

            VirtualDesktopManagerInternal.SetDesktopName(desktop, desktopName);
            VirtualDesktopManagerInternal.SwitchDesktopWithAnimation(desktop);
        }

        public override void RemoveDesktop(string desktopName)
        {
            Logger.Info($"Removing desktop: {desktopName}");

            var allDesktops = GetAllDesktops();

            if (allDesktops.Count == 1)
            {
                Logger.Error($"Unable to remove desktop ({desktopName}), only one desktop exists.");

                return;
            }

            var desktopIndex = allDesktops.FindIndex((d) => d.GetName() == desktopName);
            var desktop = desktopIndex != -1 ? allDesktops[desktopIndex] : null;
            if (desktop == null)
            {
                Logger.Debug($"Desktop does not exist: {desktopName}");

                return;
            }

            var fallbackDesktop = desktopIndex == 0 ? allDesktops[1] : allDesktops[0];

            VirtualDesktopManagerInternal.RemoveDesktop(desktop, fallbackDesktop);
        }

        public override void ChangeWallpaper(string wallpaperPath)
        {
            throw new NotSupportedException($"Not supported on Windows 11, use {nameof(ChangeWallpaperOnCurrentDesktop)} method instead");
        }

        public override void ChangeWallpaperOnCurrentDesktop(string wallpaperPath)
        {
            if (string.IsNullOrEmpty(wallpaperPath))
            {
                Logger.Debug("Provided wallpaper path is empty, skipping...");

                return;
            }

            Logger.Info($"Changing desktop wallpaper to: {wallpaperPath}");

            var fullWallpaperPath = FSUtils.GetAbsolutePath(wallpaperPath);
            var currentDesktop = VirtualDesktopManagerInternal.GetCurrentDesktop();

            VirtualDesktopManagerInternal.SetDesktopWallpaper(currentDesktop, fullWallpaperPath);
        }

        public override bool BackupWallpaperSettings()
        {
            throw new NotSupportedException($"Not supported on Windows 11");
        }

        public override bool RestoreWallpaperSettings()
        {
            throw new NotSupportedException($"Not supported on Windows 11");
        }

        public override string GetCurrentDesktopName()
        {
            return VirtualDesktopManagerInternal.GetCurrentDesktop().GetName();
        }

        public override Guid GetCurrentDesktopId()
        {
            return VirtualDesktopManagerInternal.GetCurrentDesktop().GetId();
        }

        public override List<WindowComponent> GetWindowsOnDesktop(Guid desktopId)
        {
            try
            {
                var allWindowHandles = new List<IntPtr>();

                EnumWindows((hWnd, lParam) =>
                {
                    if (IsWindowVisible(hWnd) && GetWindowTextLength(hWnd) != 0)
                    {
                        allWindowHandles.Add(hWnd);
                    }

                    return true;
                }, IntPtr.Zero);

                return allWindowHandles
                    .Where(hWnd => desktopId.CompareTo(GetDesktopIdForWindow(hWnd)) == 0)
                    .Select(hWnd => new WindowComponent("Window", hWnd))
                    .ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to get windows on desktop (id: {desktopId}): {ex}");

                return new List<WindowComponent>();
            }
        }

        private List<IVirtualDesktop> GetAllDesktops()
        {
            IObjectArray desktopsObj;

            VirtualDesktopManagerInternal.GetDesktops(out desktopsObj);

            return CastAndReleaseObjectArray<IVirtualDesktop>(desktopsObj);
        }

        private Guid GetDesktopIdForWindow(IntPtr windowHandle)
        {
            try
            {
                return VirtualDesktopManager.GetWindowDesktopId(windowHandle);
            }
            catch
            {
                return Guid.Empty;
            }
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
    }
}
