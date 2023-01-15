using BackgroundService.Source.Services.Jobs.Components.Common;
using BackgroundService.Source.Services.System.Models;
using Core.Utils;
using System;
using System.Text.RegularExpressions;

namespace BackgroundService.Source.Services.Jobs.Components.JobActions
{
    internal class WindowInteraction : JobAction
    {
        enum Interactions
        {
            Close,
            Minimize,
            Maximize,
            Show,
            Click,
            StopProcess,
            ForceStopProcess
        }

        public class WindowInteractionOptions
        {
            public string WindowName { get; set; }
            public string ProcessName { get; set; }
            public string ComponentName { get; set; } = null;
            public string ComponentType { get; set; } = null;
            public string Interaction { get; set; }
        }

        private WindowInteractionOptions Options => GetOptions<WindowInteractionOptions>();

        public WindowInteraction(object options) : base(options) { }

        protected override void OnOptionsValidation()
        {
            Validations.ValidateNotEmptyOrNull(nameof(Options.WindowName), Options.WindowName);
            Validations.ValidateEnumValue<Interactions>(nameof(Options.Interaction), Options.Interaction);
        }

        protected override void OnExecution()
        {
            var component = FindWindowComponent();
            if (component == null || !component.IsValid)
            {
                return;
            }

            var interaction = EnumUtils.GetValue<Interactions>(Options.Interaction);
            switch (interaction)
            {
                case Interactions.Close:
                    component.Close();
                    break;
                case Interactions.Minimize:
                    component.Close();
                    break;
                case Interactions.Maximize:
                    component.Close();
                    break;
                case Interactions.Show:
                    component.Close();
                    break;
                case Interactions.Click:
                    component.Close();
                    break;
                case Interactions.StopProcess:
                    StopComponentParentProcess(component, false);
                    break;
                case Interactions.ForceStopProcess:
                    StopComponentParentProcess(component, true);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected WindowComponent FindWindowComponent()
        {
            var window = GetWindow();
            if (window?.IsValid != true)
            {
                return null;
            }

            if (string.IsNullOrEmpty(Options.ComponentType))
            {
                return window;
            }

            var components = Services.System.Window.GetChildComponent(window, Options.ComponentType);
            if (components.Count == 0)
            {
                return null;
            }

            if (string.IsNullOrEmpty(Options.ComponentName))
            {
                return components[0];
            }

            return components.Find(c => c.Name == Options.ComponentName);
        }

        protected WindowComponent GetWindow()
        {
            var currentDesktopName = Services.System.Desktop.GetCurrentDesktopName();
            var allWindows = Services.System.Desktop.GetWindowsOnDesktop(currentDesktopName);

            var window = allWindows.Find(win => {
                var windowNameMatching = Regex.IsMatch(win.Name, Options.WindowName);
                var processNameMatching = !string.IsNullOrEmpty(Options.ProcessName) && Regex.IsMatch(win.Process.ProcessName, Options.ProcessName);

                return windowNameMatching && processNameMatching;
            });

            return window;
        }

        protected void StopComponentParentProcess(WindowComponent component, bool force)
        {
            var processId = component.ProcessID;

            ProcessUtils.CloseProcess(processId, force);
        }
    }
}
