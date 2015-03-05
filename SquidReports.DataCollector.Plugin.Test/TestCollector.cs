using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.Test
{
    public class TestCollector : ICollector
    {
        public event EventHandler DataCollected;
        public event EventHandler MessageLogged;

        public IDbRelay DbRelay { get; set; }

        public void Init(IDbRelay dbRelay)
        {

        }

        public void Execute()
        {
            for (int index = 0; index < 10; index++)
            {
                //DataCollected(this, new CollectorEventArgs("hello"));
                TestData test = new TestData(index);
                DataCollected(this, new CollectorEventArgs(test));
                MessageLogged(this, new LogEventArgs(String.Format("Owla! {0}", index), LogLevel.Debug));
            }
        }
    }
}
