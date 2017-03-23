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

        [JsonProperty(PropertyName = "voteCount")]
        public int VoteCount { get; set; }

        [JsonProperty(PropertyName = "matchupCount")]
        public int MatchupCount { get; set; }

        [JsonProperty(PropertyName = "correctVoteCount")]
        public int CorrectVoteCount { get; set; }

        [JsonProperty(PropertyName = "messageCount")]
        public int MessageCount { get; set; }
    }
}