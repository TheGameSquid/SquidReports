using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    public class Action : ICollectible
    {
        public Action()
        {
            // Empty constructor for RestSharp
        }

        public Action(int actionID, int siteID, string name)
        {
            this.ActionID = actionID;
            this.SiteID = siteID;
            this.Name = name;
        }

        public int ID           { get; set; }             // Identity ID assigned by DB
        public int ActionID     { get; set; }       // Identity ID assigned by API
        public int SiteID       { get; set; }
        public string Name      { get; set; }
    }
}
