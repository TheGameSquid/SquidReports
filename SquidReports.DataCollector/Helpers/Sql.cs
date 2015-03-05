using System;
using System.Linq;
using System.Reflection;
using SquidReports.DataCollector.Interface;

namespace SquidReports.DataCollector.Helpers
{
    public enum SqlType
    {
        Select,
        Update,
        Insert,
        Delete
    }

    public class Sql
    {
        public static string SqlBuilder(SqlType sqlType, Type dataType, object parameters)
        {
            switch (sqlType)
            {
                case SqlType.Select:
                    return SelectBuilder(dataType, parameters);
                case SqlType.Insert:
                    return InsertBuilder(dataType);
                case SqlType.Update:
                    return UpdateBuilder(dataType);
                case SqlType.Delete:
                    return DeleteBuilder(dataType);
                default:
                    return "Hmmm...";
            }
        }

        public static string SelectBuilder(Type type, object parameters)
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

        public static string InsertBuilder(Type type)
        {
            string query = String.Empty;

            // Step one, get the Table attribute to detect the Schema and Table name
            TableAttribute[] tableAttributes = (TableAttribute[])type.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttributes.Length > 0)
            {
                query = String.Format("INSERT INTO [{0}].[{1}]", tableAttributes[0].Schema, tableAttributes[0].Table);
            }
            else
            {
                throw new ApplicationException(String.Format("No Schema and Table defined for Type {0}", type.Name));
            }

            // Now we need to compose the VALUES clause
            // Let's look at the properties of the Type
            PropertyInfo[] properties = type.GetProperties();

            // Let's filter out the ID column, it is part of ICollectible, and is an IDENTITY column
            properties = properties.Where(p => p.Name != "ID").ToArray();

            string propertyString = String.Empty;
            string valuesString = String.Empty;

            for (int index = 0; index < properties.Length; index++)
            {
                // Add the first WHERE
                if (index == 0)
                {
                    propertyString += String.Format("{0}", properties[index].Name);
                    valuesString += String.Format("@{0}", properties[index].Name);
                }
                // Add an AND
                else
                {
                    propertyString += String.Format(", {0}", properties[index].Name);
                    valuesString += String.Format(", @{0}", properties[index].Name);
                }
            }

            return String.Format("{0} ({1}) VALUES ({2})", query, propertyString, valuesString);
        }

        public static string UpdateBuilder(Type type)
        {
            throw new NotImplementedException();
        }

        public static string DeleteBuilder(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
