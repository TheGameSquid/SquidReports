using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "SITE")]
    public class Site : ICollectible
    {
        public Site()
        {
            // Constructor for RestSharp
        }

        public Site(string aName, string aType)
        {
            this.Name = aName;
            this.Type = aType;
        }

        public int ID       { get; set; }     // Identity ID assigned by DB
        [Key]
        public string Name  { get; set; }
        public string Type  { get; set; }
    }
}
