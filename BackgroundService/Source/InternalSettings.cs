using Core.Utils;

namespace BackgroundService.Source
{
    internal static class InternalSettings
    {
        public static readonly string WINDOW_TITLE = "GamingService";

        public static readonly string DESKTOP_GAME_ENVIRONMENT_NAME = "GameEnvironment";

        public static readonly string CURSOR_EMPTY_FILE_NAME = "tv-gaming-empty-cursor.cur";

        public static readonly string PATH_CONFIG = FSUtils.GetAbsolutePath("Configs", "Config.json");
        public static readonly string PATH_CONFIG_JOBS = FSUtils.GetAbsolutePath("Configs", "Jobs.json");
        public static readonly string PATH_RESOURCE_EMPTY_CURSOR = FSUtils.GetAbsolutePath("Resources", CURSOR_EMPTY_FILE_NAME);
        public static readonly string PATH_DATA_STATES = FSUtils.GetAbsolutePath("Data", "State.json");
        public static readonly string PATH_DATA_GAME_CONFIGS = FSUtils.GetAbsolutePath("Data", "GameConfigs");
        public static readonly string PATH_DATA_BACKUPS = FSUtils.GetAbsolutePath("Data", "Backups");

        public static readonly int BACKUP_DEFAULT_AMOUNT = 5;

        public static readonly uint TIMEOUT_HOTKEY_ACTION = 500;
        public static readonly uint TIMEOUT_SET_DEFAULT_AUDIO_DEVICE = 10_000;

        public static readonly bool CONSOLE_DEFAULT_VISIBILITY = false;

        public static readonly string LOG_FILE_NAME = "BackgroundService.log";
        public static readonly string LOG_FILE_NAME_ARCHIVE = "BackgroundService.{#}.log";
        public static readonly int LOG_FILE_MAX_SIZE = 4_000_000;
        public static readonly int LOG_FILE_MAX_ARCHIVES = 3;
    }
}
