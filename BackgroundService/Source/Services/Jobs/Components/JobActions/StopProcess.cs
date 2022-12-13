using BackgroundService.Source.Services.Jobs.Components.Common;
using Core.Utils;
using System;

namespace BackgroundService.Source.Services.Jobs.Components.JobActions
{
    internal class StopProcess : JobAction
    {
        public class StopProcessOptions
        {
            public string ProcessName { get; set; }
            public bool ForceClose { get; set; } = true;
        }

        private StopProcessOptions Options => GetOptions<StopProcessOptions>();

        public StopProcess(object options) : base(options) { }

        protected override void OnOptionsValidation()
        {
            Validations.ValidateNotEmptyOrNull(nameof(Options.ProcessName), Options.ProcessName);
        }

        protected override void OnExecution()
        {
            ProcessUtils.CloseProcess(Options.ProcessName, Options.ForceClose);
        }
    }
}
