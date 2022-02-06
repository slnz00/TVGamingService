using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TVGamingService
{
    internal static class DesktopManager
    {
        private static readonly uint WM_ICONS_VISIBILITY_COMMAND = 0x111;
        private static readonly string VD_PATH = Utils.GetFullPath("deps/vd.exe");

        public static void CreateAndSwitchToDesktop(string desktopName)
        {
            Utils.StartProcess(VD_PATH, $"/New /Name:{desktopName} /Switch", ProcessWindowStyle.Hidden, true);
        }

        public static void RemoveDesktop(string desktopName)
        {
            Utils.StartProcess(VD_PATH, $"/Remove:{desktopName}", ProcessWindowStyle.Hidden, true);
        }

        public static void ToggleIconsVisiblity()
        {
            var toggleDesktopCommand = new IntPtr(0x7402);
            IntPtr hWnd = GetWindow(FindWindow("Progman", "Program Manager"), GetWindow_Cmd.GW_CHILD);
            SendMessage(hWnd, WM_ICONS_VISIBILITY_COMMAND, toggleDesktopCommand, IntPtr.Zero);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static private extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }
    }
}
