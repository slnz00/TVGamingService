using Core.Playnite.Communication.Models;
using Core.Playnite.Communication.Services;
using System;
using System.ServiceModel;

namespace BackgroundService.Source.Services.ThirdParty.Playnite.Communication.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    internal class PlayniteAppEventsService : IPlayniteAppEventsService
    {
        public class Events {
            public Action<PlayniteGameInfo> OnGameStarting { get; set; }
            public Action<PlayniteGameInfo> OnGameStarted { get; set; }
            public Action<PlayniteGameInfo> OnGameStopped { get; set; }
        }

        private readonly Events events;

        public PlayniteAppEventsService(Events events)
        {
            this.events = events;
        }

        public void SendGameStarting(PlayniteGameInfo gameInfo)
        {
            events.OnGameStarting(gameInfo);
        }

        public void SendGameStarted(PlayniteGameInfo gameInfo)
        {
            events.OnGameStarting(gameInfo);
        }

        public void SendGameStopped(PlayniteGameInfo gameInfo)
        {
            events.OnGameStopped(gameInfo);
        }
    }
}
