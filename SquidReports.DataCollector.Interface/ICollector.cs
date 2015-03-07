using System;

namespace SquidReports.DataCollector.Interface
{
    public interface ICollector
    {
        IDbRelay DbRelay { get; set; }
        ILogManager LogManager { get; set; }

        void Init(ILogManager logManager, IDbRelay dbRelay);
        void Execute();
    }
}
