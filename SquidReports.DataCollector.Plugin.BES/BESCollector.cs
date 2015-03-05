﻿using System;
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
        public IDbRelay DbRelay { get; set; }
        public ILogger Logger { get; set; }

        public BesApi API { get; set; }

        public void Init(ILogger logger, IDbRelay dbRelay)
        {
            // Let's make sure to explicitly call the .dll.config file
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);

            this.API = new BesApi(
                                    appConfig.AppSettings.Settings["ApiEndpoint"].Value,
                                    appConfig.AppSettings.Settings["ApiUser"].Value,
                                    appConfig.AppSettings.Settings["ApiPassword"].Value
                                );
            this.Logger = logger;
            this.DbRelay = dbRelay;
        }

        public void Execute()
        {
            CollectActions();
        }

        public void CollectActions()
        {
            try
            {
                List<Model.Action> actions = API.GetActions();
            }
            catch (Exception e)
            {
                Logger.LogException(e.Message, e);
            }
            

            //foreach (Model.Action action in actions)
            //{
            //    DataCollected(this, new CollectorEventArgs(action));
            //}
            IEnumerable<Model.Action> dbActions = DbRelay.Get<Model.Action>(new { ActionID = 63 });
            dbActions = DbRelay.Get<Model.Action>();
            DbRelay.Put<Model.Action>(dbActions.ElementAt<Model.Action>(0));
            Console.WriteLine("");
        }
    }
}
