using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using SquidReports.DataCollector.Interface;
using Dapper;
using Newtonsoft.Json;

namespace SquidReports.DataCollector
{
    public class DbRelay : IDbRelay
    {
        public DbRelay (string connectionString, CollectorType collectorType)
        {
            this.ConnectionString = connectionString;
            this.Connection = new SqlConnection(connectionString);
            this.CollectorType = collectorType;
        }

        public SqlConnection Connection { get; set; }
        public string ConnectionString { get; set; }
        public CollectorType CollectorType { get; set; }

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
            Dictionary<string, object> keyValues = GetKeyValues<T>(data);
            Dictionary<string, object> nonKeyValues = GetNonKeyValues<T>(data);

            string keyHash = Helpers.Crypto.GetMD5HashFromObject(keyValues);
            string nonKeyHash = Helpers.Crypto.GetMD5HashFromObject(nonKeyValues);

            // If the collector type is Absolute, we need to cache the data so we can delete old entries based on the ABSOLUTE list in the cache
            if (this.CollectorType == CollectorType.Absolute)
            {
                CacheData<T>(keyHash, nonKeyHash);
            }

            if (IsNew<T>(keyHash))
            {
                int newID = InsertData<T>(data);
                InsertHash<T>(keyHash, nonKeyHash, newID);
            }
            else if (IsToUpdate<T>(keyHash, nonKeyHash))
            { 
                UpdateData<T>(data);
                UpdateHash<T>(keyHash, nonKeyHash);
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

        public int InsertData<T>(ICollectible data)
        {
            Connection.Execute(Helpers.Sql.InsertBuilder(typeof(T)), data);
            dynamic id = Connection.Query(Helpers.Sql.SelectBuilder(typeof(T), data), data).Single();
            return id.ID;
        }

        public void InsertHash<T>(string keyHash, string nonKeyHash, int newID)
        {
            Connection.Execute("INSERT INTO [SQR].[DATA_HASH] (ModelID, TableID, KeyHash, NonKeyHash) VALUES (@ModelID, @TableID, @KeyHash, @NonKeyHash)", new { @ModelID = GetModelID(typeof(T)), TableID = newID, KeyHash = keyHash, NonKeyHash = nonKeyHash });
        }

        public void UpdateData<T>(ICollectible data)
        {
            Connection.Execute(Helpers.Sql.UpdateBuilder(typeof(T)), data);
        }

        public void UpdateHash<T>(string keyHash, string nonKeyHash)
        {
            Connection.Execute("UPDATE [SQR].[DATA_HASH] SET NonKeyHash = @NonKeyHash WHERE KeyHash = @KeyHash", new { KeyHash = keyHash, NonKeyHash = nonKeyHash });
        }

        public void CacheData<T>(string keyHash, string nonKeyHash)
        {
            // Find the ModelID
            int modelID = GetModelID(typeof(T));
            Connection.Execute("INSERT INTO [SQR].[DATA_CACHE] (ModelID, KeyHash, NonKeyHash) VALUES (@ModelID, @KeyHash, @NonKeyHash)", new { @ModelID = modelID, KeyHash = keyHash, NonKeyHash = nonKeyHash });
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
