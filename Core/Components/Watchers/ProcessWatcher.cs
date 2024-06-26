﻿using Core.Interfaces;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Core.Components.Watchers
{
    public class ProcessWatcher
    {
        public class Events
        {
            public Action OnProcessOpened { get; set; } = null;
            public Action OnProcessClosed { get; set; } = null;
        }

        public string ProcessName { get; private set; }

        public bool IsWatcherRunning => watcherTask != null && watcherTask.IsAlive;
        public bool IsProcessOpen { get; private set; }

        private readonly ILogger Logger;

        private readonly object threadLock = new object();
        private readonly int timeBetweenChecks;
        private readonly Events events;

        private ManagedTask watcherTask = null;

        public ProcessWatcher(string processName, ILogger logger, Events events, int timeBetweenChecks = 500)
        {
            ProcessName = processName;
            Logger = logger;

            this.timeBetweenChecks = timeBetweenChecks;
            this.events = events;
        }

        public void Start()
        {
            lock (threadLock)
            {
                if (IsWatcherRunning)
                {
                    Stop();
                }

                IsProcessOpen = RequestProcessRunningStatus();
                watcherTask = WatchProcess();
            }
        }

        public void Stop()
        {
            lock (threadLock)
            {
                watcherTask.Cancel();


                IsProcessOpen = false;
                watcherTask = null;
            }
        }

        private ManagedTask WatchProcess()
        {
            return ManagedTask.Run(async (ctx) =>
            {
                while (!ctx.Cancellation.IsCancellationRequested)
                {
                    bool currentlyOpen = RequestProcessRunningStatus();

                    if (IsProcessOpen && !currentlyOpen)
                    {
                        _ = Task.Run(() => RunEvent(events.OnProcessClosed));
                    }
                    else if (!IsProcessOpen && currentlyOpen)
                    {
                        _ = Task.Run(() => RunEvent(events.OnProcessOpened));
                    }

                    IsProcessOpen = currentlyOpen;

                    await Task.Delay(timeBetweenChecks, ctx.Cancellation.Token);
                }
            });
        }

        private bool RequestProcessRunningStatus()
        {
            return Process.GetProcessesByName(ProcessName).Length != 0;
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
