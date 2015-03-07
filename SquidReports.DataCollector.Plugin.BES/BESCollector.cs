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
    [CollectorType(Type=CollectorType.Absolute)]
    public class BESCollector : ICollector
    {
        public IDbRelay DbRelay { get; set; }
        public ILogManager LogManager { get; set; }
        public ILogger Logger { get; set; }

        public BesApi API { get; set; }

        public void Init(ILogManager logManager, IDbRelay dbRelay)
        {
            // Let's make sure to explicitly call the .dll.config file
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            this.API = new BesApi(
                                    logManager,
                                    appConfig.AppSettings.Settings["ApiEndpoint"].Value,
                                    appConfig.AppSettings.Settings["ApiUser"].Value,
                                    appConfig.AppSettings.Settings["ApiPassword"].Value
                                );
            this.LogManager = logManager;
            this.Logger = this.LogManager.GetCurrentClassLogger();
            this.DbRelay = dbRelay;
        }

        public void Execute()
        {
            CollectActions();
        }

        public void CollectActions()
        {
            List<Model.Action> actions = API.GetActions();
            

            //foreach (Model.Action action in actions)
            //{
            //    DataCollected(this, new CollectorEventArgs(action));
            //}
            this.Logger.LogMessage(LogLevel.Debug, "Hello World from SquirReports!");

            Model.Action action1 = new Model.Action(1, "1", "Den eerste");
            Model.Action action2 = new Model.Action(2, "2", "Den tweede");
            //Model.Action action3 = new Model.Action(3, "3", "Den derde");
            DbRelay.Put<Model.Action>(action1);
            DbRelay.Put<Model.Action>(action2);
            //DbRelay.Put<Model.Action>(action3);
            Console.WriteLine("");
        }
    }
}
