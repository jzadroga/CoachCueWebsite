using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class Media
    {
        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "objectUrl")]
        public string ObjectUrl { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "site")]
        public string Site { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        public string SecureUrl
        {
            get
            {
                return (string.IsNullOrEmpty(this.Url)) ? string.Empty : this.Url.Replace("http:", "https:");
            }
        }

        public string SecureObjectUrl
        {
            get
            {
                return (string.IsNullOrEmpty(this.ObjectUrl)) ? string.Empty : this.ObjectUrl.Replace("http:", "https:");
            }
        }
    }
}