using Android.Util;
using NodeLibrary.Native;

namespace Core.Droid
{
    public class Logger : ILogger
    {
        public void Message(string tag, string msg, LoggerType type)
        {
            switch (type)
            {
                case LoggerType.Trace:
                case LoggerType.Debug:
                    Log.Debug(tag, msg);
                    break;
                case LoggerType.Error:
                    Log.Error(tag, msg);
                    break;
                case LoggerType.Info:
                    Log.Info(tag, msg);
                    break;
                case LoggerType.Warning:
                    Log.Warn(tag, msg);
                    break;                    
            }
        }
    }
}