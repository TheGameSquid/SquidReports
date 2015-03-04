using System;

namespace SquidReports.DataCollector.Interface
{
    public interface ICollector
    {
        event EventHandler DataCollected;
        event EventHandler MessageLogged;
    }
}
