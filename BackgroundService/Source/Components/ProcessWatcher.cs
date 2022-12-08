using BackgroundService.Source.Providers;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BackgroundService.Source.Common
{
    internal class ProcessWatcher
    {
        public class Events
        {
            public Action OnProcessOpened { get; set; } = null;
            public Action OnProcessClosed { get; set; } = null;
        }

        public string ProcessName { get; private set; }

        public bool IsWatcherRunning { get; private set; }
        public bool IsProcessOpen { get; private set; }

        private LoggerProvider Logger;

        private object threadLock = new object();
        private int timeBetweenChecks;
        private Events events;

        private Task watcherTask = null;
        private CancellationTokenSource watcherCancellation = null;

        public ProcessWatcher(string processName, Events events, int timeBetweenChecks = 500)
        {
            ProcessName = processName;

            this.timeBetweenChecks = timeBetweenChecks;
            this.events = events;

            Logger = new LoggerProvider($"{GetType().Name}:{ProcessName}");
        }

        public void Start()
        {
            lock (threadLock)
            {
                if (IsWatcherRunning)
                {
                    return;
                }
                if (watcherTask != null)
                {
                    ResetWatcher();
                }

                watcherCancellation = new CancellationTokenSource();
                watcherTask = WatchProcess();

                IsWatcherRunning = true;
            }
        }

        public void Stop()
        {
            lock (threadLock)
            {
                watcherCancellation.Cancel();
                watcherTask.Wait();

                ResetWatcher();
            }
        }

        private void ResetWatcher()
        {
            if (!watcherCancellation.IsCancellationRequested)
            {
                watcherCancellation.Cancel();
            }

            IsWatcherRunning = false;
            IsProcessOpen = false;

            watcherCancellation = null;
            watcherTask = null;
        }

        private async Task WatchProcess()
        {
            try
            {
                while (!watcherCancellation.IsCancellationRequested)
                {
                    bool currentlyOpen = Process.GetProcessesByName(ProcessName).Length != 0;

                    if (IsProcessOpen && !currentlyOpen)
                    {
                        _ = Task.Run(() => RunEvent(events.OnProcessClosed));
                    }
                    else if (!IsProcessOpen && currentlyOpen)
                    {
                        _ = Task.Run(() => RunEvent(events.OnProcessOpened));
                    }

                    IsProcessOpen = currentlyOpen;

                    await Task.Delay(timeBetweenChecks, watcherCancellation.Token);
                }
            }
            catch (TaskCanceledException) { }
        }

        private void RunEvent(Action e)
        {
            try
            {
                e?.Invoke();
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to run ProcessWatcher event: {ex}");
            }
        }
    }
}
