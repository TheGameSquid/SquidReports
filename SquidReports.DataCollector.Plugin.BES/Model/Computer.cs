using System;
using SquidReports.DataCollector.Interface;
using RestSharp.Deserializers;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "COMPUTER")]
    public class Computer : ICollectible
    {
        public Computer()
        {
            // Empty constructor used by RestSharp
        }

        [DeserializeAs(Name = "IgnoreID")]
        public int ID                   { get; set; }       // Identity ID assigned by DB
        [DeserializeAs(Name = "ID"), Key]
        public int ComputerID           { get; set; }       // Identity ID assigned by API
        public string ComputerName      { get; set; }
        public DateTime LastReportTime  { get; set; }
    }
}
