using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoachCue.Models
{
    public class Message
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "media")]
        public Media Media { get; set; }

        [JsonProperty(PropertyName = "reply")]
        public List<Message> Reply { get; set; }
    }
}