using System;
using SquidReports.DataCollector.Interface;
using Quartz;

namespace SquidReports.DataCollector
{
    [DisallowConcurrentExecution]
    class CollectorJob : IJob
    {
        public ICollector CollectorObject       { get; set; }
        public ITrigger Trigger                 { get; set; }
        public IJobDetail JobDetail             { get; set; }

        public CollectorJob()
        {

        }

        public CollectorJob(ICollector collector, ITrigger trigger, IJobDetail jobDetail)
        {
            this.CollectorObject = collector;
            this.Trigger = trigger;
            this.JobDetail = jobDetail;
        }

        public void Execute(IJobExecutionContext context)
        {
            // Extract the ICollector from the DataMap
            JobDataMap dataMap = context.MergedJobDataMap;
            ICollector collector = (ICollector)dataMap["ICollector"];

            // Finally! Execute the collector
            collector.Execute();
        }
    }
}
