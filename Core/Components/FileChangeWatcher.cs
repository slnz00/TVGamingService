using Core.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace Core.Components
{
    public class FileChangeWatcher
    {
        public string FilePath { get; private set; }
        public ILogger Logger { get; private set; }

        private readonly int EVENT_DISPATCH_THRESHOLD_MS = 500;
        private readonly int EVENT_LOOP_SLEEP_TIME_MS = 250;

        private readonly object threadLock = new object();
        private readonly FileSystemWatcher fileWatcher = new FileSystemWatcher();

        private int currentListenerId = 0;
        private Dictionary<int, Action> eventListeners;
        private ManagedTask eventLoop;
        private long? eventDispatchAt = null;

        public FileChangeWatcher(string filePath, ILogger logger)
        {
            FilePath = filePath;
            Logger = logger;

            RunEventLoop();
            SetupFileWatcher();
        }

        public FileChangeWatcher(string filePath) : this(filePath, null) { }

        public void Dispose()
        {
            StopEventLoop();
            fileWatcher.Dispose();
        }

        public int OnChanged(Action handler)
        {
            lock (threadLock)
            {
                int listenerId = currentListenerId++;

                eventListeners.Add(listenerId, handler);

                return listenerId;
            }
        }

        public bool RemoveEventListener(int listenerId)
        {
            lock (threadLock)
            {
                if (!eventListeners.ContainsKey(listenerId))
                {
                    return false;
                }

                eventListeners.Remove(listenerId);
                return true;
            }
        }

        private void SetupFileWatcher()
        {
            fileWatcher.Path = Path.GetDirectoryName(FilePath);
            fileWatcher.Filter = Path.GetFileName(FilePath);
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.EnableRaisingEvents = true;

            fileWatcher.Changed += new FileSystemEventHandler((object source, FileSystemEventArgs e) =>
            {
                ScheduleOnChangedEvent();
            });
        }

        private void RunEventLoop()
        {
            eventListeners = new Dictionary<int, Action>();

            eventLoop = ManagedTask.Run(async (ctx) =>
            {
                while (true)
                {
                    ctx.Lock(threadLock, () =>
                    {
                        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        bool shouldDispatchEvent = eventDispatchAt != null && eventDispatchAt <= now;

                        if (shouldDispatchEvent)
                        {
                            DispatchOnChangedEvent();

                            eventDispatchAt = null;
                        }
                    });

                    await ctx.Delay(EVENT_LOOP_SLEEP_TIME_MS);
                }
            });
        }

        private void StopEventLoop()
        {
            if (eventLoop != null) {
                eventLoop.Cancel();
            }
        }

        private void ScheduleOnChangedEvent()
        {
            lock (threadLock)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                eventDispatchAt = now + EVENT_DISPATCH_THRESHOLD_MS;
            }
        }

        private void DispatchOnChangedEvent()
        {
            lock (threadLock)
            {
                foreach (var listener in eventListeners.Values)
                {
                    RunEventListener(listener);
                }
            }
        }

        private void RunEventListener(Action listener)
        {
            try
            {
                listener();
            }
            catch (Exception ex)
            {
                var message = $"Executing file onChanged event listener failed: {ex}";

                if (Logger != null)
                {
                    Logger.Error(message);
                }
                else
                {
                    Console.WriteLine(message);
                }
            }
        }
    }
}
