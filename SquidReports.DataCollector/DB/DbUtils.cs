using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using SquidReports.DataCollector.Interface;
using Dapper;

namespace SquidReports.DataCollector
{
    class DbUtils
    {
        public static void CleanCache(ICollector collector)
        {
            // First things first: spawn a logger
            NLogManager logManager = new NLogManager();
            ILogger logger = logManager.GetCurrentClassLogger();

            logger.LogMessage(LogLevel.Info, String.Format("Performing a Cache-sweep for Collector of type {0}", collector.GetType().Name));

            // Get the full list of models that are part of this collector
            SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
            IEnumerable<Int32> IDs = connection.Query<Int32>("SELECT ID FROM [SQR].[DATA_MODEL] WHERE AssemblyName = @AssemblyName", new { AssemblyName = collector.GetType().Assembly.GetName().Name });

            // Handle the Post-Run for all the different models
            foreach (int ID in IDs)
            {
                logger.LogMessage(LogLevel.Debug, String.Format("Performing a Cache-sweep for Model with ID {0}", ID));

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

                        // Next up, we need to delete from the actual persistent table, so we need to find out what Model this is
                        // For that, we use the ModelID and the [SQR].[DATA_MODEL] table
                        dynamic modelInfo = connection.Query("SELECT * FROM [SQR].[DATA_MODEL] WHERE ID = @ModelID", new { ModelID = ID }).Single();
                        // Spawn an instance of the model type
                        object modelObject = Activator.CreateInstance((string)modelInfo.AssemblyName, (string)modelInfo.ModelNameFull).Unwrap();
                        // Delete based on the Model Type
                        connection.Execute(Helpers.Sql.DeleteBuilder(modelObject.GetType(), tableID));
                    }
                }

                // Step 3: Clear the cache for this model
                connection.Execute("DELETE FROM [SQR].[DATA_CACHE] WHERE ModelID = @ModelID", new { ModelID = ID });
            }
        }
    }
}
