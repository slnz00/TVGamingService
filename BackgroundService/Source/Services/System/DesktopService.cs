using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using Core.SystemComponents;
using Core.Utils;
using Microsoft.Win32;

namespace BackgroundService.Source.Services.System
{
    internal class DesktopService : Service
    {
        private const int SPI_SETDESKWALLPAPER = 20;
        private const int SPIF_UPDATEINIFILE = 0x01;
        private const int SPIF_SENDWININICHANGE = 0x02;

        private const uint WM_ICONS_VISIBILITY_COMMAND = 0x111;

        private static readonly string VIRTUAL_DESKTOP_PATH = InternalSettings.PATH_VIRTUAL_DESKTOP;
        private static readonly string DESKTOP_REGISTRY = @"Control Panel\Desktop";

        enum GetWindowCommand : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        public DesktopService(ServiceProvider services) : base(services) { }

        public void CreateAndSwitchToDesktop(string desktopName)
        {
            Logger.Debug($"Switching desktop to: {desktopName}");

            ExecVirtualDesktop($"/New /Name:{desktopName} /Switch");
        }

        public void RemoveDesktop(string desktopName)
        {
            Logger.Debug($"Removing desktop: {desktopName}");

            ExecVirtualDesktop($"/Remove:{desktopName}");
        }

        public void ToggleIconsVisiblity(bool visible = false)
        {
            Logger.Debug("Toggling desktop icons visibility");

            var toggleDesktopCommand = new IntPtr(0x7402);
            IntPtr hWnd = GetWindow(FindWindow("Progman", "Program Manager"), GetWindowCommand.GW_CHILD);
            SendMessage(hWnd, WM_ICONS_VISIBILITY_COMMAND, toggleDesktopCommand, IntPtr.Zero);
        }

        public void RestartExplorer() {
            var sessionKey = Guid.NewGuid().ToString();

            RestartManagerApi.RmStartSession(out IntPtr session, 0, sessionKey);
            try
            {
                RestartManagerApi.RmRegisterResources(session, 1, new[] { Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe") }, 0, null, 0, null);
                RestartManagerApi.RmShutdown(session, 0, null);
                RestartManagerApi.RmRestart(session, 0, null);
            }
            finally
            {
                RestartManagerApi.RmEndSession(session);
            }
        }

        public void ChangeWallpaper(string wallpaperPath) {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(DESKTOP_REGISTRY, true)) {
                // Fill desktop:
                key.SetValue("WallpaperStyle", "10");
                key.SetValue("TileWallpaper", "0");

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, Path.GetFullPath(wallpaperPath), SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            }
        }

        private void ExecVirtualDesktop(string args)
        {
            Logger.Debug($"Exec virtual desktop, args: {args}");

            ProcessUtils.StartProcess(VIRTUAL_DESKTOP_PATH, args, ProcessWindowStyle.Hidden, true);
        }
    }
}
