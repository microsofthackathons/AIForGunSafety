using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue.Protocol;

namespace AIForGunSafetyFunctionApp
{
    /// <summary>
    /// Function app to add social media feed to cosmos db.
    /// </summary>
    public static class AddTwitterToCosmosFunction
    {
        [FunctionName("AddTwitterToCosmosFunction")]

        public static void Run([ServiceBusTrigger("twitterfeedqueue", Connection = "SBUS_CONNECTIONSTRING")]string myQueueItem, [CosmosDB(
                databaseName: "temp-db",
                collectionName: "tweets",
                ConnectionStringSetting = "CosmosDBConnection")]out dynamic document, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
            log.LogInformation($"Adding message to cosmos db.");
            document = new { Description = myQueueItem, id = Guid.NewGuid() };
           
            log.LogInformation($"Added message successfully to cosmos db");
        }
    }
}
