using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoachCue.Model;
using CoachCue.Models;

namespace CoachCue.ViewModels
{
    public class MatchupsViewModel
    {
        public List<gameschedule> Games { get; set; }
        public List<matchup> Matchups { get; set; }
        public List<PlayersByPostion> Players { get; set; }
    }

    public class MessagesViewModel
    {
        public List<message> Messages { get; set; }
    }

    public class TeamJournalistModel
    {
        public List<Team> Teams { get; set; }
        public string SelectedTeam { get; set; }
        public List<string> Journalists { get; set; }
    }

    public class TeamRosterModel
    {
        public List<Team> Teams { get; set; }
        public string SelectedTeam { get; set; }
        public IEnumerable<Player> Players { get; set; }
    }

    public class TeamPlayerAccountModel
    {
        public nflteam Team { get; set; }
        public List<nflplayer> PlayerAccounts { get; set; }
    }

    public class UsersModel
    {
        public List<user> Users { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public int Total { get; set; }
    }
}