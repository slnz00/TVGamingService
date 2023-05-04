using Core.Utils;
using System;

namespace BackgroundService.Source.Services.Jobs.Components.JobActions
{
    internal class Log : JobAction
    {
        public enum LogLevel
        {
            Info,
            Debug,
            Warn,
            Error,
        }

        public class LogOptions
        {
            public string Message { get; set; }
            public string Level { get; set; } = "Info";
        }

        private LogOptions Options => GetOptions<LogOptions>();

        public Log(object options) : base(options) { }

        protected override void OnOptionsValidation()
        {
            var allowedLevels = EnumUtils.GetNames<LogLevel>();
            if (!allowedLevels.Contains(Options.Level))
            {
                throw new InvalidOperationException($"Unknown log level option (Options.Level): {Options.Level}");
            }
        }

        protected override void OnExecution()
        {
            var logLevel = EnumUtils.GetValue<LogLevel>(Options.Level);
            var message = Options.Message;

            switch (logLevel)
            {
                case LogLevel.Info:
                    Logger.Info(message);
                    break;
                case LogLevel.Debug:
                    Logger.Debug(message);
                    break;
                case LogLevel.Warn:
                    Logger.Warn(message);
                    break;
                case LogLevel.Error:
                    Logger.Error(message);
                    break;
            }
        }

        protected override void OnReset() { }
    }
}
