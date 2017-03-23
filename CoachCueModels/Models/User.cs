using CoachCue.Model;
using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class User
    {
        public User()
        {
            this.Statistics = new UserStatistics()
                { VoteCount = 0,
                    LoginCount = 0,
                    LastLogin = DateTime.UtcNow.GetEasternTime(),
                    MatchupCount = 0,
                    CorrectVoteCount = 0,
                    MessageCount = 0
                };
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "fullName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "isActive")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "isAdmin")]
        public bool Admin { get; set; }

        [JsonProperty(PropertyName = "verified")]
        public bool Verified { get; set; }

        [JsonProperty(PropertyName = "referrer")]
        public string Referrer { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "profile")]
        public UserProfile Profile { get; set; }

        [JsonProperty(PropertyName = "settings")]
        public UserSettings Settings { get; set; }

        [JsonProperty(PropertyName = "stats")]
        public UserStatistics Statistics { get; set; }

        [JsonProperty(PropertyName = "link")]
        public string Link
        {
            get
            {
                string name = this.UserName.Replace(".", "").Replace("-", "").Replace("'", "");
                return name.Replace(" ", "-").ToLower();
            }
        }
    }
}