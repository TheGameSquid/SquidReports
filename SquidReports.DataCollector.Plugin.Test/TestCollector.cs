using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.Test
{
    public class TestCollector : ICollector
    {
        public IDbRelay DbRelay { get; set; }
        public ILogManager LogManager { get; set; }

        public void Init(ILogManager logManager, IDbRelay dbRelay)
        {
            
        }

        public void Execute()
        {

        }
    }
}
