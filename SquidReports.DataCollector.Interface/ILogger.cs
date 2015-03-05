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
        void LogMessage(string message, LogLevel logLevel);
        void LogException(string message, Exception e);
    }
}
