using System;
using System.Configuration;

namespace SquidReports.DataCollector.Config
{
    public class PluginConfigurationCollection : ConfigurationElementCollection
    {
        public PluginConfigurationCollection()
        {
            // Empty!!! :)
        }

        public PluginConfiguration this[int index]
        {
            get { return (PluginConfiguration)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public void Add(PluginConfiguration pluginConfiguration)
        {
            BaseAdd(pluginConfiguration);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PluginConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PluginConfiguration)element).CollectorName;
        }

        public void Remove(PluginConfiguration pluginConfiguration)
        {
            BaseRemove(pluginConfiguration.CollectorName);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }
    }
}
