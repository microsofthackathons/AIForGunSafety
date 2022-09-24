using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AIForGunSafetyFunctionApp.Handlers;
using AIForGunSafetyFunctionApp.Models;
using AIForGunSafetyFunctionApp.Services;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AIForGunSafetyFunctionApp
{
    public class CallSentimentAPIFromTweetCosmos
    {
        [FunctionName("CallSentimentAPIFromTweetCosmos")]
        public async Task Run([CosmosDBTrigger(
            databaseName: "temp-db",
            collectionName: "tweets",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "leases", CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                SocialMediaPostHandler socialMediaPostHandler = new SocialMediaPostHandler();
                TwitterService twitterService = new TwitterService();

                foreach (Document document in input)
                {
                    Tweet currentTweet = JsonConvert.DeserializeObject<Tweet>(document.GetPropertyValue<string>("Description"));
                    //Call the sentiment api by passing tweet.text and get the sentiment and score.
                    var sentimentResponse = await socialMediaPostHandler.GetSentimentScoreByText(currentTweet.text);


                    var twitterUser = await socialMediaPostHandler.GetTwitterUser(currentTweet.author_id);

                    if (twitterUser.data != null)
                    {

                        var userData = new UserData
                        {
                            TwitterUser = twitterUser.data[0],
                            SentimentScore = sentimentResponse.SentimentScore,
                            TweetText = currentTweet.text,
                            Location = currentTweet.geo
                        };

                        await twitterService.AddMessageToServiceBusQueue("twitteruserqueue", JsonConvert.SerializeObject(userData));
                    }



                }
            }
        }
    }
}
