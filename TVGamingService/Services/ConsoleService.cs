using System;
using System.Runtime.InteropServices;
using TVGamingService.Providers;

namespace TVGamingService.Services
{
    internal class ConsoleService : BaseService
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static readonly bool DEFAULT_VISIBILITY = InternalSettings.CONSOLE_DEFAULT_VISIBILITY;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private IntPtr consoleWindowHandle;
        private bool isConsoleVisible;

        public bool IsConsoleVisible => isConsoleVisible;

        public ConsoleService(ServiceProvider services) : base(services)
        {
            consoleWindowHandle = GetConsoleWindow();
        }

        protected override void OnInitialize()
        {
            Logger.Debug($"Console window default visibility: {DEFAULT_VISIBILITY}");

            SetConsoleVisibility(DEFAULT_VISIBILITY);
        }

        public void ToggleConsoleVisibility()
        {
            SetConsoleVisibility(!isConsoleVisible);
        }

        public void SetConsoleVisibility(bool visibility)
        {
            ShowWindow(consoleWindowHandle, visibility ? SW_SHOW : SW_HIDE);
            isConsoleVisible = visibility;

            Logger.Debug($"Console window visibility changed to: {visibility}");
        }
    }
}
