using Newtonsoft.Json;

namespace CoachCue.Models
{
    public class Player
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "position")]
        public string Position { get; set; }

        [JsonProperty(PropertyName = "number")]
        public int? Number { get; set; }

        [JsonProperty(PropertyName = "isActive")]
        public bool Active { get; set; }    

        [JsonProperty(PropertyName = "twitter")]
        public TwitterAccount Twitter { get; set; }

        [JsonProperty(PropertyName = "team")]
        public Team Team { get; set; }
    }
}