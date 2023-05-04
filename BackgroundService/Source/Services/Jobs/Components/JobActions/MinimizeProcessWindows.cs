using BackgroundService.Source.Services.Jobs.Components.Common;
using BackgroundService.Source.Services.System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BackgroundService.Source.Services.Jobs.Components.JobActions
{

    internal class MinimizeProcessWindows : JobAction
    {
        public class MinimizeProcessWindowsOptions
        {
            public string ProcessName { get; set; }
            public bool MinimizeOnce { get; set; } = true;
        }

        private MinimizeProcessWindowsOptions Options => GetOptions<MinimizeProcessWindowsOptions>();

        private List<string> alreadyMinimizedWindowNames = new List<string>();

        public MinimizeProcessWindows(object options) : base(options) { }

        protected override void OnOptionsValidation()
        {
            Validations.ValidateNotEmptyOrNull(nameof(Options.ProcessName), Options.ProcessName);
        }

        protected override void OnExecution()
        {
            var windows = GetWindows();

            Func<WindowComponent, bool> windowIsValid = win => win.IsValid && !string.IsNullOrWhiteSpace(win.Name);
            Func<WindowComponent, bool> windowIsNotMinimized = win => win.State != WindowComponent.WindowComponentState.Minimized;

            var windowsToMinimize = windows
                .Where(windowIsValid)
                .Where(windowIsNotMinimized)
                .ToList();

            windowsToMinimize.ForEach(MinimizeWindow);
        }

        protected override void OnReset()
        {
            alreadyMinimizedWindowNames = new List<string>();
        }

        private List<WindowComponent> GetWindows()
        {
            var currentDesktopName = Services.System.Desktop.GetCurrentDesktopName();

            var windowsOnDesktop = Services.System.Desktop.GetWindowsOnDesktop(currentDesktopName);

            return windowsOnDesktop.FindAll(win => Regex.IsMatch(win.Process.ProcessName, Options.ProcessName));
        }

        private void MinimizeWindow(WindowComponent window)
        {
            var options = GetOptions<MinimizeProcessWindowsOptions>();

            bool alreadyMinimized = alreadyMinimizedWindowNames.Contains(window.Name);
            if (alreadyMinimized && options.MinimizeOnce)
            {
                return;
            }

            window.Minimize();

            if (!alreadyMinimized)
            {
                alreadyMinimizedWindowNames.Add(window.Name);
            }
        }
    }
}
