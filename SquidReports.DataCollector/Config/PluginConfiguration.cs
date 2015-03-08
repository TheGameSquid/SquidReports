using System;
using System.Configuration;

namespace SquidReports.DataCollector.Config
{
    public class PluginConfiguration : ConfigurationElement
    {
        public PluginConfiguration()
        { 
        
        }

        public PluginConfiguration(string collectorName, string assemblyLocation)
        {
            CollectorName = collectorName;
            AssemblyLocation = assemblyLocation;
        }

        [ConfigurationProperty("CollectorName", DefaultValue = "", IsRequired = true, IsKey = true)]
        public string CollectorName
        {
            get { return (string)this["CollectorName"]; }
            set { this["CollectorName"] = value; }
        }

        [ConfigurationProperty("AssemblyLocation", DefaultValue = "", IsRequired = true, IsKey = true)]
        public string AssemblyLocation
        {
            get { return (string)this["AssemblyLocation"]; }
            set { this["AssemblyLocation"] = value; }
        }
    }
}
