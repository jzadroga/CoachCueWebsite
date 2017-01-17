using Newtonsoft.Json;

namespace CoachCue.Models
{
    public class TwitterAccount
    {
        [JsonProperty(PropertyName = "isActive")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "profileImage")]
        public string ProfileImage { get; set; }
    }
}
