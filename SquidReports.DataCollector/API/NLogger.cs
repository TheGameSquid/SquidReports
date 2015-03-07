using System;
using SquidReports.DataCollector.Interface;
using NLog;

namespace SquidReports.DataCollector.API
{
    public class NLogger : ILogger
    {
        public NLogger(Logger logger)
        {
            this.Logger = logger;
        }

        public Logger Logger { get; set; }

        public void LogMessage(SquidReports.DataCollector.Interface.LogLevel logLevel, string message)
        {
            switch (logLevel)
            {
                case SquidReports.DataCollector.Interface.LogLevel.Off:
                    this.Logger.Log(NLog.LogLevel.Off, message);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Trace:
                    this.Logger.Log(NLog.LogLevel.Trace, message);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Debug:
                    this.Logger.Log(NLog.LogLevel.Debug, message);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Info:
                    this.Logger.Log(NLog.LogLevel.Info, message);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Warn:
                    this.Logger.Log(NLog.LogLevel.Warn, message);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Error:
                    this.Logger.Log(NLog.LogLevel.Error, message);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Fatal:
                    this.Logger.Log(NLog.LogLevel.Fatal, message);
                    break;
            }
        }

        public void LogException(SquidReports.DataCollector.Interface.LogLevel logLevel, string message, Exception e)
        {
            switch (logLevel)
            {
                case SquidReports.DataCollector.Interface.LogLevel.Off:
                    this.Logger.Log(NLog.LogLevel.Off, message, e);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Trace:
                    this.Logger.Log(NLog.LogLevel.Trace, message, e);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Debug:
                    this.Logger.Log(NLog.LogLevel.Debug, message, e);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Info:
                    this.Logger.Log(NLog.LogLevel.Info, message, e);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Warn:
                    this.Logger.Log(NLog.LogLevel.Warn, message, e);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Error:
                    this.Logger.Log(NLog.LogLevel.Error, message, e);
                    break;
                case SquidReports.DataCollector.Interface.LogLevel.Fatal:
                    this.Logger.Log(NLog.LogLevel.Fatal, message, e);
                    break;
            }
        }
    }
}
