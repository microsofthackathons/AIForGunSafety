using AIForGunSafetyFunctionApp.Models;
using AIForGunSafetyFunctionApp.Services;
using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AIForGunSafetyFunctionApp.Handlers
{
    /// <summary>
    /// Social media handler 
    /// </summary>
    public class SocialMediaPostHandler
    {
        private string _azureAnalyticsKey;

        public SocialMediaPostHandler()
        {
            var appSettings = ConfigurationManager.AppSettings;
            _azureAnalyticsKey = appSettings["AzureAnalyticsKey"].ToString();
        }
        public async Task ConsumeRecentSocialMediaPosts()
        {
            TwitterService twitterService = new TwitterService();
            await twitterService.ConsumeSocialMediaPosts();
        }


        public async Task<TwitterUser> GetTwitterUser(string twitterUserId)
        {
            TwitterService twitterService = new TwitterService();
            var user = await twitterService.ConsumeGetTwitterUserRequest(twitterUserId);
            return user;
        }

        public async Task<SentimentResponse> GetSentimentScoreByText(string text)
        {
            //TODO:Call sentiment api by passing text.

            AzureKeyCredential credentials = new AzureKeyCredential(_azureAnalyticsKey);
            Uri endpoint = new Uri("https://hackathon22-sentiment-analysis.cognitiveservices.azure.com/");
            var client = new TextAnalyticsClient(endpoint, credentials);
            var documents = new List<string>
            {
                text
            };

            AnalyzeSentimentResultCollection reviews = await client.AnalyzeSentimentBatchAsync(documents, options: new AnalyzeSentimentOptions()
            {
                IncludeOpinionMining = true
            });
            SentimentResponse response = new SentimentResponse();
            foreach (AnalyzeSentimentResult review in reviews)
            {
                response.SentimentScore = review.DocumentSentiment.ConfidenceScores.Negative;
                response.Sentiment = review.DocumentSentiment.Sentiment;
            }

            return response;
        }
        
    }
}
