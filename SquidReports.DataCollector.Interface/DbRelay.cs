using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;

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

        public IEnumerable<T> Get<T>(object parameters) where T : new()
        {
            return Connection.Query<T>(SqlBuilder(typeof(T), parameters), parameters);
        }

        private string SqlBuilder(Type type, object parameters)
        {
            string query = String.Empty;

            // Step one, get the Table attribute to detect the Schema and Table name
            TableAttribute[] tableAttributes = (TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttributes.Length > 0)
            {
                query = String.Format("SELECT * FROM [{0}].[{1}]", tableAttributes[0].Schema, tableAttributes[0].Table);              
            }
            else
            {
                throw new ApplicationException(String.Format("No Schema and Table defined for Type {0}", type.Name));
            }

            // Now we need to compose the WHERE clause
            // Were any parameters provided?
            if (parameters != null)
            {
                // Let's look at the properties of the anonymous object
                PropertyInfo[] properties = parameters.GetType().GetProperties();
                for (int index = 0; index < properties.Length; index++)
                {
                    Console.WriteLine(properties[index].Name);

                    // Add the first WHERE
                    if (index == 0)
                    {
                        query += String.Format(" WHERE {0} = @{0}", properties[index].Name);
                    }
                    // Add an AND
                    else
                    {
                        query += String.Format(" AND {0} = @{0}", properties[index].Name);
                    }
                }
            }
            
            return query;
        }

        public void Put<T>(ICollectible data)
        {
            // TODO
        }
    }
}
