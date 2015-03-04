using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace SquidReports.DataCollector.Interface
{
    public class DataObject
    {
        public DataObject(dynamic data)
        {
            this.Data = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });;
        }

        public string Data { get; set; }
    }
}
