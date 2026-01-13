using MS.WindowsAPICodePack.Internal;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Core.WinAPI.InputAPI;
using static Core.WinAPI.WindowAPI;

namespace BackgroundService.Source.Services.OS.Models
{
    internal class WindowComponent
    {
        public enum WindowComponentState
        {
            Normal,
            Minimized,
            Maximized
        }

        public string Type { get; private set; }
        public IntPtr Handle { get; private set; }
        public String ID => $"{ProcessID}:{Name}";
        public int ProcessID => GetProcessID();
        public Process Process => GetProcess();
        public string Name => GetName();
        public WindowComponentState State => GetState();
        public bool IsValid => Handle != null && IsWindow(Handle);

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

        public static void UnlockForegroundLock()
        {
            const byte VK_SHIFT = 0x10;
            const uint KEYEVENTF_KEYUP = 0x0002;

            keybd_event(VK_SHIFT, 0, 0, 0);
            keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, 0);
        }

        public void Focus()
        {
            var currentThreadId = GetCurrentThreadId();
            var foregroundThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out var _);

            UnlockForegroundLock();

            bool attached = AttachThreadInput(currentThreadId, foregroundThreadId, true);

            Console.WriteLine($"attached: {attached}");

            try
            {
                ShowWindow(Handle, (int)ShowWindowCommands.SW_RESTORE);
                SetForegroundWindow(Handle);
                BringWindowToTop(Handle);
                SwitchToThisWindow(Handle, true);
            }
            finally
            {
                if (attached)
                {
                    AttachThreadInput(currentThreadId, foregroundThreadId, false);
                }
            }
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
