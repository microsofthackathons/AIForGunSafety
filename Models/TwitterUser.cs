using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIForGunSafetyFunctionApp.Models
{
    public class TwitterUser
    {
        public User[] data { get; set; }
    }

    public class User
    {
        public string description { get; set; }
        public string id { get; set; }
        public bool verified { get; set; }
        public string profile_image_url { get; set; }
        public bool _protected { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public DateTime created_at { get; set; }
        public string username { get; set; }

    }
}
