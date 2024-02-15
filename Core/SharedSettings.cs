using Core.Utils;
using System;

namespace Core
{
    public static class SharedSettings
    {
        public static class Paths
        {
            public static readonly string StartupScript = FSUtils.GetAbsolutePath("GameEnvironmentService.bat");
            public static readonly string Config = FSUtils.GetAbsolutePath("Configs", "Config.json");
            public static readonly string JobsConfig = FSUtils.GetAbsolutePath("Configs", "Jobs.json");
            public static readonly string ResourceEmptyCursor = FSUtils.GetAbsolutePath("Resources", "gameenv-empty-cursor.cur");
            public static readonly string DataStates = FSUtils.GetAbsolutePath("Data", "State.json");
            public static readonly string DataGameConfigs = FSUtils.GetAbsolutePath("Data", "GameConfigs");
            public static readonly string DataBackups = FSUtils.GetAbsolutePath("Data", "Backups");
        }

        public static class ProcessGuids
        {
            public static readonly Guid BackgroundService = new Guid("929f0e08-977a-4c90-b825-6823e4f75d5a");
        }

        public static class Playnite
        {
            public static readonly Guid PIPE_GUID = new Guid("4dbc9aba-f0c2-4ba0-a637-5ca12f3a621a");
            public static readonly string PIPE_BASE_URL = $"net.pipe://localhost/TVGamingService-Playnite-{PIPE_GUID}";

            public static string GetServiceAddress<TService>() where TService : class
            {
                return $"{PIPE_BASE_URL}/{typeof(TService).Name}";
            }
        }
    }
}
