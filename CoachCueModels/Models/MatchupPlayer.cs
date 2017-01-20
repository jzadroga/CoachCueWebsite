using Newtonsoft.Json;

namespace CoachCue.Models
{
    public class MatchupPlayer
    {
        [JsonProperty(PropertyName = "playerId")]
        public string PlayerId { get; set; }

        [JsonProperty(PropertyName = "points")]
        public decimal? Points { get; set; }

        [JsonProperty(PropertyName = "gameId")]
        public string GameId { get; set; }

        [JsonProperty(PropertyName = "winner")]
        public bool IsWinner { get; set; }
    }
}