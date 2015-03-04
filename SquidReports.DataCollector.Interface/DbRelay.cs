using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using SquidReports.DataCollector;
using Dapper;
using LambdaSqlBuilder;
using LambdaSqlBuilder.Builder;

namespace SquidReports.DataCollector.Interface
{
    public class DbRelay
    {
        public DbRelay (string connectionString)
        {
            this.ConnectionString = connectionString;
            this.Connection = new SqlConnection(connectionString);
        }

        private SqlConnection Connection { get; set; }
        private string ConnectionString { get; set; }

        public IEnumerable<T> Get<T>(SqlLam<T> query)
        {
            Console.WriteLine(query.QueryString);
            foreach (string str in query.SqlBuilder.TableNames)
            {
                Console.WriteLine(str);
            }

            return this.Connection.Query<T>(query.QueryString, query.QueryParameters);
        }

        public void Put<T>(ICollectible data)
        {
            // TODO
        }
    }
}
