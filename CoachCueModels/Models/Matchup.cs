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

        [JsonProperty(PropertyName = "link")]
        public string Link { get; set; }
        //should be the matchup type, then the week number, then the players names seperated by -

        public string TimeAgo
        {
            get
            {
                return twitter.GetRelativeTime(this.DateCreated);
            }
        }

        public string UserLink
        {
            get
            {
                string name = this.UserName.Replace(".", "").Replace("-", "").Replace("'", "");
                return name.Replace(" ", "-").ToLower();
            }
        }
    }

    public class MatchupType
    {
        public static List<string>  GetList()
        {
            List<string> types = new List<string>();

            types.Add("Who Do I Start? (Standard)");
            types.Add("Who Do I Start? (PPR)");
            types.Add("Who Do I Start? (Daily Fantasy)");
            types.Add("Who Do I Keep?");
            types.Add("Who Do I Add? (Waiver Wire)");
            types.Add("Who Do I Drop? (Waiver Wire)");
            types.Add("Who Do I Draft?");

            return types;
        }
    }
}