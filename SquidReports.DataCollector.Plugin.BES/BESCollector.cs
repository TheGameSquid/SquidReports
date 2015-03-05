using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using SquidReports.DataCollector.Interface;
using SquidReports.DataCollector.Plugin.BES.API;
using SquidReports.DataCollector.Plugin.BES.Model;

namespace SquidReports.DataCollector.Plugin.BES
{
    public class BESCollector : ICollector
    {
        public event EventHandler DataCollected;
        public event EventHandler MessageLogged;

        public IDbRelay DbRelay { get; set; }

        public BesApi API { get; set; }

        public void Init(IDbRelay dbRelay)
        {
            // Let's make sure to explicitly call the .dll.config file
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            this.API = new BesApi(
                                    appConfig.AppSettings.Settings["ApiEndpoint"].Value,
                                    appConfig.AppSettings.Settings["ApiUser"].Value,
                                    appConfig.AppSettings.Settings["ApiPassword"].Value
                                );
            this.DbRelay = dbRelay;
        }

        public void Execute()
        {
            CollectActions();
        }

        public void CollectActions()
        {
            MessageLogged(this, new LogEventArgs("Collecting Actions", LogLevel.Debug));
            //List<Model.Action> actions = API.GetActions();

            //foreach (Model.Action action in actions)
            //{
            //    DataCollected(this, new CollectorEventArgs(action));
            //}
            IEnumerable<Model.Action> actions = DbRelay.Get<Model.Action>(new { ActionID = 63 });
            Console.WriteLine("");
        }
    }
}
