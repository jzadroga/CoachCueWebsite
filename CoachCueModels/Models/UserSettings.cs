using Newtonsoft.Json;

namespace CoachCue.Models
{
    public class UserSettings
    {
        [JsonProperty(PropertyName = "emailNotifications")]
        public bool EmailNotifications { get; set; }
    }
}