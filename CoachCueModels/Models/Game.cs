﻿using Newtonsoft.Json;
using System;

namespace CoachCue.Models
{
    public class Game
    {
        public Game()
        {
            this.AwayTeam = new Team();
            this.HomeTeam = new Team();
            this.Season = 2017;
        }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "season")]
        public int Season { get; set; }

        [JsonProperty(PropertyName = "homeTeam")]
        public Team HomeTeam { get; set; }

        [JsonProperty(PropertyName = "awayTeam")]
        public Team AwayTeam { get; set; }

        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "week")]
        public int Week { get; set; }
    }
}