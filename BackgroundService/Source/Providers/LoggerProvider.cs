using Core.Models;
using System;

namespace BackgroundService.Source.Providers
{
    internal class LoggerProvider : ILogger
    {
        private static readonly NLog.Logger NLogger = NLog.LogManager.GetLogger("");
        public readonly static LoggerProvider Global = new LoggerProvider("Global");

        private string name;

        public string Name => name;

        static LoggerProvider()
        {
            SetupNLog();
        }

        public LoggerProvider(string name)
        {
            this.name = name;
        }

        public void Plain(string message)
        {
            NLogger.Info(message);
        }

        public void Info(string message)
        {
            NLogger.Info(GetLogContent("INFO", message));
        }

        public void Debug(string message)
        {
            NLogger.Debug(GetLogContent("DEBUG", message));
        }

        public void Warn(string message)
        {
            NLogger.Warn(GetLogContent("WARN", message));
        }

        public void Error(string message)
        {
            NLogger.Error(GetLogContent("ERROR", message));
        }

        public void LogEmptyLine()
        {
            Console.WriteLine();
        }

        private string GetLogContent(string logType, string message)
        {
            return $"[{logType}] [{name}] {message}";
        }

        private static void SetupNLog()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                ArchiveAboveSize = InternalSettings.LOG_FILE_MAX_SIZE,
                MaxArchiveFiles = InternalSettings.LOG_FILE_MAX_ARCHIVES,
                FileName = InternalSettings.LOG_FILE_NAME,
                ArchiveFileName = InternalSettings.LOG_FILE_NAME_ARCHIVE,
                ArchiveNumbering = NLog.Targets.ArchiveNumberingMode.Rolling,
                Layout = "[${longdate}] ${message}"
            };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole")
            {
                Layout = "${message}"
            };

            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);

            NLog.LogManager.Configuration = config;
        }
    }
}
