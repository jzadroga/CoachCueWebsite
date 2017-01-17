using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class UserStatistics
    {
        [JsonProperty(PropertyName = "loginCount")]
        public int LoginCount { get; set; }

        [JsonProperty(PropertyName = "lastLogin")]
        public DateTime LastLogin { get; set; }
    }
}