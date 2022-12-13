using BackgroundService.Source.Controllers.EnvironmentControllers.Models;
using BackgroundService.Source.Providers;
using Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BackgroundService.Source.Services
{
    internal class GameConfigService : Service
    {
        private readonly string GAME_CONFIGS_PATH = InternalSettings.PATH_GAME_CONFIGS;
        private readonly string GAME_CONFIGS_DATA_PATH = InternalSettings.PATH_DATA_GAME_CONFIGS;

        private readonly Dictionary<string, string> gameConfigPathHashCache = new Dictionary<string, string>();

        private List<string> gameConfigPaths;

        public GameConfigService(ServiceProvider services) : base(services) { }

        protected override void OnInitialize()
        {
            LoadGameConfigPaths();
        }

        public void SaveGameConfigsForEnvironment(Environments env)
        {
            gameConfigPaths.ForEach(configPath =>
            {
                try
                {
                    Logger.Debug($"Saving game config for environment ({EnumUtils.GetName(env)}): {configPath}");

                    string savePath = GetGameConfigDataPath(configPath, env);

                    if (!File.Exists(configPath)) {
                        Logger.Warn($"Game config file does not exist: {configPath}");
                        return;
                    }

                    FSUtils.EnsureFileDirectory(savePath);
                    File.Copy(configPath, savePath, true);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to save game config: {ex}");
                }
            });
        }

        public void LoadGameConfigsForEnvironment(Environments env)
        {
            gameConfigPaths.ForEach(configPath =>
            {
                try
                {
                    Logger.Debug($"Loading game config for environment ({EnumUtils.GetName(env)}): {configPath}");

                    string savePath = GetGameConfigDataPath(configPath, env);

                    if (!File.Exists(savePath))
                    {
                        Logger.Warn($"Saved game config file does not exist for: {configPath}");
                        return;
                    }

                    FSUtils.EnsureFileDirectory(configPath);
                    File.Copy(savePath, configPath, true);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to load game config: {ex}");
                }
            });
        }

        private void LoadGameConfigPaths()
        {
            Logger.Debug($"Loading game config paths from JSON file: {GAME_CONFIGS_PATH}");

            string pathsJson = File.ReadAllText(GAME_CONFIGS_PATH, Encoding.Default);
            gameConfigPaths = JsonConvert.DeserializeObject<List<string>>(pathsJson);
        }

        private string GetGameConfigDataPath(string configPath, Environments env)
        {
            var pathHash = GetGameConfigPathHash(configPath);
            var directory = Path.Combine(GAME_CONFIGS_DATA_PATH, pathHash);
            var envName = EnumUtils.GetName(env);

            return Path.Combine(directory, envName);
        }

        private string GetGameConfigPathHash(string configPath)
        {
            var pathIndex = Path.GetFullPath(configPath).ToLower();

            if (gameConfigPathHashCache.ContainsKey(pathIndex))
            {
                return gameConfigPathHashCache[pathIndex];
            }

            var pathHash = HashUtils.SHA1(pathIndex);
            gameConfigPathHashCache[pathIndex] = pathHash;

            return pathHash;
        }
    }
}
