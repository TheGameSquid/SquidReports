using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using SquidReports.DataCollector.Interface;
using Dapper;
using Newtonsoft.Json;

namespace SquidReports.DataCollector.API
{
    public class DbRelay : IDbRelay
    {
        public DbRelay (string connectionString)
        {
            this.ConnectionString = connectionString;
            this.Connection = new SqlConnection(connectionString);
        }

        public SqlConnection Connection { get; set; }
        public string ConnectionString { get; set; }

        public IEnumerable<T> Get<T>() where T : new()
        {
            return Connection.Query<T>(Helpers.Sql.SqlBuilder(Helpers.SqlType.Select, typeof(T), null));
        }

        public IEnumerable<T> Get<T>(object parameters) where T : new()
        {
            return Connection.Query<T>(Helpers.Sql.SqlBuilder(Helpers.SqlType.Select, typeof(T), parameters), parameters);
        }

        public void Put<T>(ICollectible data)
        {
            Console.WriteLine(Helpers.Sql.InsertBuilder(typeof(T)));
            // TODO: Move this to application initialization fase
            RegisterModel(typeof(T));

            Dictionary<string, object> keyValues = GetKeyValues<T>(data);
            Dictionary<string, object> nonKeyValues = GetNonKeyValues<T>(data);

            string keyHash = Helpers.Crypto.GetMD5HashFromObject(keyValues);
            string nonKeyHash = Helpers.Crypto.GetMD5HashFromObject(nonKeyValues);

            if (IsNew<T>(keyHash))
            {
                InsertHash<T>(keyHash, nonKeyHash);
                InsertData<T>(data);
            }
            else if (IsToUpdate<T>(keyHash, nonKeyHash))
            {

            }
        }

        public Dictionary<string, object> GetKeyValues<T>(ICollectible data)
        {
            Dictionary<string, object> keyValues = new Dictionary<string, object>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                KeyAttribute[] keyAttributes = (KeyAttribute[])property.GetCustomAttributes(typeof(KeyAttribute), true);
                // If the property is decorated with the Key attribute, add it to the Dict
                if (keyAttributes.Length > 0)
                {
                    keyValues.Add(property.Name, property.GetValue(data, null));
                }
            }

            return keyValues;
        }

        public Dictionary<string, object> GetNonKeyValues<T>(ICollectible data)
        {
            Dictionary<string, object> nonKeyValues = new Dictionary<string, object>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (PropertyInfo property in properties)
            {
                KeyAttribute[] keyAttributes = (KeyAttribute[])property.GetCustomAttributes(typeof(KeyAttribute), true);
                // If the property is NOT decorated with the Key attribute, add it to the Dict
                if (keyAttributes.Length == 0)
                {
                    nonKeyValues.Add(property.Name, property.GetValue(data, null));
                }
            }

            return nonKeyValues;
        }

        public int GetModelID(Type type)
        {
            dynamic registeredType = Connection.Query("SELECT * FROM [SQR].[DATA_MODEL] WHERE ModelName = @ModelName AND AssemblyName = @AssemblyName", new { ModelName = type.Name, AssemblyName = type.Assembly.GetName().Name }).Single();
            return registeredType.ID;
        }

        public bool IsNew<T>(string hash)
        {
            return !Connection.Query<T>("SELECT * FROM [SQR].[DATA_HASH] WHERE ModelID = @ModelID AND KeyHash = @KeyHash", new { ModelID = GetModelID(typeof(T)), KeyHash = hash }).Any();
        }

        public bool IsToUpdate<T>(string keyHash, string nonKeyHash)
        {
            dynamic hashedItem = Connection.Query("SELECT NonKeyHash FROM [SQR].[DATA_HASH] WHERE ModelID = @ModelID AND KeyHash = @KeyHash", new { ModelID = GetModelID(typeof(T)), KeyHash = keyHash }).Single();
            if (hashedItem.NonKeyHash == nonKeyHash)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void InsertData<T>(ICollectible data)
        {
            Connection.Execute(Helpers.Sql.InsertBuilder(typeof(T)), data);
        }

        public void InsertHash<T>(string keyHash, string nonKeyHash)
        {
            Connection.Execute("INSERT INTO [SQR].[DATA_HASH] (ModelID, KeyHash, NonKeyHash) VALUES (@ModelID, @KeyHash, @NonKeyHash)", new { @ModelID = GetModelID(typeof(T)), KeyHash = keyHash, NonKeyHash = nonKeyHash });
        }

        public void RegisterModel(Type type)
        {
            // Check if the data model has been registered
            if (!Connection.Query("SELECT * FROM [SQR].[DATA_MODEL] WHERE ModelName = @ModelName AND AssemblyName = @AssemblyName", new { ModelName = type.Name, AssemblyName = type.Assembly.GetName().Name }).Any())
            {
                // It's not there yet, register in in the [SQR].[DATA_MODEL] table
                Connection.Execute("INSERT INTO [SQR].[DATA_MODEL] (ModelName, AssemblyName) VALUES (@ModelName, @AssemblyName)", new { ModelName = type.Name, AssemblyName = type.Assembly.GetName().Name });
            }      
        }
    }
}
