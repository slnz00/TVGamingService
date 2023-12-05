using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace BackgroundService.Source.Services.System.Models
{
    internal class WindowComponent
    {
        public enum WindowComponentState
        {
            Normal,
            Minimized,
            Maximized
        }

        private enum ShowWindowCommands
        {
            SW_HIDE,
            SW_NORMAL,
            SW_SHOWMINIMIZED,
            SW_MAXIMIZE,
            SW_SHOWNOACTIVATE,
            SW_SHOW,
            SW_MINIMIZE,
            SW_SHOWMINNOACTIVE,
            SW_SHOWNA,
            SW_RESTORE,
            SW_SHOWDEFAULT,
            SW_FORCEMINIMIZE,
        }

        private struct WindowPlacement
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rectangle rcNormalPosition;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowPlacement(IntPtr hWnd, ref WindowPlacement lpwndpl);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        public string Type { get; private set; }
        public IntPtr Handle { get; private set; }
        public int ProcessID => GetProcessID();
        public Process Process => GetProcess();
        public string Name => GetName();
        public WindowComponentState State => GetState();
        public bool IsValid => Handle != IntPtr.Zero;

        public WindowComponent(string type, IntPtr handle)
        {
            Type = type;
            Handle = handle;
        }

        public void Click()
        {
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONUP = 0x0202;

            // First click activates the component's window:
            SendMessage(Handle, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            SendMessage(Handle, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);

            // Click on the component:
            SendMessage(Handle, WM_LBUTTONDOWN, IntPtr.Zero, IntPtr.Zero);
            SendMessage(Handle, WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
        }

        public void Close()
        {
            const int WM_CLOSE = 0x0010;

            SendMessage(Handle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public void Show()
        {
            ShowWindow(Handle, (int)ShowWindowCommands.SW_NORMAL);
        }

        public void Minimize()
        {
            ShowWindow(Handle, (int)ShowWindowCommands.SW_SHOWMINNOACTIVE);
        }

        public void Maximize()
        {
            ShowWindow(Handle, (int)ShowWindowCommands.SW_MAXIMIZE);
        }

        public void Focus()
        {
            SetForegroundWindow(Handle);
        }

        private string GetName()
        {
            if (!IsValid)
            {
                return "";
            }

            var length = GetWindowTextLength(Handle) + 1;
            var nameBuffer = new StringBuilder(length);
            GetWindowText(Handle, nameBuffer, nameBuffer.Capacity);

            var name = nameBuffer.ToString();

            return name.Replace("&", " ").Trim();
        }

        private WindowComponentState GetState()
        {
            WindowPlacement placement = new WindowPlacement();
            GetWindowPlacement(Handle, ref placement);

            switch (placement.showCmd)
            {
                case 1:
                    return WindowComponentState.Normal;
                case 2:
                    return WindowComponentState.Minimized;
                case 3:
                    return WindowComponentState.Maximized;
                default:
                    return WindowComponentState.Normal;
            }
        }

        private int GetProcessID()
        {
            GetWindowThreadProcessId(Handle, out uint pid);

            return (int)pid;
        }

        private Process GetProcess()
        {
            return Process.GetProcessById(GetProcessID());
        }
    }
}
