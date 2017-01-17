using Newtonsoft.Json;

namespace CoachCue.Models
{
    public class Team
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "slug")]
        public string Slug { get; set; }
    }
}