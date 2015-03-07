using System;

namespace SquidReports.DataCollector.Interface
{
    // Use to indicate loglevel, maps to both Log4Net and NLog loglevels
    public enum LogLevel
    {
        Off,
        Fatal,
        Error,
        Warn,
        Info,
        Debug,
        Trace
    }

    public interface ILogger
    {
        void LogMessage(LogLevel logLevel, string message);
        void LogException(LogLevel logLevel, string message, Exception e);
    }
}
