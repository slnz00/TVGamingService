using Core.Components;
using System.Threading.Tasks;

namespace BackgroundService.Source.Services.Jobs.Components.JobTriggers
{
    internal class Timer : JobTrigger
    {
        public class TimerOptions
        {
            public int Time { get; set; }
        }

        private TimerOptions Options => GetOptions<TimerOptions>();

        private ManagedTask timerTask;

        public Timer(JobTriggerAction action, object options) : base(action, options) { }

        protected override void OnOpen()
        {
            timerTask = ManagedTask.Run(async (ctx) =>
            {
                await Task.Delay(Options.Time, ctx.Cancellation.Token);

                ExecuteTrigger();
            });
        }

        protected override void OnClose()
        {
            timerTask.Cancel(false);
        }
    }
}
