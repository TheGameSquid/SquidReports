using System;
using System.Runtime.Serialization.Formatters;
using SquidReports.DataCollector.Interface;
using SquidReports.DataCollector.Plugin.Test;
using Newtonsoft.Json;

namespace SquidReports.DataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            TestCollector test = new TestCollector();
            test.DataCollected += DataTest;
            test.MessageLogged += LogTest;
            test.Execute();

            Console.Read();
        }

        public static void DataTest(object sender, EventArgs e)
        {
            CollectorEventArgs args = (CollectorEventArgs)e;
            //string data = args.DataJson;
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
