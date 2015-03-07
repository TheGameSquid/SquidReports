using System;
using SquidReports.DataCollector.Interface;
using RestSharp.Deserializers;

namespace SquidReports.DataCollector.Plugin.BES.Model
{
    [Table(Schema = "BESEXT", Table = "ANALYSIS_PROPERTY")]
    public class AnalysisProperty
    {
        public AnalysisProperty()
        {
            // Empty constructor for RestSharp
        }

        public AnalysisProperty(int analysisID, int sequenceNo, string name)
        {
            this.AnalysisID = analysisID;
            this.SequenceNo = sequenceNo;
            this.Name = name;
        }

        public int ID               { get; set; }   // Identity ID assigned by DB
        public int AnalysisID       { get; set; }   // Parent Analysis API ID
        [DeserializeAs(Name = "ID")]
        public int SequenceNo       { get; set; }   // This the N-th property of the analysis
        public string Name          { get; set; }
    }
}
