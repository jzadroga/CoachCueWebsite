using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class Badge
    {
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }
    }
}