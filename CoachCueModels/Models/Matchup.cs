using CoachCue.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoachCue.Models
{
    public class Matchup
    {
        public Matchup()
        {
            this.Votes = new List<MatchupVote>();
            this.Players = new List<MatchupPlayer>();
            this.Messages = new List<Message>();
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "isActive")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "isCompleted")]
        public bool Completed { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "scoringType")]
        public string ScoringType { get; set; }

        //WDIS
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "votes")]
        public List<MatchupVote> Votes { get; set; }

        [JsonProperty(PropertyName = "players")]
        public List<MatchupPlayer> Players { get; set; }

        [JsonProperty(PropertyName = "messages")]
        public List<Message> Messages { get; set; }

        [JsonProperty(PropertyName = "fullName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "profileImage")]
        public string ProfileImage { get; set; }

        public string TimeAgo
        {
            get
            {
                return twitter.GetRelativeTime(this.DateCreated);
            }
        }
    }
}