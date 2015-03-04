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

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string message, LogLevel logLevel)
        {
            this.Message = message;
            this.LogLevel = logLevel;
        }

        public string Message       { get; set; }
        public LogLevel LogLevel    { get; set; }
    }
}
