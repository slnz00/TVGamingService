using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Core.SystemComponents
{
    public static class MessageLoop
    {
        private const int WM_CUSTOM_EXIT = 0x0400 + 2000;

        [DllImport(@"user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetMessage(ref MSG message, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);
        [DllImport(@"user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool TranslateMessage(ref MSG message);
        [DllImport(@"user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern long DispatchMessage(ref MSG message);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            long x;
            long y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public UIntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public POINT pt;
        }

        private static bool running = false;
        private static List<Action<MSG>> messageHandlers = new List<Action<MSG>>();

        public static bool IsRunning => running;

        public static void Run()
        {
            if (running)
            {
                throw new InvalidOperationException("Cannot run message loop multiple times, message loop is already running");
            }
            running = true;

            MSG msg = new MSG();

            while (GetMessage(ref msg, IntPtr.Zero, 0, 0))
            {
                if (WM_CUSTOM_EXIT == msg.message)
                {
                    break;
                }

                messageHandlers.ForEach(handler => handler(msg));

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        public static uint RegisterMessageHandler(Action<MSG> handler)
        {
            messageHandlers.Add(handler);

            return (uint)messageHandlers.Count - 1;
        }

        public static bool UnregisterMessageHandler(uint id)
        {
            if (id >= messageHandlers.Count)
            {
                return false;
            }

            messageHandlers.RemoveAt((int)id);
            return true;
        }
    }
}
