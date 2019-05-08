namespace NodeLibrary.Native
{
    public enum LoggerType
    {
        Info,
        Error,
        Debug,
        Trace,
        Warning
    }

    public interface ILogger
    {
        void Message(string tag, string msg, LoggerType type);
    }
}
