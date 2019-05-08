using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using NodeLibrary.Native;

namespace NodeLibrary
{
    public class Log
    {
        public static List<ILogger> AdditionalLoggers { get; } = new List<ILogger>();
        private static ISet<LoggerType> LogToDatabaseSet { get; } = new HashSet<LoggerType>();
        public void AddLogToDatabase(LoggerType type)
        {
            LogToDatabaseSet.Add(type);
        }
        public void RemoveLogToDatabase(LoggerType type)
        {
            LogToDatabaseSet.Remove(type);
        }

        private static void LogMessage(Exception message, string tag, LoggerType type, string path, int lineNumber)
        {
            var b = new StringBuilder();
            b.AppendLine(message.Message);
            b.Append(message.StackTrace);
            LogMessage(b.ToString(),tag,type,path,lineNumber);
        }
        private static void LogMessage(string message, string tag, LoggerType type, string path, int lineNumber)
        {
            var b = new StringBuilder();
            if (lineNumber > 0)
                b.Append($"#{lineNumber}: ");
            b.AppendLine(message);
            b.AppendLine(path);

            foreach (var logger in AdditionalLoggers)
            {
                logger?.Message(tag, b.ToString(), type);
            }
            //TODO fix this
            //if (LogToDatabaseSet.Contains(type))
            //    SettingsData.Default.AddLog(type.ToString(), tag, b.ToString());
        }
        public static void Info(Exception message, 
            [CallerMemberName] string tag = "INFO", 
            [CallerFilePath] string sourceFilePath = "", 
            [CallerLineNumber] int sourceLineNumber = 0) { LogMessage(message, tag, LoggerType.Info, sourceFilePath, sourceLineNumber); }
        public static void Error(Exception message, 
            [CallerMemberName] string tag = "ERROR",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { LogMessage(message, tag, LoggerType.Error, sourceFilePath, sourceLineNumber); }
        public static void Debug(Exception message, 
            [CallerMemberName] string tag = "DEBUG",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { LogMessage(message, tag, LoggerType.Debug, sourceFilePath, sourceLineNumber); }
        public static void Trace(Exception message, 
            [CallerMemberName] string tag = "TRACE",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { LogMessage(message, tag, LoggerType.Trace, sourceFilePath, sourceLineNumber); }
        public static void Warning(Exception message, 
            [CallerMemberName] string tag = "WARNING",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) { LogMessage(message, tag, LoggerType.Warning, sourceFilePath, sourceLineNumber); }

        public static void Info(string message,
            [CallerMemberName] string tag = "INFO",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        { LogMessage(message, tag, LoggerType.Info, sourceFilePath, sourceLineNumber); }
        public static void Error(string message,
            [CallerMemberName] string tag = "ERROR",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        { LogMessage(message, tag, LoggerType.Error, sourceFilePath, sourceLineNumber); }
        public static void Debug(string message,
            [CallerMemberName] string tag = "DEBUG",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        { LogMessage(message, tag, LoggerType.Debug, sourceFilePath, sourceLineNumber); }
        public static void Trace(string message,
            [CallerMemberName] string tag = "TRACE",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        { LogMessage(message, tag, LoggerType.Trace, sourceFilePath, sourceLineNumber); }
        public static void Warning(string message,
            [CallerMemberName] string tag = "WARNING",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        { LogMessage(message, tag, LoggerType.Warning, sourceFilePath, sourceLineNumber); }

    }
}
