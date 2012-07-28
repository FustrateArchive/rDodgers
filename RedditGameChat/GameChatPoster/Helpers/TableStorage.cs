namespace GameChatPoster.Helpers
{
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    public class TableStorage
    {
        public static TableServiceContext GetTableServiceContext(string tableName)
        {
            // Retrieve storage account from connection-string
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString"));

            // Create the table client
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist
            tableClient.CreateTableIfNotExist(tableName);

            TableServiceContext serviceContext = tableClient.GetDataServiceContext();
            return serviceContext;
        }

        public static void AddEntity(TableServiceEntity entity, string tableName)
        {
            var tableServiceContext = GetTableServiceContext(tableName);

            // Add the new customer to the people table
            tableServiceContext.AddObject(tableName, entity);

            // Submit the operation to the table service
            tableServiceContext.SaveChangesWithRetries();
        }

        public static void DeleteEntity(TableServiceEntity entity, string tableName)
        {
            var tableServiceContext = GetTableServiceContext(tableName);

            tableServiceContext.DeleteObject(entity);

            tableServiceContext.SaveChangesWithRetries();
        }
    }
}