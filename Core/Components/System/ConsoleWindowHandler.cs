using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Core.Components.System
{
    public static class ConsoleWindowHandler
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        private static EventHandler handler;

        private static bool initialized = false;

        public static Action OnExit = null;

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool WindowHandler(CtrlType sig)
        {
            if (OnExit != null)
            {
                OnExit();
            }

            Environment.Exit(0);

            return true;
        }

        public static void Initialize()
        {
            handler += new EventHandler(WindowHandler);

            SetConsoleCtrlHandler(handler, true);
        }
    }
}
