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
            CollectComputerGroups();
            CollectComputerGroupMembers();
            CollectBaselines();
            CollectBaselineResults();
            CollectActions();
            CollectActionDetails();
            CollectActionResults();
            CollectAnalyses();
            CollectAnalysisProperties();
            CollectAnalysisPropertyResults();
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

        public void CollectActionDetails()
        {
            try
            {
                List<ActionDetail> actionDetails = API.GetActionDetails();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} ActionDetails!", actionDetails.Count));

                foreach (ActionDetail actionDetail in actionDetails)
                {
                    DbRelay.Put<ActionDetail>(actionDetail);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectActionResults()
        {
            try
            {
                List<ActionResult> actionResults = API.GetActionResults();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} ActionResults!", actionResults.Count));

                foreach (ActionResult actionResult in actionResults)
                {
                    DbRelay.Put<ActionResult>(actionResult);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectAnalyses()
        {
            try
            {
                List<Analysis> analyses = API.GetAnalyses();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} Analyses!", analyses.Count));

                foreach (Analysis analysis in analyses)
                {
                    DbRelay.Put<Analysis>(analysis);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectAnalysisProperties()
        {
            try
            {
                List<AnalysisProperty> analysisProperties = API.GetAnalysisProperties();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} AnalysisProperties!", analysisProperties.Count));

                foreach (AnalysisProperty analysisProperty in analysisProperties)
                {
                    DbRelay.Put<AnalysisProperty>(analysisProperty);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectAnalysisPropertyResults()
        {
            try
            {
                List<AnalysisPropertyResult> analysisPropertyResults = API.GetAnalysisPropertyResults();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} AnalysisPropertyResults!", analysisPropertyResults.Count));

                foreach (AnalysisPropertyResult analysisPropertyResult in analysisPropertyResults)
                {
                    DbRelay.Put<AnalysisPropertyResult>(analysisPropertyResult);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectBaselines()
        {
            try
            {
                List<Baseline> baselines = API.GetBaselines();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} Baselines!", baselines.Count));

                foreach (Baseline baseline in baselines)
                {
                    DbRelay.Put<Baseline>(baseline);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectBaselineResults()
        {
            try
            {
                List<BaselineResult> baselineResults = API.GetBaselineResults();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} BaselineResults!", baselineResults.Count));

                foreach (BaselineResult baselineResult in baselineResults)
                {
                    DbRelay.Put<BaselineResult>(baselineResult);
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

        public void CollectComputerGroups()
        {
            try
            {
                List<ComputerGroup> groups = API.GetComputerGroups();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} ComputerGroups!", groups.Count));

                foreach (ComputerGroup group in groups)
                {
                    DbRelay.Put<ComputerGroup>(group);
                }
            }
            catch (Exception e)
            {
                this.Logger.LogException(LogLevel.Error, e.Message, e);
            }
        }

        public void CollectComputerGroupMembers()
        {
            try
            {
                List<ComputerGroupMember> groupMembers = API.GetGroupMembers();
                this.Logger.LogMessage(LogLevel.Info, String.Format("Collected {0} ComputerGroupMembers!", groupMembers.Count));

                foreach (ComputerGroupMember groupMember in groupMembers)
                {
                    DbRelay.Put<ComputerGroupMember>(groupMember);
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
