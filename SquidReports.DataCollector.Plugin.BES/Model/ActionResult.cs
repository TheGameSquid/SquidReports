using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "ACTION_RESULT")]
    public class ActionResult
    {
        public ActionResult()
        {
            // Empty constructor for RestSharp
        }

        public ActionResult(int aActionID, int aComputerID, string aStatus, int aState, int aApplyCount, int aRetryCount, DateTime? aStartTime, DateTime? aEndTime)
        {
            this.ActionID = aActionID;
            this.ComputerID = aComputerID;
            this.Status = aStatus;
            this.State = aState;
            this.ApplyCount = aApplyCount;
            this.RetryCount = aRetryCount;
            this.StartTime = aStartTime;
            this.EndTime = aEndTime;
        }

        public int ID               { get; set; }   // Identity ID assigned by DB
        [Key]
        public int ActionID         { get; set; }
        [Key]
        public int ComputerID       { get; set; }
        public string Status        { get; set; }
        public int State            { get; set; }
        public int ApplyCount       { get; set; }
        public int RetryCount       { get; set; }
        public DateTime? StartTime  { get; set; }
        public DateTime? EndTime    { get; set; }
    }
}
