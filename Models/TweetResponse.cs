using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIForGunSafetyFunctionApp.Models
{
    public class Rootobject
    {
        public Tweet[] data { get; set; }

        [JsonProperty("meta")]
        public Metadata metadata { get; set; }
    }


    public class Metadata
    {
        public string newest_id { get; set; }
        public string oldest_id { get; set; }
        public int result_count { get; set; }
        public string next_token { get; set; }
    }

    public class Tweet
    {
        public string id { get; set; }
        public string text { get; set; }
        public bool possibly_sensitive { get; set; } = false;
        public string author_id { get; set; }
        public Geo geo { get; set; }
    }


    public class Geo
    {
        public Coordinates coordinates { get; set; }
        public string place_id { get; set; }
    }

    public class Coordinates
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }


}
