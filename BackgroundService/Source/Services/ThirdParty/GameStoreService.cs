using System;
using System.Collections.Generic;
using BackgroundService.Source.Providers;
using BackgroundService.Source.Services.Configs.Models;
using Core.Utils;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class GameStoreService : Service
    {
        public enum GameStoreTypes
        {
            Steam,
            EpicGames,
            BattleNet
        }

        private IReadOnlyDictionary<GameStoreTypes, AppConfig> storeConfigs;

        public GameStoreService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            var config = Services.Config.GetConfig();

            storeConfigs = new Dictionary<GameStoreTypes, AppConfig>
            {
               { GameStoreTypes.Steam, config.ThirdParty.Steam },
               { GameStoreTypes.EpicGames, config.ThirdParty.EpicGames },
               { GameStoreTypes.BattleNet, config.ThirdParty.BattleNet },
            };
        }

        public void CloseAllGameStores()
        {
            var stores = (GameStoreTypes[])Enum.GetValues(typeof(GameStoreTypes));

            Array.ForEach(stores, CloseGameStore);
        }

        public void CloseGameStore(GameStoreTypes type)
        {
            if (!storeConfigs.ContainsKey(type))
            {
                var storeName = Enum.GetName(typeof(GameStoreTypes), type);
                Logger.Error($"Failed to close game store {storeName}: store definition is not found");
                return;
            }

            var store = storeConfigs[type];
            if (store == null) {
                return;
            }

            Logger.Debug($"Closing game store: {type}");

            ProcessUtils.CloseProcess(store.ProcessName, true);
        }
    }
}
