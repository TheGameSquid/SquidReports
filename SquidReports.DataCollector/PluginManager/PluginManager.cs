using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Reflection;
using SquidReports.DataCollector.Config;
using SquidReports.DataCollector.Interface;
using Dapper;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;

namespace SquidReports.DataCollector
{
    class PluginManager
    {
        public PluginManager(ILogManager logManager)
        {
            this.LogManager = logManager;
            this.Logger = logManager.GetCurrentClassLogger();
            this.Collectors = new List<CollectorJob>();
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;
        }

        public List<CollectorJob> Collectors   { get; set; }
        public ILogger Logger               { get; set; }
        public ILogManager LogManager { get; set; }

        public void Start()
        {
            // Grab the Scheduler instance from the Factory and start it
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            // Read the Plugins defined in the App.Config
            PluginConfigurationSection pluginConfigSection = ConfigurationManager.GetSection("PluginSection") as PluginConfigurationSection;

            foreach (PluginConfiguration pluginConfig in pluginConfigSection.Plugins)
            {
                ICollector collector = null;

                try
                {
                    // Load the Assembly in a Reflection-only context
                    // Depedencies are resolve by AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += ReflectionOnlyAssemblyResolve;
                    //Assembly pluginAssembly = Assembly.ReflectionOnlyLoadFrom(pluginConfig.AssemblyLocation);
                    Assembly pluginAssembly = Assembly.LoadFrom(pluginConfig.AssemblyLocation);

                    // Loop through all the types in the assembly that have the name defined in the config
                    foreach (Type type in pluginAssembly.GetTypes().Where(t => t.Name == pluginConfig.CollectorName))
                    {
                        // If the GUIDs of the interface types are the same, it's a collector!
                        if (type.GetInterfaces().Any(i => i.GUID == typeof(ICollector).GUID))
                        {
                            // This is a confirmed ICollector!
                            this.Logger.LogMessage(LogLevel.Info, String.Format("Succesfully detected ICollector with name '{0}' in the assembly at location '{1}'", pluginConfig.CollectorName, pluginConfig.AssemblyLocation));
                           
                            // Spawn an instance of the ICollector
                            collector = (ICollector)Activator.CreateInstance(type);

                            // What type of collector is this?
                            CollectorType collectorType;
                            CollectorTypeAttribute[] typeAttributes = (CollectorTypeAttribute[])collector.GetType().GetCustomAttributes(typeof(CollectorTypeAttribute), true);
                            if (typeAttributes.Length > 0)
                            {
                                collectorType = typeAttributes[0].Type;
                            }
                            else
                            {
                                throw new ApplicationException(String.Format("No CollectorType defined for ICollector: {0}", collector.GetType().Name));
                            }

                            CollectorValidation(collector);

                            // Call Init on the Collector, prepare it for scheduling
                            collector.Init(this.LogManager, new DbRelay(ConfigurationManager.ConnectionStrings["DB"].ConnectionString, collectorType));

                            // Build a job with a Cron-scheduled job
                            // We're passing along a JobDataMap in order the send the ICollector along with the job
                            // This is because an IJob can not call a constructor
                            IJobDetail job = JobBuilder.Create<CollectorJob>()
                                .UsingJobData(new JobDataMap(new Dictionary<string, ICollector> { { "ICollector", collector } }))
                                .WithIdentity(type.FullName)
                                .Build();

                            ITrigger trigger = TriggerBuilder.Create()
                                .WithCronSchedule("0 0/1 * 1/1 * ? *")
                                .StartNow()
                                .Build();

                            // Tell Quartz to schedule the job using our trigger
                            scheduler.ScheduleJob(job, trigger);                         
                        }
                    }

                    scheduler.ListenerManager.AddJobListener(new CollectorJobListener(LogManager), GroupMatcher<JobKey>.AnyGroup());

                    if (collector == null)
                    {
                        this.Logger.LogMessage(LogLevel.Warn, String.Format("Could not find an ICollector with name '{0}' in the assembly at location '{1}'", pluginConfig.CollectorName, pluginConfig.AssemblyLocation));
                    }
                }
                catch (Exception e)
                {
                    this.Logger.LogException(LogLevel.Warn, String.Format("Failed to load assembly containing {0}: {1}", pluginConfig.CollectorName, e.Message), e);
                }
            }
        }

        private void CollectorValidation(ICollector collector)
        {
            // Explore the full list of ICollectibles in the ICollector assembly
            IEnumerable<Type> types = collector.GetType().Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICollectible)) && t.GetConstructor(Type.EmptyTypes) != null);

            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);

            foreach (Type type in types)
            {
                // Check if the data model has been registered
                if (!connection.Query("SELECT * FROM [SQR].[DATA_MODEL] WHERE ModelName = @ModelName AND AssemblyName = @AssemblyName", new { ModelName = type.Name, AssemblyName = type.Assembly.GetName().Name }).Any())
                {
                    // It's not there yet, register in in the [SQR].[DATA_MODEL] table
                    connection.Execute("INSERT INTO [SQR].[DATA_MODEL] (ModelName, ModelNameFull, AssemblyName, AssemblyNameFull, NameSpace) VALUES (@ModelName, @ModelNameFull, @AssemblyName, @AssemblyNameFull, @NameSpace)",
                                                                        new
                                                                        {
                                                                            ModelName = type.Name,
                                                                            ModelNameFull = String.Format("{0}.{1}", type.Namespace, type.Name),
                                                                            AssemblyName = type.Assembly.GetName().Name,
                                                                            AssemblyNameFull = type.Assembly.FullName,
                                                                            type.Namespace
                                                                        });
                }
            }
        }

        public Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            // This event is used to resolve dependency problems. We need to return the requested assembly in this method.
            // We should probably look in two locations:
            //   1. The SquidReports.DataCollector folder
            //   2. The Corresponding folder in SquidReports.DataCollector\Plugins    
   
            // Let's first turn the 'full' assembly name into something more compact
            AssemblyName assemblyName = new AssemblyName(args.Name);
            this.Logger.LogMessage(LogLevel.Debug, String.Format("Attempting to resolve Assembly {0} to load {0}", assemblyName.Name, args.RequestingAssembly.GetName().Name));

            // Let's also get the location where the requesting assembly is located
            DirectoryInfo pluginDirectory = Directory.GetParent(args.RequestingAssembly.Location);
            string assemblyFileName = String.Format("{0}.dll", assemblyName.Name);

            if (File.Exists(assemblyFileName))
            {
                // It's in the main bin folder, let's try to load from here
                return Assembly.ReflectionOnlyLoadFrom(assemblyFileName);
            }
            else if (File.Exists(Path.Combine(pluginDirectory.FullName, assemblyFileName)))
            {
                // It's in the plugin folder, let's load from there
                return Assembly.ReflectionOnlyLoadFrom(Path.Combine(pluginDirectory.FullName, assemblyFileName));
            }

            return null;
        }
    }
}
