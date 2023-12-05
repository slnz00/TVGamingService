using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Core.Components.System
{
    public class MessageLoop
    {
        private const int WM_CUSTOM_EXIT = 0x0400 + 2000;
        private const int WM_CUSTOM_PROCESS_ACTION_QUEUE = 0x0400 + 2001;

        [DllImport(@"user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool GetMessage(ref MSG message, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport(@"user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern bool TranslateMessage(ref MSG message);

        [DllImport(@"user32.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        private static extern long DispatchMessage(ref MSG message);

        [DllImport(@"kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool PostThreadMessage(uint threadId, int msg, IntPtr wParam, IntPtr lParam);

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

        private bool running = false;
        private List<Action<MSG>> messageHandlers = new List<Action<MSG>>();
        private ConcurrentQueue<Action> actionQueue = new ConcurrentQueue<Action>();

        public readonly uint ThreadId;

        public bool IsRunning => running;

        public MessageLoop()
        {
            ThreadId = GetCurrentThreadId();
        }

        public void Run()
        {
            if (running)
            {
                throw new InvalidOperationException("Cannot run message loop multiple times, message loop is already running");
            }

            running = true;

            MSG msg = new MSG();

            while (GetMessage(ref msg, IntPtr.Zero, 0, 0))
            {
                if (msg.message == WM_CUSTOM_EXIT)
                {
                    break;
                }
                if (msg.message == WM_CUSTOM_PROCESS_ACTION_QUEUE)
                {
                    ProcessActionQueue();
                    continue;
                }

                messageHandlers.ForEach(handler => handler(msg));

                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }

        public void ExecuteOnMainThread(Action action)
        {
            actionQueue.Enqueue(action);

            PostThreadMessage(ThreadId, WM_CUSTOM_PROCESS_ACTION_QUEUE, IntPtr.Zero, IntPtr.Zero);
        }

        public uint RegisterMessageHandler(Action<MSG> handler)
        {
            messageHandlers.Add(handler);

            return (uint)messageHandlers.Count - 1;
        }

        private void ProcessActionQueue()
        {
            while (actionQueue.TryDequeue(out Action action)) {
                action();
            }
        }

        public bool UnregisterMessageHandler(uint id)
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
