using System;
using System.Configuration;
using System.Runtime.Serialization.Formatters;
using SquidReports.DataCollector.API;
using SquidReports.DataCollector.Interface;
using SquidReports.DataCollector.Plugin.Test;
using SquidReports.DataCollector.Plugin.BES;

namespace SquidReports.DataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            BESCollector collector = new BESCollector();
            IDbRelay dbRelay = new DbRelay(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
            ILogger nLogger = new NLogger();
            collector.Init(nLogger, dbRelay);
            collector.Execute();

            Console.Read();
        }
    }
}
