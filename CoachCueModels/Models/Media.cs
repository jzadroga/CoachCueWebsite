using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class Media
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "objectUrl")]
        public string ObjectUrl { get; set; }
    }
}