using System;

namespace SquidReports.DataCollector.Interface
{
    public interface ICollector
    {
        event EventHandler DataCollected;
        event EventHandler MessageLogged;

        DbRelay DbRelay { get; set; }

        void Init(DbRelay dbRelay);
        void Execute();
    }
}
