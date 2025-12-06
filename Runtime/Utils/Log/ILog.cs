namespace T2FGame.Client.Utils
{
    public interface ILog
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
