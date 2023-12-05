﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.System.API;
using BackgroundService.Source.Services.System.Models;
using Core.Utils;
using Microsoft.Win32;

namespace BackgroundService.Source.Services.System
{
    internal abstract class DesktopService : Service
    {
        protected const int SPI_SETDESKWALLPAPER = 20;
        protected const int SPIF_UPDATEINIFILE = 0x01;
        protected const int SPIF_SENDWININICHANGE = 0x02;

        protected const uint WM_ICONS_VISIBILITY_COMMAND = 0x111;

        protected static readonly string DESKTOP_REGISTRY = @"Control Panel\Desktop";

        protected enum GetWindowCommand : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
        }

        [DllImport("user32.dll", SetLastError = true)]
        private protected static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private protected static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCommand uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private protected static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private protected static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [DllImport("user32.dll")]
        static protected extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static protected extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);
        [DllImport("user32.dll")]
        private protected static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        public static DesktopService Create(ServiceProvider services)
        {
            string windowsVersion = OSUtils.GetCurrentWindowsVersion();
            bool isWindows11 = OSUtils.IsWindows11(windowsVersion);

            if (isWindows11) {
                return new DesktopServiceW11(services);
            }

            return new DesktopServiceW10(services);
        }

        protected DesktopService(ServiceProvider services) : base(services) { }

        public abstract void CreateAndSwitchToDesktop(string desktopName);

        public abstract void RemoveDesktop(string desktopName);

        public abstract string GetCurrentDesktopName();

        public abstract void ChangeWallpaper(string wallpaperPath);

        public abstract List<WindowComponent> GetWindowsOnDesktop(string desktopName);

        public void ToggleIconsVisiblity(bool visible = false)
        {
            Logger.Info("Toggling desktop icons visibility");

            var toggleDesktopCommand = new IntPtr(0x7402);
            IntPtr hWnd = GetWindow(FindWindow("Progman", "Program Manager"), GetWindowCommand.GW_CHILD);
            SendMessage(hWnd, WM_ICONS_VISIBILITY_COMMAND, toggleDesktopCommand, IntPtr.Zero);
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
