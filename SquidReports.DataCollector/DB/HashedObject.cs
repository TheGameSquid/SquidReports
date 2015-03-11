using System;

namespace SquidReports.DataCollector
{
    class HashedObject
    {
        public HashedObject(int modelID, string keyHash, string nonKeyHash)
        {
            this.ModelID = modelID;
            this.KeyHash = keyHash;
            this.NonKeyHash = nonKeyHash;
        }

        public int ModelID          { get; set; }
        public string KeyHash       { get; set; }
        public string NonKeyHash    { get; set; }
    }
}
