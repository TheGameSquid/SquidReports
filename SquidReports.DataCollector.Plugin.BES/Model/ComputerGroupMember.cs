using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "GROUP_MEMBER")]
    public class ComputerGroupMember : ICollectible
    {
        public ComputerGroupMember()
        {
            // Empty constructor for RestSharp
        }

        public ComputerGroupMember(int GroupID, int ComputerID)
        {
            this.GroupID = GroupID;
            this.ComputerID = ComputerID;
        }

        public int ID           { get; set; }   // Identity ID assigned by DB
        [Key]
        public int GroupID      { get; set; }
        [Key]
        public int ComputerID   { get; set; }
    }
}
