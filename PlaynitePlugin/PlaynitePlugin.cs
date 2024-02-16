using Core.Playnite.Communication.Models;
using Core.Playnite.Communication.Services;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using PlaynitePlugin.Communication;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace PlaynitePlugin
{
    public class PlaynitePlugin : GenericPlugin
    {
        private static readonly ILogger Logger = LogManager.GetLogger();
        private static readonly ServiceClient<IPlayniteAppService> GameEnvironmentService = new ServiceClient<IPlayniteAppService>();

        // GameId -> Game path
        private static readonly Dictionary<string, string> GamePaths = new Dictionary<string, string>();

        private PlaynitePluginSettingsViewModel Settings { get; set; }

        public override Guid Id { get; } = Guid.Parse("6dc9c7f6-44f0-4e09-b294-52aac097750a");

        public PlaynitePlugin(IPlayniteAPI api) : base(api)
        {
            Settings = new PlaynitePluginSettingsViewModel(this);
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

            Logger.Info($"GameEnvironmentService: Sending GameStarted event, game: {gameInfo.Name}");

            GameEnvironmentService.Service.SendGameStarted(gameInfo);
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            SetGamePath(args.Game, args.SourceAction?.Path);

            var gameInfo = GetGameInfo(args.Game);

            Logger.Info($"GameEnvironmentService: Sending GameStarting event, game: {gameInfo.Name}");

            GameEnvironmentService.Service.SendGameStarting(gameInfo);
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            var gameInfo = GetGameInfo(args.Game);

            Logger.Info($"GameEnvironmentService: Sending GameStopped event, game: {gameInfo.Name}");

            GameEnvironmentService.Service.SendGameStopped(gameInfo);
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args)
        {
            Logger.Info("GameEnvironmentService: Plugin is loaded");
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args)
        {
        }

        public override void OnLibraryUpdated(OnLibraryUpdatedEventArgs args)
        {
        }

        public override ISettings GetSettings(bool firstRunSettings)
        {
            return Settings;
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
            if (game.Platforms == null || game.Platforms.Count == 0)
            {
                return null;
            }

            return game.Platforms[0].Name;
        }

        private string GetGamePath(Game game)
        {
            if (!GamePaths.ContainsKey(game.GameId))
            {
                return null;
            }

            return GamePaths[game.GameId];
        }

        private void SetGamePath(Game game, string path)
        {
            var gameId = game.GameId;

            GamePaths[gameId] = path;
        }
    }
}