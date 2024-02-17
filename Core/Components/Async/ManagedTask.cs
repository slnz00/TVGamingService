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

            public readonly CancellationTokenSource Cancellation;

            public Context(CancellationTokenSource cancellation)
            {
                Cancellation = cancellation;
            }

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

        public bool IsAlive => task != null && AsyncUtils.IsTaskAlive(task);
        public bool Started => task != null;

        private readonly Func<Context, Task> actionAsync;
        private readonly Action<Context> actionSync;
        private readonly Context context;
        private readonly CancellationTokenSource cancellation;
        private Task task;

        public static ManagedTask Run(Action<Context> action)
        {
            var managedTask = new ManagedTask(action);

            managedTask.Start();

            return managedTask;
        }

        public static ManagedTask Run(Func<Context, Task> action)
        {
            var managedTask = new ManagedTask(action);

            managedTask.Start();

            return managedTask;
        }

        public ManagedTask(Action<Context> action) : this()
        {
            actionSync = action;
        }

        public ManagedTask(Func<Context, Task> action) : this()
        {
            actionAsync = action;
        }

        private ManagedTask()
        {
            cancellation = new CancellationTokenSource();
            context = new Context(cancellation);
        }

        public ManagedTask Start()
        {
            if (Started)
            {
                return this;
            }

            if (actionSync != null)
            {
                task = new Task(() => RunSyncAction(), cancellation.Token, TaskCreationOptions.LongRunning);
                task.Start();
            }
            else
            {
                task = Task.Run(RunAsyncAction, cancellation.Token);
            }

            return this;
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

        private void RunSyncAction()
        {
            try
            {
                actionSync(context);
            }
            catch (TaskCanceledException) { }
        }

        private async Task RunAsyncAction()
        {
            try
            {
                await actionAsync(context);
            }
            catch (TaskCanceledException) { }
        }
    }
}
