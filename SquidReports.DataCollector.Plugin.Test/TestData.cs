using System;
using SquidReports.DataCollector.Interface;
using DapperExtensions.Mapper;

namespace SquidReports.DataCollector.Plugin.Test
{
    public class TestData
    {
        public TestData(int data)
        {
            this.Data = data;
        }

        public int ID { get; set; }
        public int Data { get; set; }
    }

    public class TestDataMapper : ClassMapper<TestData>
    {
        public TestDataMapper()
        {
            // Define target Table and Schema
            Schema("BESEXT");
            Table("ACTION_DETAIL");

            // Define target columns
            Map(f => f.ID).Column("ID").Key(KeyType.Identity);
            Map(f => f.Data).Column("Data");
        }
    }
}
