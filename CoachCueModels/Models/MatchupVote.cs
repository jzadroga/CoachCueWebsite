using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class MatchupVote
    {
        public MatchupVote()
        {
            this.ProfileImage = "sm_profile.jpg";
            this.Name = "Guest";
        }

        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "playerName")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "fullName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "profileImage")]
        public string ProfileImage { get; set; }

        [JsonProperty(PropertyName = "correct")]
        public bool IsCorrect { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        public string UserLink
        {
            get
            {
                return string.IsNullOrEmpty(this.UserName) ? string.Empty : this.UserName.Replace(".", "").Replace("-", "").Replace("'", "");
            }
        }
    }
}