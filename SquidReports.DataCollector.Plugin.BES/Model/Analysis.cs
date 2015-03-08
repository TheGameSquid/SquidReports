using System;
using SquidReports.DataCollector.Interface;
using RestSharp.Deserializers;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "ANALYSIS")]
    public class Analysis : ICollectible
    {
        public Analysis()
        {
            // Empty constructor for RestSharp
        }

        [DeserializeAs(Name = "IgnoreID")]
        public int ID           { get; set; }       // Identity ID assigned by DB
        [DeserializeAs(Name = "ID")]
        [Key]
        public int AnalysisID   { get; set; }       // Identity ID assigned by API
        public int SiteID       { get; set; }
        public string Name      { get; set; }
    }
}
