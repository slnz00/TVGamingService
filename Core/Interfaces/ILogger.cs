namespace Core.Interfaces
{
    public interface ILogger
    {
        void Plain(string message);
        void Info(string message);
        void Debug(string message);
        void Warn(string message);
        void Error(string message);
        void LogEmptyLine();
    }
}
