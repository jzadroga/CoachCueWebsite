using Newtonsoft.Json;

namespace CoachCue.Models
{
    public class MatchupPlayer : Player
    {
        [JsonProperty(PropertyName = "points")]
        public decimal? Points { get; set; }

        [JsonProperty(PropertyName = "gameWeek")]
        public Game GameWeek { get; set; }

        [JsonProperty(PropertyName = "winner")]
        public bool IsWinner { get; set; }
    }
}