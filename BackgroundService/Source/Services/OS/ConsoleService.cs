using BackgroundService.Source.Providers;
using System;
using System.Runtime.InteropServices;

namespace BackgroundService.Source.Services.OS
{
    internal class ConsoleService : Service
    {
        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static readonly bool DEFAULT_VISIBILITY = InternalSettings.ConsoleDefaultVisibility;

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private readonly IntPtr consoleWindowHandle;

        public bool IsConsoleVisible { get; private set; }

        public ConsoleService(ServiceProvider services) : base(services)
        {
            consoleWindowHandle = GetConsoleWindow();
        }

        protected override void OnInitialize()
        {
            SetConsoleVisibility(DEFAULT_VISIBILITY);
        }

        public void ToggleConsoleVisibility()
        {
            SetConsoleVisibility(!IsConsoleVisible);
        }

        public void SetConsoleVisibility(bool visibility)
        {
            ShowWindow(consoleWindowHandle, visibility ? SW_SHOW : SW_HIDE);

            IsConsoleVisible = visibility;

            Logger.Info($"Console window visibility changed to: {visibility}");
        }
    }
}
