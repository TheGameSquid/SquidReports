using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES
{
    public class BESCollector : ICollector
    {
        public event EventHandler DataCollected;
        public event EventHandler MessageLogged;

        public void Execute()
        {

        }
    }
}
