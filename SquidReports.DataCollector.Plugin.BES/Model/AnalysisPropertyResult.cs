using System;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "ANALYSIS_PROPERTY_RESULT")]
    public class AnalysisPropertyResult : ICollectible
    {
        public AnalysisPropertyResult()
        {
            // Empty constructor for Dapper and RestSharp
        }

        public AnalysisPropertyResult(int propertyID, int computerID, string value)
        {
            this.PropertyID = propertyID;
            this.ComputerID = computerID;
            this.Value = value;
        }

        public int ID           { get; set; }   // Identity ID assigned by DB
        [Key]
        public int PropertyID   { get; set; }   // Parent Property ID assigned by DB
        [Key]
        public int ComputerID   { get; set; }   // Parent Computer ID assigned by API
        public string Value     { get; set; }
    }
}
