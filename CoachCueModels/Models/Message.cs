using CoachCue.Model;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoachCue.Models
{
    public class Message
    {
        public Message()
        {
            this.PlayerMentions = new List<Player>();
            this.Reply = new List<Message>();
            this.UserMentions = new List<User>();
            this.Media = new List<Media>();
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "media")]
        public List<Media> Media { get; set; }

        [JsonProperty(PropertyName = "playerMentions")]
        public List<Player> PlayerMentions { get; set; }

        [JsonProperty(PropertyName = "userMentions")]
        public List<User> UserMentions { get; set; }

        [JsonProperty(PropertyName = "reply")]
        public List<Message> Reply { get; set; }

        [JsonProperty(PropertyName = "fullName")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "userName")]
        public string UserName { get; set; }

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