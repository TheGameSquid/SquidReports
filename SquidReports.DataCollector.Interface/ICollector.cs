using System;

namespace SquidReports.DataCollector.Interface
{
    public interface ICollector
    {
        IDbRelay DbRelay { get; set; }
        ILogger Logger { get; set; }
        CollectorType CollectorType { get; set; }

        void Init(ILogger logger, IDbRelay dbRelay);
        void Execute();
    }
}
