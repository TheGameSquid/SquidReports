using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace SquidReports.DataCollector.Interface
{
    public class CollectorEventArgs : EventArgs
    {
        public CollectorEventArgs(dynamic data)
        {
            this.DataType = data.GetType();
            this.DataJson = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });
        }

        public Type DataType    { get; set; }
        public string DataJson  { get; set; }
    }
}
