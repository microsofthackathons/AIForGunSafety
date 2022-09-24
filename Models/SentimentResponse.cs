using Azure.AI.TextAnalytics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIForGunSafetyFunctionApp.Models
{
    public class SentimentResponse
    {
        public TextSentiment Sentiment { get; set; }
        public double SentimentScore { get; set; }
    }
}
