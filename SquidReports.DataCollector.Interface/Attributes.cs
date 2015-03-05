using System;

namespace SquidReports.DataCollector.Interface
{
    public enum CollectorType
    {
        Absolute,
        Cumulative
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute
    {
        public string Schema    { get; set; }
        public string Table     { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CollectorTypeAttribute : Attribute
    {
        public CollectorType Type { get; set; }
    }
}
