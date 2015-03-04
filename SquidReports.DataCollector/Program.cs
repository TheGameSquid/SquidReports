using System;
using System.Configuration;
using System.Runtime.Serialization.Formatters;
using SquidReports.DataCollector.Interface;
using SquidReports.DataCollector.Plugin.Test;
using SquidReports.DataCollector.Plugin.BES;
using Newtonsoft.Json;

namespace SquidReports.DataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            BESCollector collector = new BESCollector();
            collector.Init(new DbRelay(ConfigurationManager.ConnectionStrings["DB"].ConnectionString));
            collector.DataCollected += DataTest;
            collector.MessageLogged += LogTest;
            collector.Execute();

            Console.Read();
        }

        public static void DataTest(object sender, EventArgs e)
        {
            CollectorEventArgs args = (CollectorEventArgs)e;
            ICollectible data = args.Data;

            Console.WriteLine("Data: {0}", data);
        }

        public static void LogTest(object sender, EventArgs e)
        {
            LogEventArgs args = (LogEventArgs)e;
            Console.WriteLine(args.Message);
        }
    }
}
