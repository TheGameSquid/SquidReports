using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "ACTION_DETAIL")]
    public class ActionDetail : ICollectible
    {
        public ActionDetail()
        {
            // Empty constructor for RestSharp
        }

        public ActionDetail(int aActionID, string aStatus, string aDateIssued)
        {
            this.ActionID = aActionID;
            this.Status = aStatus;
            this.DateIssued = aDateIssued;
        }

        public int ID               { get; set; }       // Identity ID assigned by DB
        [Key]
        public int ActionID         { get; set; }
        public string Status        { get; set; }
        public string DateIssued    { get; set; }
    }
}
