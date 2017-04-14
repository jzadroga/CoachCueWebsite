using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoachCue.Models
{
    public class Notification
    {
        public Notification()
        {
            this.Sent = false;
            this.Read = false;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty(PropertyName = "createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "userFrom")]
        public string UserFrom { get; set; }

        [JsonProperty(PropertyName = "userTo")]
        public string UserTo { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "matchup")]
        public string Matchup { get; set; }

        [JsonProperty(PropertyName = "sent")]
        public bool Sent { get; set; }

        [JsonProperty(PropertyName = "read")]
        public bool Read { get; set; }
    }

    public class NotificationNotice
    {
        public Notification Notification { get; set; }
        public User UserFrom { get; set; }
        public Message Message { get; set; }
        public Matchup Matchup { get; set; }
    }
}