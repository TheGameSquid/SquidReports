using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "BASELINE")]
    public class Baseline : ICollectible
    {
        public Baseline()
        {
            // Empty constructor for RestSharp
        }

        public Baseline(int baselineID, int siteID, string name)
        {
            this.BaselineID = baselineID;
            this.SiteID = siteID;
            this.Name = name;
        }

        public int ID { get; set; }
        [Key]
        public int BaselineID { get; set; }
        public int SiteID { get; set; }
        public string Name { get; set; }
    }
}
