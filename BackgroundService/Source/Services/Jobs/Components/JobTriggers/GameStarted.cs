using BackgroundService.Source.Services.Jobs.Components.Common;
using Core.Playnite.Communication.Models;
using System;

namespace BackgroundService.Source.Services.Jobs.Components.JobTriggers
{
    internal class GameStarted : JobTrigger
    {
        public class GameStartedOptions
        {
            public PlayniteGameInfo Filter { get; set; }
        }

        private GameStartedOptions Options => GetOptions<GameStartedOptions>();

        private uint? eventListenerId = null;

        public GameStarted(JobTriggerAction action, object options) : base(action, options) { }

        protected override void OnOpen()
        {
            eventListenerId = Services.ThirdParty.Playnite.OnGameStarting(gameInfo =>
            {
                if (!Filters.MatchesWithFilter(gameInfo, Options.Filter))
                {
                    return;
                }

                ExecuteTrigger();
            });
        }

        protected override void OnClose()
        {
            if (eventListenerId != null)
            {
                Services.ThirdParty.Playnite.RemoveEventListener((uint)eventListenerId);
            }
        }
    }
}
