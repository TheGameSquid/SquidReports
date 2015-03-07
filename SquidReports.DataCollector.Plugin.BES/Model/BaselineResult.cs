using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "BASELINE_RESULT")]
    public class BaselineResult : ICollectible
    {
        public BaselineResult()
        {
            // Empty constructor for RestSharp
        }

        public BaselineResult(int baselineID, int computerID)
        {
            this.BaselineID = baselineID;
            this.ComputerID = computerID;
        }

        public int ID { get; set; }
        [Key]
        public int BaselineID { get; set; }
        [Key]
        public int ComputerID { get; set; }
    }
}
