using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using SquidReports.DataCollector.API;
using SquidReports.DataCollector.Interface;
using SquidReports.DataCollector.Plugin.Test;
using SquidReports.DataCollector.Plugin.BES;
using Dapper;

namespace SquidReports.DataCollector
{
    class Program
    {
        static void Main(string[] args)
        {
            BESCollector collector = new BESCollector();
            CollectorValidation(collector);
            CollectorStartup(collector);
            CollectorExecute(collector);          
            CollectorPostRun(collector);

            Console.Read();
        }

        private static void CollectorValidation(ICollector collector)
        {
            // Explore the full list of ICollectibles in the ICollector assembly
            IEnumerable<Type> types = collector.GetType().Assembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICollectible)) && t.GetConstructor(Type.EmptyTypes) != null);

            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);

            foreach (Type type in types)
            {
                Console.WriteLine(type.Name);
                // Check if the data model has been registered
                if (!connection.Query("SELECT * FROM [SQR].[DATA_MODEL] WHERE ModelName = @ModelName AND AssemblyName = @AssemblyName", new { ModelName = type.Name, AssemblyName = type.Assembly.GetName().Name }).Any())
                {
                    // It's not there yet, register in in the [SQR].[DATA_MODEL] table
                    connection.Execute("INSERT INTO [SQR].[DATA_MODEL] (ModelName, AssemblyName) VALUES (@ModelName, @AssemblyName)", new { ModelName = type.Name, AssemblyName = type.Assembly.GetName().Name });
                }
            }
        }

        private static void CollectorStartup(ICollector collector)
        {
            // What type of collector is this?
            CollectorType type;
            CollectorTypeAttribute[] typeAttributes = (CollectorTypeAttribute[])collector.GetType().GetCustomAttributes(typeof(CollectorTypeAttribute), true);
            if (typeAttributes.Length > 0)
            {
                type = typeAttributes[0].Type;
            }
            else
            {
                throw new ApplicationException(String.Format("No CollectorType defined for ICollector: ", collector.GetType().Name));
            }

            // Let's create a logger object
            ILogger nLogger = new NLogger();

            // ... And a DbRelay, while specifying the CollectorType
            IDbRelay dbRelay = new DbRelay(ConfigurationManager.ConnectionStrings["DB"].ConnectionString, type);

            collector.Init(nLogger, dbRelay);
        }

        private static void CollectorExecute(ICollector collector)
        {
            collector.Execute();
        }

        private static void CollectorPostRun(ICollector collector)
        {
            // What type of collector is this?
            CollectorType type;
            CollectorTypeAttribute[] typeAttributes = (CollectorTypeAttribute[])collector.GetType().GetCustomAttributes(typeof(CollectorTypeAttribute), true);
            if (typeAttributes.Length > 0)
            {
                type = typeAttributes[0].Type;
            }
            else
            {
                throw new ApplicationException(String.Format("No CollectorType defined for ICollector: ", collector.GetType().Name));
            }

            if (type == CollectorType.Absolute)
            {   
                // Get the full list of models that are part of this collector
                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
                IEnumerable<Int32> IDs = connection.Query<Int32>("SELECT ID FROM [SQR].[DATA_MODEL] WHERE AssemblyName = @AssemblyName", new { AssemblyName = collector.GetType().Assembly.GetName().Name });         

                // Handle the Post-Run for all the different models
                foreach (int ID in IDs)
                {
                    // Step 1: For each model -- Get the full list in the cache, and the full list in the table
                    IEnumerable<String> cachedKeys = connection.Query<String>("SELECT KeyHash FROM [SQR].[DATA_CACHE] WHERE ModelID = @ModelID", new { ModelID = ID });
                    IEnumerable<String> tableKeys = connection.Query<String>("SELECT KeyHash FROM [SQR].[DATA_HASH] WHERE ModelID = @ModelID", new { ModelID = ID });

                    // Step 2: For each Key of said model -- Check if the data found in the persistent storage is still valid
                    foreach (string tableKey in tableKeys)
                    {
                        if (!cachedKeys.Any<String>(ck => ck == tableKey))
                        {
                            // Obtain the reference used to link the KeyHash with the ID of the entity in the persistent table
                            int tableID = connection.Query<Int32>("SELECT TableID FROM [SQR].[DATA_HASH] WHERE ModelID = @ModelID AND KeyHash = @KeyHash", new { ModelID = ID, KeyHash = tableKey }).Single();

                            // Cached keys contain ALL the data that should be retained. The key in the table was NOT found in the cache.
                            // Therefore, it has become invalid. Let's delete it from the HashTable
                            connection.Execute("DELETE FROM [SQR].[DATA_HASH] WHERE ModelID = @ModelID AND KeyHash = @KeyHash", new { ModelID = ID, KeyHash = tableKey });

                            // Also, delete the date in the actual persistent table
                            connection.Execute(Helpers.Sql.DeleteBuilder(collector.GetType(), tableID));
                        }
                    }
                    
                    // Step 3: Clear the cache for this model
                    connection.Execute("DELETE FROM [SQR].[DATA_CACHE] WHERE ModelID = @ModelID", new { ModelID = ID });
                }
            }
        }
    }
}
