using BackgroundService.Source.Services.Jobs.Components.Common;
using Core.Playnite.Communication.Models;

namespace BackgroundService.Source.Services.Jobs.Components.JobTriggers
{
    internal class GameStoppedOptions
    {
        public PlayniteGameInfo Filter { get; set; }
    }

    internal class GameStopped : JobTrigger
    {
        private GameStoppedOptions Options => GetOptions<GameStoppedOptions>();

        private uint? eventListenerId = null;

        public GameStopped(JobTriggerAction action, object options) : base(action, options) { }

        protected override void OnOpen()
        {
            eventListenerId = Services.ThirdParty.Playnite.OnGameStopped(gameInfo =>
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
