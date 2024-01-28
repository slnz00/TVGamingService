using Core.Utils;

namespace BackgroundService.Source
{
    internal static class InternalSettings
    {
        public static readonly string WINDOW_TITLE = "TVGamingService";

        public static readonly string DESKTOP_TV_NAME = "TVGaming";

        public static readonly string CURSOR_EMPTY_FILE_NAME = "tv-gaming-empty-cursor.cur";

        public static readonly string PATH_CONFIG = FSUtils.GetAbsolutePath("config.json");
        public static readonly string PATH_JOBS_CONFIG = FSUtils.GetAbsolutePath("jobs.config.json");
        public static readonly string PATH_GAME_CONFIGS = FSUtils.GetAbsolutePath("game-configs.json");
        public static readonly string PATH_VIRTUAL_DESKTOP_W10 = FSUtils.GetAbsolutePath("Binaries", "vd_win10.exe");
        public static readonly string PATH_VIRTUAL_DESKTOP_W11 = FSUtils.GetAbsolutePath("Binaries", "vd_win11.exe");
        public static readonly string PATH_NIRCMD = FSUtils.GetAbsolutePath("Binaries", "nircmd.exe");
        public static readonly string PATH_EMPTY_CURSOR = FSUtils.GetAbsolutePath("Resources", CURSOR_EMPTY_FILE_NAME);
        public static readonly string PATH_DATA_STATES = FSUtils.GetAbsolutePath("Data", "State.json");
        public static readonly string PATH_DATA_GAME_CONFIGS = FSUtils.GetAbsolutePath("Data", "GameConfigs");
        public static readonly string PATH_DATA_BACKUPS = FSUtils.GetAbsolutePath("Data", "Backups");

        public static readonly int BACKUP_DEFAULT_AMOUNT = 5;

        public static readonly uint TIMEOUT_HOTKEY_ACTION = 1000;
        public static readonly uint TIMEOUT_DEFAULT_SOUND_DEVICE_SET = 25_000;

        public static readonly bool CONSOLE_DEFAULT_VISIBILITY = false;

        public static readonly string LOG_FILE_NAME = "BackgroundService.log";
        public static readonly string LOG_FILE_NAME_ARCHIVE = "BackgroundService.{#}.log";
        public static readonly int LOG_FILE_MAX_SIZE = 4_000_000;
        public static readonly int LOG_FILE_MAX_ARCHIVES = 3;
    }
}
