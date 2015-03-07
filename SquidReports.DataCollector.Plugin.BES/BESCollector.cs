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
                                    dbRelay,
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
            CollectSites();
            CollectComputers();
            CollectActions();
        }

        public void CollectActions()
        {
            try
            {
                List<Model.Action> actions = API.GetActions();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} Actions!", actions.Count));

                foreach (Model.Action action in actions)
                {
                    DbRelay.Put<Model.Action>(action);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectComputers()
        {
            try
            {
                List<Computer> computers = API.GetComputers();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} Computers!", computers.Count));

                foreach (Computer computer in computers)
                {
                    DbRelay.Put<Computer>(computer);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectSites()
        {
            try
            {
                List<Site> sites = API.GetSites();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} Sites!", sites.Count));

                foreach (Site site in sites)
                {
                    DbRelay.Put<Site>(site);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }
    }
}
