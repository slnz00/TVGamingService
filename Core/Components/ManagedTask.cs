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

        public Task Task => Task;
        public bool IsAlive => AsyncUtils.IsTaskAlive(task);

        private Func<Context, Task> action;
        private Context context;
        private Task task;
        private CancellationTokenSource cancellation;

        public static ManagedTask Run(Action<Context> action)
        {
            return Run(async (ctx) => action(ctx));
        }

        public static ManagedTask Run(Func<Context, Task> action)
        {
            var managedTask = new ManagedTask(action);

            managedTask.Start();

            return managedTask;
        }

        public ManagedTask(Action<Context> action) : this(async (ctx) => action(ctx)) { }

        public ManagedTask(Func<Context, Task> action)
        {
            cancellation = new CancellationTokenSource();

            context = new Context
            {
                Cancellation = cancellation
            };

            this.action = action;

            task = new Task(async () => await RunAction(), cancellation.Token);
        }

        public void Start()
        {
            task.Start();
        }

        public void Wait()
        {
            task.Wait();
        }

        public void Cancel()
        {
            if (!cancellation.IsCancellationRequested && IsAlive)
            {
                cancellation.Cancel();
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
