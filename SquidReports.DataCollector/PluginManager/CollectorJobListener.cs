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

            // We're now entering the post-run phase, so let's perform some checks
            // Let's unwrap the ICollector object from the JobDataMap
            ICollector collector = (ICollector)context.MergedJobDataMap["ICollector"];

            // What type of collector is this?
            CollectorType type;
            CollectorTypeAttribute[] typeAttributes = (CollectorTypeAttribute[])collector.GetType().GetCustomAttributes(typeof(CollectorTypeAttribute), true);
            if (typeAttributes.Length > 0)
            {
                type = typeAttributes[0].Type;
            }
            else
            {
                throw new ApplicationException(String.Format("No CollectorType defined for ICollector: {0}", collector.GetType().Name));
            }

            // An absolute Collector uses a Data Cache to compare data collected during the run with the data already stored in-table
            if (type == CollectorType.Absolute)
            {
                DbUtils.CleanCache(collector);
            }
        }
    }
}
