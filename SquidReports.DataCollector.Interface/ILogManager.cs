using System;

namespace SquidReports.DataCollector.Interface
{
    public interface ILogManager
    {
        ILogger GetCurrentClassLogger();
    }
}
