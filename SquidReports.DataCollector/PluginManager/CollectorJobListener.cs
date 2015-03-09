using System;
using SquidReports.DataCollector.Interface;
using Quartz;

namespace SquidReports.DataCollector
{
    class CollectorJobListener : IJobListener
    {
        public CollectorJobListener(ILogManager logManager)
        {
            this.Logger = logManager.GetCurrentClassLogger();
        }

        public string Name
        {
            get { return "CollectorJobListener"; }
        }

        public ILogger Logger { get; set; }

        public void JobToBeExecuted(IJobExecutionContext context)
        {
            this.Logger.LogMessage(LogLevel.Info, String.Format("Starting Job: {0}", context.JobDetail.Key.Name));
        }

        public void JobExecutionVetoed(IJobExecutionContext context)
        {
            this.Logger.LogMessage(LogLevel.Info, String.Format("Job {0} was vetoed!", context.JobDetail.Key.Name));
        }

        public void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            this.Logger.LogMessage(LogLevel.Info, String.Format("Job {0} completed in {1}", context.JobDetail.Key.Name, context.JobRunTime));
        }
    }
}
