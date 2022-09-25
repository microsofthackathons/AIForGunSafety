using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data;
using System.Reflection.Metadata;
using AIForGunSafetyFunctionApp.Models;
using Azure.Messaging.ServiceBus;
using System.ComponentModel;
using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.RecordIO;
using System.Configuration;

namespace AIForGunSafetyFunctionApp.Services
{
   
    public class TwitterService
    {
        private string _cosmosConnectionString;
        private string _twitterAuthKey;
        private string _sbConnectionString;
        public TwitterService()
        {
            var appSettings = ConfigurationManager.AppSettings;
            _cosmosConnectionString = appSettings["CosmosDBConnection"].ToString();
            _twitterAuthKey = appSettings["TwitterAuthKey"].ToString();
            _sbConnectionString = appSettings["SBUS_CONNECTIONSTRING"].ToString();
        }
        public async Task ConsumeSocialMediaPosts()
        {

            await AddTweet();
        }


        public async Task<TwitterUser> ConsumeGetTwitterUserRequest(string twitterUserId)
        {
            return await GetTwitterUser(twitterUserId);
        }

        public async Task AddMessageToServiceBusQueue(string queueName, string jsonString)
        {
            //string connectionString = $"Endpoint=sb://aigunsafetyhackathon.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=q1DvmcyakLVVraez5T+pAQj4Sgq6m1/Go9cvzdhXqM4=;EntityPath={queueName}";

            // the client that owns the connection and can be used to create senders and receivers
            ServiceBusClient client;

            // the sender used to publish messages to the queue
            ServiceBusSender sender;

            var clientOptions = new ServiceBusClientOptions() { TransportType = ServiceBusTransportType.AmqpWebSockets };
            client = new ServiceBusClient(_sbConnectionString, clientOptions);
            sender = client.CreateSender(queueName);

            await sender.SendMessageAsync(new ServiceBusMessage(jsonString));

        }

        private async Task AddTweet()
        {
            int records = 0;
            var sinceId = await GetLatestTweetId();
            Rootobject tweets = new Rootobject();
            if (!string.IsNullOrEmpty(sinceId))
                tweets = await RecentTweets("", sinceId);
            else
                tweets = await RecentTweets("", "");


            var updatedSinceId = tweets.data[0].id;
            records = tweets.metadata.result_count;

            var latestTweetPull = await AddLatestTweetId(updatedSinceId);


            await PostMessageInQueue(tweets);

            while (!string.IsNullOrEmpty(tweets.metadata.next_token))
            {
                tweets = await RecentTweets(tweets.metadata.next_token, sinceId);
                records = records + tweets.metadata.result_count;
                await PostMessageInQueue(tweets);
            }
            latestTweetPull.records = records;
            await UpdateLatestTweetId(latestTweetPull);

        }

        private async Task<Rootobject> RecentTweets(string nextToken = "", string sinceId = "")
        {
            Rootobject tweets = new Rootobject();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.twitter.com/2/tweets/search/recent");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var url = client.BaseAddress + "?query=violence&max_results=100&tweet.fields=author_id,possibly_sensitive,geo";
            if (!string.IsNullOrEmpty(nextToken)) url = url + "&next_token=" + nextToken;
            if (!string.IsNullOrEmpty(sinceId)) url = url + "&since_id=" + sinceId;

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            var authString = _twitterAuthKey;
            request.Headers.Add("Authorization", $"Bearer {authString}");
            request.Headers.Add("Accept", "application/json");

            HttpResponseMessage response = new HttpResponseMessage();
            response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Reading Response.  
                string content = response.Content.ReadAsStringAsync().Result;
                tweets = JsonConvert.DeserializeObject<Rootobject>(content);
            }

            return tweets;
        }

        private async Task<TwitterUser> GetTwitterUser(string twitterUserId)
        {
            TwitterUser user = new TwitterUser();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://api.twitter.com/2/users");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var url = client.BaseAddress + "?ids=" + twitterUserId + "&user.fields=created_at,description,entities,id,location,name,pinned_tweet_id,profile_image_url,protected,url,username,verified,withheld";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            var authString = _twitterAuthKey;
            request.Headers.Add("Authorization", $"Bearer {authString}");
            request.Headers.Add("Accept", "application/json");

            HttpResponseMessage response = new HttpResponseMessage();
            response = await client.SendAsync(request).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                // Reading Response.  
                string content = response.Content.ReadAsStringAsync().Result;
                user = JsonConvert.DeserializeObject<TwitterUser>(content);
            }

