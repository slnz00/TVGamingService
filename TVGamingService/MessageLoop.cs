using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TVGamingService
{
    internal class MessageLoop
    {
        private const int WM_CUSTOM_EXIT = 0x0400 + 2000;

        private static List<Action<MSG>> _messageHandlers = new List<Action<MSG>>();

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

        public static void Run() {
            MSG msg = new MSG();

            while (GetMessage(ref msg, IntPtr.Zero, 0, 0))
            {
                if (WM_CUSTOM_EXIT == msg.message)
                {
                    break;
                }

                foreach (var handler in _messageHandlers) {
                    handler(msg);
                }

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        public static uint RegisterMessageHandler(Action<MSG> handler) {
            _messageHandlers.Add(handler);

            return (uint)_messageHandlers.Count - 1;
        }

        public static bool UnregisterMessageHandler(uint id) {
            if (id >= _messageHandlers.Count) {
                return false;
            }

            _messageHandlers.RemoveAt((int)id);
            return true;
        }
    }
}
