using BackgroundService.Source.Controllers.Environment.Components;
using BackgroundService.Source.Providers;
using Core;
using Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackgroundService.Source.Services
{
    internal class GameConfigService : Service
    {
        private readonly Dictionary<string, string> gameConfigPathHashCache = new Dictionary<string, string>();

        private List<string> GameConfigPaths => Services.Config.GetConfig().GameConfigPaths;

        public GameConfigService(ServiceProvider services) : base(services) { }

        public void SaveGameConfigsForEnvironment(Environments env)
        {
            GameConfigPaths.ForEach(configPath =>
            {
                try
                {
                    Logger.Debug($"Saving game config for environment ({EnumUtils.GetName(env)}): {configPath}");

                    string savePath = GetGameConfigDataPath(configPath, env);

                    if (!File.Exists(configPath))
                    {
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
            GameConfigPaths.ForEach(configPath =>
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

        private string GetGameConfigDataPath(string configPath, Environments env)
        {
            var pathHash = GetGameConfigPathHash(configPath);
            var directory = Path.Combine(SharedSettings.Paths.DataGameConfigs, pathHash);
            var envName = EnumUtils.GetName(env);

            return Path.Combine(directory, envName);
        }

        private string GetGameConfigPathHash(string configPath)
        {
            var pathIndex = Path.GetFullPath(configPath).ToLower().Replace(@"\", "/").Trim();

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
