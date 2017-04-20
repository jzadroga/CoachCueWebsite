using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace CoachCue.Models
{
    public class Schedule
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "games")]
        public List<Game> Games { get; set; }
    }

    public class Game
    {
        [JsonProperty(PropertyName = "season")]
        public int Season { get; set; }

        [JsonProperty(PropertyName = "homeTeam")]
        public string HomeTeam { get; set; }

        [JsonProperty(PropertyName = "awayTeam")]
        public string AwayTeam { get; set; }

        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }

        [JsonProperty(PropertyName = "week")]
        public int Week { get; set; }

        public DateTime? GameDate
        {
            get
            {
                if (string.IsNullOrEmpty(Date))
                    return null;

                return DateTime.ParseExact(Date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
        }
    }
}