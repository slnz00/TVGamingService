using Core.Utils;
using System.Diagnostics;
using System.IO;

namespace BackgroundService.Source.Services.Jobs.Models.JobActions
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

        public StartProcess(object options) : base(options) { }

        public override void Run(Job job)
        {
            var options = GetOptions<StartProcessOptions>();

            var path = Path.GetFullPath(options.Path);
            var args = options.Args;
            var windowStyle = EnumUtils.GetValue<ProcessWindowStyle>(options.WindowStyle);

            ProcessUtils.StartProcess(path, args, windowStyle, false, SetupStartInfo);
        }

        private void SetupStartInfo(ProcessStartInfo startInfo)
        {
            var options = GetOptions<StartProcessOptions>();

            if (!string.IsNullOrWhiteSpace(options.WorkingDirectory))
            {
                startInfo.WorkingDirectory = Path.GetFullPath(options.WorkingDirectory);
            }
        }
    }
}
