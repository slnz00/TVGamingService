namespace BackgroundService.Source
{
    internal static class InternalSettings
    {
        public static readonly string WindowTitle = "GameEnvironmentService";

        public static readonly string DesktopNameGameEnvironment = "GameEnvironment";

        public static readonly int BackupDefaultAmount = 5;

        public static readonly uint TimeoutHotkeyAction = 500;
        public static readonly uint TimeoutSetDefaultAudio = 10_000;

        public static readonly bool ConsoleDefaultVisibility = false;

        public static readonly string LogFileName = "BackgroundService.log";
        public static readonly string LogFileNameArchive = "BackgroundService.{#}.log";
        public static readonly int LogFileMaxSize = 4_000_000;
        public static readonly int LogFileMaxArchives = 3;
    }
}
