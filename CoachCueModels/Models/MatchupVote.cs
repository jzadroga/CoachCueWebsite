using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class MatchupVote
    {
        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "correct")]
        public bool IsCorrect { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }
    }
}