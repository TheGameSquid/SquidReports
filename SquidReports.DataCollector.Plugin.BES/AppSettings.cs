using System;
using System.Configuration;
using System.ComponentModel;
using System.Reflection;

namespace SquidReports.DataCollector.Plugin.BES
{
    public static class AppSettings
    {
        public static T Get<T>(string key)
        {
            Configuration appConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var appSetting = appConfig.AppSettings.Settings[key].Value;
            if (String.IsNullOrWhiteSpace(appSetting))
            {
                throw new Exception(String.Format("Key {0} was not found", key));
            }

            TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }
    }
}