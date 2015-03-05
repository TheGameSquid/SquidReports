using System;

namespace SquidReports.DataCollector.Interface
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Schema    { get; set; }
        public string Table     { get; set; }
    }
}
