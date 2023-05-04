using BackgroundService.Source.Services.Jobs.Components.Common;
using Core.Utils;
using System;
using System.Diagnostics;
using System.IO;

namespace BackgroundService.Source.Services.Jobs.Components.JobActions
{
    internal class StartProcess : JobAction
    {
        public class StartProcessOptions
        {
            public string Path { get; set; }
            public string Args { get; set; } = "";
            public string WorkingDirectory { get; set; } = "";
            public string WindowStyle { get; set; } = "Normal";
        }

        private StartProcessOptions Options => GetOptions<StartProcessOptions>();

        public StartProcess(object options) : base(options) { }

        protected override void OnOptionsValidation()
        {
            Validations.ValidateNotEmptyOrNull(nameof(Options.Path), Options.Path);
        }

        protected override void OnExecution()
        {
            var path = Path.GetFullPath(Options.Path);
            var args = Options.Args;
            var windowStyle = EnumUtils.GetValue<ProcessWindowStyle>(Options.WindowStyle);

            ProcessUtils.StartProcess(path, args, windowStyle, false, SetupStartInfo);
        }

        protected override void OnReset() { }

        private void SetupStartInfo(ProcessStartInfo startInfo)
        {
            if (!string.IsNullOrWhiteSpace(Options.WorkingDirectory))
            {
                startInfo.WorkingDirectory = Path.GetFullPath(Options.WorkingDirectory);
            }
        }
    }
}
