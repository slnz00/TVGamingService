﻿using Core.Playnite.Communication.Models;
using Core.Playnite.Communication.Models.Commands;
using Core.Playnite.Communication.Services;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlaynitePlugin.Communication;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;

namespace PlaynitePlugin
{
    public class PlaynitePlugin : GenericPlugin
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private static readonly ServiceClient<IPlayniteAppService> tvGamingService = new ServiceClient<IPlayniteAppService>();

        // GameId -> Game path
        private static readonly Dictionary<string, string> gamePaths = new Dictionary<string, string>();

        private PlaynitePluginSettingsViewModel settings { get; set; }

        private Timer asyncTaskTimer { get; set; } = null;

        public override Guid Id { get; } = Guid.Parse("6dc9c7f6-44f0-4e09-b294-52aac097750a");

        public PlaynitePlugin(IPlayniteAPI api) : base(api)
        {
            settings = new PlaynitePluginSettingsViewModel(this);
            Properties = new GenericPluginProperties
            {
                HasSettings = true
            };
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            var gameInfo = GetGameInfo(args.Game);

            logger.Info($"TVGamingService: Sending GameStarted event, game: {gameInfo.Name}");

            tvGamingService.Service.SendGameStarted(gameInfo);
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            SetGamePath(args.Game, args.SourceAction?.Path);

            var gameInfo = GetGameInfo(args.Game);

            logger.Info($"TVGamingService: Sending GameStarting event, game: {gameInfo.Name}");

            tvGamingService.Service.SendGameStarting(gameInfo);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var gameInfo = GetGameInfo(args.Game);

            logger.Info($"TVGamingService: Sending GameStopped event, game: {gameInfo.Name}");

            tvGamingService.Service.SendGameStopped(gameInfo);
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            logger.Info("TVGamingService: Plugin is loaded");
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return settings;
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new PlaynitePluginSettingsView();
        }

        private PlayniteGameInfo GetGameInfo(Game game)
        {
            return new PlayniteGameInfo
            {
                Id = game.GameId,
                Name = game.Name,
                Path = GetGamePath(game),
                Library = GetGameLibraryName(game),
                Platform = GetGamePlatformName(game),
            };
        }

        private string GetGameLibraryName(Game game)
        {
            if (game.PluginId == null)
            {
                return null;
            }

            var library = PlayniteApi.Addons.Plugins.Find(l => l.Id == game.PluginId);
            if (library == null)
            {
                return null;
            }

            return (string)library.GetType().GetProperty("Name")?.GetValue(library, null);
        }

        private string GetGamePlatformName(Game game)
        {
            if (game.Platforms == null || game.Platforms.Count == 0) {
                return null;
            }

            return game.Platforms[0].Name;
        }

        private string GetGamePath(Game game)
        {
            if (!gamePaths.ContainsKey(game.GameId))
            {
                return null;
            }

            return gamePaths[game.GameId];
        }

        private void SetGamePath(Game game, string path)
        {
            var gameId = game.GameId;

            gamePaths[gameId] = path;
        }

        private void StartAsyncTaskProcessor()
        {
            var delay = TimeSpan.FromSeconds(0);
            var sleep = TimeSpan.FromSeconds(1);

            asyncTaskTimer = new Timer((object _) =>
            {
                var task = tvGamingService.Service.GetAsyncTask();

                ExecuteAsyncTask(task);
            }, null, delay, sleep);
        }

        private void ExecuteAsyncTask(AsyncPlayniteTask task)
        {
            if (task == null)
            {
                return;
            }
        }
    }
}