            return user;
        }

        private async Task PostMessageInQueue(Rootobject tweets)
        {
            // name of your Service Bus queue
            string queueName = "twitterfeedqueue";

            foreach (Tweet twt in tweets.data)
            {
                var jsonString = JsonConvert.SerializeObject(twt);
                await AddMessageToServiceBusQueue(queueName, jsonString);
            }
        }

        private async Task<string> GetLatestTweetId()
        {
            string recentId = "";
            // The Azure Cosmos DB endpoint for running this sample.
            //string EndpointUri = @"https://hackathon22.documents.azure.com:443/";

            // The primary key for the Azure Cosmos account.
            //string PrimaryKey = "bjRSRtPRWmqyinuTOtlna0rkOzAdpZtX5ZMWPPyJLudIfeqw8Fzn2RA2hIu7q9ed59NOqSQ4trUYUnaEhKaIzA==";

            string databaseId = "temp-db";
            string containerId = "latesttweet";

            // The Cosmos client instance
            CosmosClient cosmosClient = new CosmosClient(_cosmosConnectionString, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });

            // The database we will create
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

            // The name of the database and container we will create


            // The container we will create.
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/id");

            var sqlQueryText = "SELECT top 1 * FROM c order by c._ts desc";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);

            // Query multiple items from container
            using FeedIterator<LatestTweet> filteredFeed = container.GetItemQueryIterator<LatestTweet>(queryDefinition);

            // Iterate query result pages
            while (filteredFeed.HasMoreResults)
            {
                FeedResponse<LatestTweet> response = await filteredFeed.ReadNextAsync();

                // Iterate query results
                foreach (LatestTweet item in response)
                {
                    recentId = item.recent_tweet_id;
                }
            }

            return recentId;
        }

        private async Task UpdateLatestTweetId(LatestTweet latestTweet)
        {
            // The Azure Cosmos DB endpoint for running this sample.
            //string EndpointUri = @"https://hackathon22.documents.azure.com:443/";

            // The primary key for the Azure Cosmos account.
            //string PrimaryKey = "bjRSRtPRWmqyinuTOtlna0rkOzAdpZtX5ZMWPPyJLudIfeqw8Fzn2RA2hIu7q9ed59NOqSQ4trUYUnaEhKaIzA==";

            string databaseId = "temp-db";
            string containerId = "latesttweet";

            // The Cosmos client instance
            CosmosClient cosmosClient = new CosmosClient(_cosmosConnectionString, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });

            // The database we will create
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

            // The name of the database and container we will create


            // The container we will create.
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/id");

            ItemResponse<LatestTweet> latestTweetResponse = await container.ReadItemAsync<LatestTweet>(latestTweet.id, new PartitionKey(latestTweet.id));
            var itemBody = latestTweetResponse.Resource;

            itemBody.records = latestTweet.records;

            latestTweetResponse = await container.ReplaceItemAsync<LatestTweet>(itemBody, itemBody.id);
        }

        private async Task<LatestTweet> AddLatestTweetId(string recentId)
        {
            // The Azure Cosmos DB endpoint for running this sample.
            //string EndpointUri = @"https://hackathon22.documents.azure.com:443/";

            // The primary key for the Azure Cosmos account.
            //string PrimaryKey = "bjRSRtPRWmqyinuTOtlna0rkOzAdpZtX5ZMWPPyJLudIfeqw8Fzn2RA2hIu7q9ed59NOqSQ4trUYUnaEhKaIzA==";

            string databaseId = "temp-db";
            string containerId = "latesttweet";

            // The Cosmos client instance
            CosmosClient cosmosClient = new CosmosClient(_cosmosConnectionString, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });

            // The database we will create
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);

            // The name of the database and container we will create


            // The container we will create.
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/id");
            LatestTweet latestTweetItem = new LatestTweet() { id = Guid.NewGuid().ToString(), recent_tweet_id = recentId, records = 0 };
            await container.CreateItemAsync<LatestTweet>(latestTweetItem);

            return latestTweetItem;
        }
    }
}