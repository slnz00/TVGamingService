using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.OS.Models;
using Core.Utils;
using Core.WinAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackgroundService.Source.Services.OS
{
    internal abstract class DesktopService : Service
    {
        public static DesktopService Create(ServiceProvider services)
        {
            string windowsVersion = OSUtils.GetCurrentWindowsVersionName();
            bool isWindows11 = OSUtils.IsWindows11(windowsVersion);

            if (isWindows11)
            {
                return new DesktopServiceW11(services);
            }

            return new DesktopServiceW10(services);
        }

        protected DesktopService(ServiceProvider services) : base(services) { }

        public abstract void CreateAndSwitchToDesktop(string desktopName);

        public abstract void RemoveDesktop(string desktopName);

        public abstract string GetCurrentDesktopName();

        public abstract Guid GetCurrentDesktopId();

        public abstract void ChangeWallpaper(string wallpaperPath);

        public abstract void ChangeWallpaperOnCurrentDesktop(string wallpaperPath);

        public abstract bool BackupWallpaperSettings();

        public abstract bool RestoreWallpaperSettings();

        public abstract List<WindowComponent> GetWindowsOnDesktop(Guid desktopId);

        public void ToggleIconsVisiblity(bool visible)
        {
            Logger.Info($"Toggling desktop icons visibility to: {visible}");

            var shellWindows = (ShellAPI.IShellWindows)new ShellAPI.ShellWindows();
            var serviceProvider = (ShellAPI.IServiceProvider)shellWindows.FindWindowSW(
                ShellAPI.CSIDL_DESKTOP,
                new object(),
                ShellAPI.ShellWindowTypeConstants.SWC_DESKTOP,
                out _,
                ShellAPI.ShellWindowFindWindowOptions.SWFO_NEEDDISPATCH
            );

            var desktopBrowser = (ShellAPI.IShellBrowser)serviceProvider.QueryService(
                ShellAPI.SID_STopLevelBrowser,
                typeof(ShellAPI.IShellBrowser).GUID
            );

            var folderView = (ShellAPI.IFolderView2)desktopBrowser.QueryActiveShellView();

            folderView.GetCurrentFolderFlags(out uint flags);

            BitUtils.SetBit(ref flags, ShellAPI.FWF_NOICONS, !visible);

            folderView.SetCurrentFolderFlags(ShellAPI.FWF_NOICONS, flags);
        }

        public void RestartExplorer()
        {
            var sessionKey = Guid.NewGuid().ToString();

            RestartManagerAPI.RmStartSession(out IntPtr session, 0, sessionKey);
            try
            {
                var explorerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");

                RestartManagerAPI.RmRegisterResources(session, 1, new[] { explorerPath }, 0, null, 0, null);
                RestartManagerAPI.RmShutdown(session, 0, null);
                RestartManagerAPI.RmRestart(session, 0, null);
            }
            finally
            {
                RestartManagerAPI.RmEndSession(session);
            }
        }
    }
}
