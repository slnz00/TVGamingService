﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BackgroundService.Source.Providers;
using Core.Utils;

namespace BackgroundService.Source.Services.System
{
    internal class DesktopService : Service
    {
        private const uint WM_ICONS_VISIBILITY_COMMAND = 0x111;
        private static readonly string VIRTUAL_DESKTOP_PATH = InternalSettings.PATH_VIRTUAL_DESKTOP;

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
        static private extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

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

        public void ToggleIconsVisiblity()
        {
            Logger.Debug("Toggling desktop icons visibility");

            var toggleDesktopCommand = new IntPtr(0x7402);
            IntPtr hWnd = GetWindow(FindWindow("Progman", "Program Manager"), GetWindowCommand.GW_CHILD);
            SendMessage(hWnd, WM_ICONS_VISIBILITY_COMMAND, toggleDesktopCommand, IntPtr.Zero);
        }

        private void ExecVirtualDesktop(string args)
        {
            Logger.Debug($"Exec virtual desktop, args: {args}");

            ProcessUtils.StartProcess(VIRTUAL_DESKTOP_PATH, args, ProcessWindowStyle.Hidden, true);
        }
    }
}