using System;
using System.Configuration;

namespace SquidReports.DataCollector.Config
{
    public class PluginConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("Plugins", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(PluginConfigurationCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public PluginConfigurationCollection Plugins
        {
            get
            {
                return (PluginConfigurationCollection)base["Plugins"];
            }
        }
    }
}
