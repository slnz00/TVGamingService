using Core.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Components
{
    public class ManagedTask
    {
        public class Context
        {
            public CancellationTokenSource Cancellation { get; set; }
        }

        public Task Task => task;
        public bool IsAlive => AsyncUtils.IsTaskAlive(task);

        private Func<Context, Task> action;
        private Context context;
        private Task task;
        private CancellationTokenSource cancellation;

        public static ManagedTask Run(Func<Context, Task> action)
        {
            var managedTask = new ManagedTask(action);

            managedTask.Start();

            return managedTask;
        }

        public ManagedTask(Func<Context, Task> action)
        {
            cancellation = new CancellationTokenSource();

            context = new Context
            {
                Cancellation = cancellation
            };

            this.action = action;

            task = new Task(() => RunAction().Wait(), cancellation.Token);
        }

        public void Start()
        {
            task.Start();
        }

        public void Wait() {
            task.Wait();
        }

        public bool Wait(TimeSpan timeout)
        {
            return task.Wait(timeout);
        }

        public void Cancel(bool wait = true)
        {
            if (!cancellation.IsCancellationRequested && IsAlive)
            {
                cancellation.Cancel();
            }

            if (wait)
            {
                bool timedOut = !Wait(TimeSpan.FromSeconds(900));
                if (timedOut) {
                    throw new TimeoutException("ManagedTask cancellation timed out");
                }
            }
        }

        private async Task RunAction()
        {
            try
            {
                await action(context);
            }
            catch (TaskCanceledException) { }
        }
    }
}
