using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace CoachCue.Models
{
    public class Team
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "slug")]
        public string Slug { get; set; }

        public static List<Team> GetList()
        {
            List<Team> teams = new List<Team>();

            //hardcode all the nfl teams
            teams.Add(new Team { Name = "Arizona Cardinals", Slug = "ARI" });
            teams.Add(new Team { Name = "Atlanta Falcons", Slug = "ATL" });
            teams.Add(new Team { Name = "Baltimore Ravens", Slug = "BAL" });
            teams.Add(new Team { Name = "Buffalo Bills", Slug = "BUF" });
            teams.Add(new Team { Name = "Carolina Panthers", Slug = "CAR" });
            teams.Add(new Team { Name = "Chicago Bears", Slug = "CHI" });
            teams.Add(new Team { Name = "Cincinnati Bengals", Slug = "CIN" });
            teams.Add(new Team { Name = "Cleveland Browns", Slug = "CLE" });
            teams.Add(new Team { Name = "Dallas Cowboys", Slug = "DAL" });
            teams.Add(new Team { Name = "Denver Broncos", Slug = "DEN" });
            teams.Add(new Team { Name = "Detroit Lions", Slug = "DET" });
            teams.Add(new Team { Name = "Green Bay Packers", Slug = "GB" });
            teams.Add(new Team { Name = "Houston Texans", Slug = "HOU" });
            teams.Add(new Team { Name = "Indianapolis Colts", Slug = "IND" });
            teams.Add(new Team { Name = "Jacksonville Jaguars", Slug = "JAC" });
            teams.Add(new Team { Name = "Kansas City Chiefs", Slug = "KC" });
            teams.Add(new Team { Name = "Los Angeles Rams", Slug = "LA" });
            teams.Add(new Team { Name = "Los Angeles Chargers", Slug = "LAC" });
            teams.Add(new Team { Name = "Miami Dolphins", Slug = "MIA" });
            teams.Add(new Team { Name = "Minnesota Vikings", Slug = "MIN" });
            teams.Add(new Team { Name = "New England Patriots", Slug = "NE" });
            teams.Add(new Team { Name = "New Orleans Saints", Slug = "NO" });
            teams.Add(new Team { Name = "New York Giants", Slug = "NYG" });
            teams.Add(new Team { Name = "New York Jets", Slug = "NYJ" });
            teams.Add(new Team { Name = "Oakland Raiders", Slug = "OAK" });
            teams.Add(new Team { Name = "Philadelphia Eagles", Slug = "PHI" });
            teams.Add(new Team { Name = "Pittsburgh Steelers", Slug = "PIT" });
            teams.Add(new Team { Name = "Seattle Seahawks", Slug = "SEA" });
            teams.Add(new Team { Name = "San Francisco 49ers", Slug = "SF" });
            teams.Add(new Team { Name = "Tampa Bay Buccaneers", Slug = "TB" });
            teams.Add(new Team { Name = "Tennessee Titans", Slug = "TEN" });
            teams.Add(new Team { Name = "Washington Redskins", Slug = "WAS" });

            return teams;
        }

        public static Team GetTeamBySlug(string slug)
        {
            return GetList().Where(tm => tm.Slug == slug).FirstOrDefault();
        }
    }
}