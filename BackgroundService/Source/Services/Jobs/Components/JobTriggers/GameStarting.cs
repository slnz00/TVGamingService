using BackgroundService.Source.Services.Jobs.Components.Common;
using Core.Models.Playnite;

namespace BackgroundService.Source.Services.Jobs.Components.JobTriggers
{
    internal class GameStarting : JobTrigger
    {
        public class GameStartingOptions
        {
            public PlayniteGameInfo Filter { get; set; }
        }

        private GameStartingOptions Options => GetOptions<GameStartingOptions>();

        private uint? eventListenerId = null;

        public GameStarting(JobTriggerAction action, object options) : base(action, options) { }

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
