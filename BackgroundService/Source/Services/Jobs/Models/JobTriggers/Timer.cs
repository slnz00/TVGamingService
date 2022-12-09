using Core.Components;
using System.Threading.Tasks;

namespace BackgroundService.Source.Services.Jobs.Models.JobTriggers
{
    internal class Timer : JobTrigger
    {
        public class TimerOptions
        {
            public int Time { get; set; }
        }

        private ManagedTask task;

        private TimerOptions Options => GetOptions<TimerOptions>();

        public Timer(TriggerAction action, object options) : base(action, options) { }

        protected override void OnSetup()
        {
            task = ManagedTask.Run(async (ctx) =>
            {
                await Task.Delay(Options.Time, ctx.Cancellation.Token);

                ExecuteTrigger();
            });
        }

        protected override void OnTeardown() {
            task.Cancel();
        }
    }
}
