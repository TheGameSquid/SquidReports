using System;
using Topshelf;

namespace SquidReports.DataCollector
{
    class PluginService
    {
        public PluginService()
        {
            NLogManager nLogManager = new NLogManager();
            this.PluginManager = new PluginManager(nLogManager);
        }

        public PluginManager PluginManager { get; set; }

        public bool Start()
        {
            this.PluginManager.Start();
            return true;
        }

        public bool Stop()
        {
            this.PluginManager.Stop();
            return true;
        }
    }
}
