using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using Core.Utils;

using static Core.WinAPI.VirtualDesktop.VirtualDesktopAPIW11;

namespace BackgroundService.Source.Services.OS
{
    internal class DesktopServiceW11 : DesktopService
    {
        private IVirtualDesktopManagerInternal VirtualDesktopManagerInternal;
        private IVirtualDesktopManagerInternalOld VirtualDesktopManagerInternal_Old;
        private IApplicationViewCollection ApplicationViewCollection;

        private double buildNumber = OSUtils.GetCurrentWindowsBuildNumber();

        private bool OutdatedVersion => buildNumber < 22631.3085;

        public DesktopServiceW11(ServiceProvider services) : base(services)
        {
            var shell = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell));

            ApplicationViewCollection = (IApplicationViewCollection)shell.QueryService(typeof(IApplicationViewCollection).GUID, typeof(IApplicationViewCollection).GUID);


            if (OutdatedVersion)
            {
                VirtualDesktopManagerInternal_Old = (IVirtualDesktopManagerInternalOld)shell.QueryService(
                    Guids.CLSID_VirtualDesktopManagerInternal,
                    typeof(IVirtualDesktopManagerInternalOld).GUID
                );
            }
            else
            {
                VirtualDesktopManagerInternal = (IVirtualDesktopManagerInternal)shell.QueryService(
                    Guids.CLSID_VirtualDesktopManagerInternal,
                    typeof(IVirtualDesktopManagerInternal).GUID
                );
            }
        }

        public override void CreateAndSwitchToDesktop(string desktopName)
        {
            Logger.Info($"Switching desktop to: {desktopName}");

            if (OutdatedVersion)
            {
                var desktop = VirtualDesktopManagerInternal_Old.CreateDesktop();

                VirtualDesktopManagerInternal_Old.SetDesktopName(desktop, desktopName);
                VirtualDesktopManagerInternal_Old.SwitchDesktopWithAnimation(desktop);
            }
            else
            {
                var desktop = VirtualDesktopManagerInternal.CreateDesktop();

                VirtualDesktopManagerInternal.SetDesktopName(desktop, desktopName);
                VirtualDesktopManagerInternal.SwitchDesktopWithAnimation(desktop);
            }
        }

        public override void RemoveDesktop(string desktopName)
        {
            Logger.Info($"Removing desktop: {desktopName}");

            var allDesktops = GetAllDesktops();

            var selectedDesktop = allDesktops.FirstOrDefault((d) => d.GetName() == desktopName);
            if (selectedDesktop == null)
            {
                Logger.Debug($"Desktop does not exist: {desktopName}");

                return;
            }

            var fallbackDesktop = allDesktops[0];

            if (OutdatedVersion)
            {
                VirtualDesktopManagerInternal_Old.RemoveDesktop(selectedDesktop, fallbackDesktop);
            }
            else
            {
                VirtualDesktopManagerInternal.RemoveDesktop(selectedDesktop, fallbackDesktop);
            }
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

            if (OutdatedVersion)
            {
                var currentDesktop = VirtualDesktopManagerInternal_Old.GetCurrentDesktop();

                VirtualDesktopManagerInternal_Old.SetDesktopWallpaper(currentDesktop, fullWallpaperPath);
            }
            else
            {
                var currentDesktop = VirtualDesktopManagerInternal.GetCurrentDesktop();

                VirtualDesktopManagerInternal.SetDesktopWallpaper(currentDesktop, fullWallpaperPath);
            }
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
            if (OutdatedVersion)
            {
                return VirtualDesktopManagerInternal_Old.GetCurrentDesktop().GetName();
            }

            return VirtualDesktopManagerInternal.GetCurrentDesktop().GetName();
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

                    var isOnDesktop = desktop != null && desktop.GetName() == desktopName;
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
            IObjectArray desktopsObj;

            if (OutdatedVersion)
            {
                VirtualDesktopManagerInternal_Old.GetDesktops(out desktopsObj);
            }
            else
            {
                VirtualDesktopManagerInternal.GetDesktops(out desktopsObj);
            }

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
