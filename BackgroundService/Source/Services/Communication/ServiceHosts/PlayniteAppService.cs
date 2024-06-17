using Core.Models.Playnite;
using Core.Interfaces.ServiceContracts;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using BackgroundService.Source.Providers;

namespace BackgroundService.Source.Services.Communication.ServiceHosts
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class PlayniteAppService : IPlayniteAppService
    {
        private readonly object threadLock = new object();

        private readonly LoggerProvider Logger;

        private readonly Queue<AsyncPlayniteTask> asyncTaskQueue = new Queue<AsyncPlayniteTask>();

        public class Events
        {
            public Action<PlayniteGameInfo> OnGameStarting { get; set; }
            public Action<PlayniteGameInfo> OnGameStarted { get; set; }
            public Action<PlayniteGameInfo> OnGameStopped { get; set; }
        }

        private Events events;

        public PlayniteAppService() {
            Logger = new LoggerProvider($"ServiceHost:{GetType().Name}");
        }

        public void SetEventHandlers(Events newEvents) {
            if (events != null) {
                Logger.Warn("Event handlers are already set, overwriting...");
            }

            events = newEvents;
        }

        public void EnqueueAsyncTask(AsyncPlayniteTask task)
        {
            lock (threadLock)
            {
                asyncTaskQueue.Enqueue(task);
            }
        }

        public void SendGameStarting(PlayniteGameInfo gameInfo)
        {
            events?.OnGameStarting(gameInfo);
        }

        public void SendGameStarted(PlayniteGameInfo gameInfo)
        {
            events?.OnGameStarted(gameInfo);
        }

        public void SendGameStopped(PlayniteGameInfo gameInfo)
        {
            events?.OnGameStopped(gameInfo);
        }

        public AsyncPlayniteTask GetAsyncTask()
        {
            lock (threadLock)
            {
                if (asyncTaskQueue.Count == 0)
                {
                    return null;
                }

                return asyncTaskQueue.Dequeue();
            }
        }
    }
}
