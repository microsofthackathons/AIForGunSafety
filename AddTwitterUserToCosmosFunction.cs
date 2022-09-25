using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;

namespace AIForGunSafetyFunctionApp
{
    /// <summary>
    /// Function app to add social media users to cosmos db.
    /// </summary>
    public static class AddTwitterUserToCosmosFunction
    {
        [FunctionName("AddTwitterUserToCosmosFunction")]

        public static void Run([ServiceBusTrigger("twitteruserqueue", Connection = "SBUS_CONNECTIONSTRING")]string myQueueItem, [CosmosDB(
                databaseName: "temp-db",
                collectionName: "users",
                ConnectionStringSetting = "CosmosDBConnection")]out dynamic document, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            log.LogInformation($"Adding user to cosmos db.");
            document = new { Description = myQueueItem, id = Guid.NewGuid() };
           
            log.LogInformation($"Added user successfully to cosmos db");
        }
    }
}
