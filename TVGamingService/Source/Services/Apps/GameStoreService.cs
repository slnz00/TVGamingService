using System;
using System.Collections.Generic;
using TVGamingService.Source.Models;
using TVGamingService.Source.Providers;
using TVGamingService.Source.Utils;

namespace TVGamingService.Source.Services.Apps
{
    internal class GameStoreService : BaseService
    {
        public enum GameStoreTypes
        {
            STEAM,
            EPIC_GAMES
        }

        private IReadOnlyDictionary<GameStoreTypes, AppConfig> stores;

        public GameStoreService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            var config = Services.Config.GetConfig();

            stores = new Dictionary<GameStoreTypes, AppConfig>
            {
               { GameStoreTypes.STEAM, config.Apps.Steam },
               { GameStoreTypes.EPIC_GAMES, config.Apps.EpicGames },
            };
        }

        public void CloseGameStore(GameStoreTypes type)
        {
            if (!stores.ContainsKey(type))
            {
                var typeName = Enum.GetName(typeof(GameStoreTypes), type);
                Logger.Error($"Failed to close game store {typeName}: store definition is not found");
                return;
            }

            Logger.Debug($"Closing game store: {type}");

            var store = stores[type];
            ProcessUtils.CloseProcess(store.ProcessName, true);
        }
    }
}
