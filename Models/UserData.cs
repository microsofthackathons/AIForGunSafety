using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIForGunSafetyFunctionApp.Models
{
    public class UserData
    {
        public User TwitterUser { get; set; }

        public double SentimentScore{ get; set; }

        public string TweetText { get; set; }

        public Geo Location { get; set; }
    }
}
