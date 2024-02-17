using Core.Playnite.Communication.Models;
using Core.Playnite.Communication.Models.Commands;
using Core.Playnite.Communication.Services;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace BackgroundService.Source.Services.ThirdParty.Playnite.Communication.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class PlayniteAppService : IPlayniteAppService
    {
        private readonly object threadLock = new object();

        private readonly Queue<AsyncPlayniteTask> asyncTaskQueue = new Queue<AsyncPlayniteTask>();

        public class Events
        {
            public Action<PlayniteGameInfo> OnGameStarting { get; set; }
            public Action<PlayniteGameInfo> OnGameStarted { get; set; }
            public Action<PlayniteGameInfo> OnGameStopped { get; set; }
        }

        private readonly Events events;

        public PlayniteAppService(Events events)
        {
            this.events = events;
        }

        public void SendGameStarting(PlayniteGameInfo gameInfo)
        {
            events.OnGameStarting(gameInfo);
        }

        public void SendGameStarted(PlayniteGameInfo gameInfo)
        {
            events.OnGameStarted(gameInfo);
        }

        public void SendGameStopped(PlayniteGameInfo gameInfo)
        {
            events.OnGameStopped(gameInfo);
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

        public void EnqueueAsyncTask(AsyncPlayniteTask task)
        {
            lock (threadLock)
            {
                asyncTaskQueue.Enqueue(task);
            }
        }
    }
}
