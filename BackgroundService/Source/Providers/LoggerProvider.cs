using System;

namespace BackgroundService.Source.Providers
{
    internal class LoggerProvider
    {
        private static readonly bool DEBUG_ENABLED = InternalSettings.DEBUG_ENABLED;

        private string name;

        public string Name => name;

        public LoggerProvider(string name)
        {
            this.name = name;
        }

        public void Info(string message)
        {
            Console.WriteLine(GetLogContent("INFO", message));
        }

        public void Debug(string message)
        {
            if (!DEBUG_ENABLED)
            {
                return;
            }

            Console.WriteLine(GetLogContent("DEBUG", message));
        }

        public void Warn(string message)
        {
            Console.WriteLine(GetLogContent("WARN", message));
        }

        public void Error(string message)
        {
            Console.WriteLine(GetLogContent("ERROR", message));
        }

        public void LogEmptyLine()
        {
            Console.WriteLine();
        }

        private string GetLogContent(string logType, string message)
        {
            return $"[{logType}] [{name}] {message}";
        }
    }
}
