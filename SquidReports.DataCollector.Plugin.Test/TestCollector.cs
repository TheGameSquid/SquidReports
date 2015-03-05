using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.Test
{
    public class TestCollector : ICollector
    {
        public IDbRelay DbRelay { get; set; }
        public ILogger Logger { get; set; }

        public void Init(ILogger logger, IDbRelay dbRelay)
        {
            
        }

        public void Execute()
        {

        }
    }
}
