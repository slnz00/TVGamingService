using BackgroundService.Source.Providers;
using Core.Models.Configs;
using Core.Utils;
using System;
using System.Collections.Generic;

namespace BackgroundService.Source.Services.ThirdParty
{
    internal class GameStoreService : Service
    {
        private readonly object threadLock = new object();

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
            UpdateStoreConfigs();

            Services.Config.ConfigWatcher.OnChanged(() =>
            {
                UpdateStoreConfigs();
            });
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
            if (store == null)
            {
                return;
            }

            Logger.Info($"Closing game store: {type}");

            ProcessUtils.CloseProcess(store.ProcessName, true);
        }

        private void UpdateStoreConfigs()
        {
            lock (threadLock)
            {
                var config = Services.Config.GetConfig();

                storeConfigs = new Dictionary<GameStoreTypes, AppConfig>
                {
                   { GameStoreTypes.Steam, config.ThirdParty.Steam },
                   { GameStoreTypes.EpicGames, config.ThirdParty.EpicGames },
                   { GameStoreTypes.BattleNet, config.ThirdParty.BattleNet },
                };
            }
        }
    }
}
