using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

namespace SquidReports.DataCollector.Interface
{
    public interface IDbRelay
    {
        SqlConnection Connection { get; set; }
        string ConnectionString { get; set; }

        IEnumerable<T> Get<T>(object parameters) where T : new();
        void Put<T>(ICollectible data);
    }
}
