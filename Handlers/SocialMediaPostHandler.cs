using AIForGunSafetyFunctionApp.Models;
using AIForGunSafetyFunctionApp.Services;
using Azure;
using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace AIForGunSafetyFunctionApp.Handlers
{
    public class SocialMediaPostHandler
    {
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

            AzureKeyCredential credentials = new AzureKeyCredential("81b4fe85a0c041eb9ee7f8b9a5532055");
            Uri endpoint = new Uri("https://hackathon22-sentiment-analysis.cognitiveservices.azure.com/");
            var client = new TextAnalyticsClient(endpoint, credentials);
            var documents = new List<string>
            {
                text
            };

            AnalyzeSentimentResultCollection reviews = client.AnalyzeSentimentBatch(documents, options: new AnalyzeSentimentOptions()
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
