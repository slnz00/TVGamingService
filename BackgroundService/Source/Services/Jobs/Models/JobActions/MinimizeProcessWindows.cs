using BackgroundService.Source.Services.System.Models;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BackgroundService.Source.Services.Jobs.Models.JobActions
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

        public MinimizeProcessWindows(MinimizeProcessWindowsOptions options) : base(options) { }

        public override void Run(Job.Context context)
        {
            var services = context.Services;

            ValidateOptions();

            ProcessUtils.InteractWithProcess(Options.ProcessName, (process) =>
            {
                var allWindows = services.System.Window.GetProcessWindows(process.Id);

                Func<WindowComponent, bool> windowIsValid = win => win.IsValid && !string.IsNullOrWhiteSpace(win.Name);
                Func<WindowComponent, bool> windowIsNotMinimized = win => win.State != WindowComponent.WindowComponentState.MINIMIZED;

                var windowsToMinimize = allWindows
                    .Where(windowIsValid)
                    .Where(windowIsNotMinimized)
                    .ToList();

                windowsToMinimize.ForEach(MinimizeWindow);
            });
        }

        private void MinimizeWindow(WindowComponent window) {
            var options = GetOptions<MinimizeProcessWindowsOptions>();

            bool alreadyMinimized = alreadyMinimizedWindowNames.Contains(window.Name);
            if (alreadyMinimized && options.MinimizeOnce) {
                return;
            }

            window.Minimize();

            if (!alreadyMinimized)
            {
                alreadyMinimizedWindowNames.Add(window.Name);
            }
        }

        private void ValidateOptions()
        {
            if (string.IsNullOrEmpty(Options.ProcessName))
            {
                throw new NullReferenceException(nameof(Options.ProcessName));
            }
        }
    }
}
