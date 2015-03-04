using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace SquidReports.DataCollector.Interface
{
    public class CollectorEventArgs : EventArgs
    {
        public CollectorEventArgs(ICollectible data)
        {
            this.Data = data;
        }

        public ICollectible Data { get; set; }
    }
}
