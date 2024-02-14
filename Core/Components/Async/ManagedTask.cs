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
            private readonly TimeSpan LOCK_TIMEOUT = TimeSpan.FromMilliseconds(10);

            public CancellationTokenSource Cancellation { get; set; }

            public async Task Delay(int millisecondsDelay)
            {
                await Task.Delay(millisecondsDelay, Cancellation.Token);
            }

            public async Task Delay(TimeSpan delay)
            {
                await Task.Delay(delay, Cancellation.Token);
            }

            public void Lock(object lockObject, Action action)
            {
                var lockAcquired = false;

                while (!lockAcquired)
                {
                    lockAcquired = TryToLockAndRun(lockObject, action);

                    Cancellation.Token.ThrowIfCancellationRequested();
                }
            }

            public bool Lock(object lockObject, TimeSpan timeout, Action action)
            {
                var lockAcquired = false;
                var timeoutAt = NowMs() + timeout.TotalMilliseconds;

                while (!lockAcquired && timeoutAt <= NowMs())
                {
                    lockAcquired = TryToLockAndRun(lockObject, action);

                    Cancellation.Token.ThrowIfCancellationRequested();
                }

                return lockAcquired;
            }

            private bool TryToLockAndRun(object lockObject, Action action)
            {
                var lockAcquired = false;

                if (Monitor.TryEnter(lockObject, LOCK_TIMEOUT))
                {
                    try
                    {
                        action();
                    }
                    finally
                    {
                        Monitor.Exit(lockObject);
                        lockAcquired = true;
                    }
                }

                return lockAcquired;
            }

            private long NowMs()
            {
                return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        public Task Task => task;
        public bool IsAlive => AsyncUtils.IsTaskAlive(task);

        private Func<Context, Task> action;
        private Context context;
        private Task task;
        private CancellationTokenSource cancellation;

        public static ManagedTask Run(Action<Context> action, TaskCreationOptions options = TaskCreationOptions.None)
        {
            return Run(async (ctx) => action(ctx), options);
        }

        public static ManagedTask Run(Func<Context, Task> action, TaskCreationOptions options = TaskCreationOptions.None)
        {
            var managedTask = new ManagedTask(action, options);

            managedTask.Start();

            return managedTask;
        }

        public ManagedTask(Action<Context> action, TaskCreationOptions options = TaskCreationOptions.None)
            : this(async (ctx) => action(ctx), options)
        { }

        public ManagedTask(Func<Context, Task> action, TaskCreationOptions options = TaskCreationOptions.None)
        {
            cancellation = new CancellationTokenSource();

            context = new Context
            {
                Cancellation = cancellation
            };

            this.action = action;

            task = new Task(() => RunAction().Wait(), cancellation.Token, options);
        }

        public void Start()
        {
            task.Start();
        }

        public void Wait()
        {
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
                bool timedOut = !Wait(TimeSpan.FromSeconds(2));
                if (timedOut)
                {
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
