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
        public List<Matchup> Matchups { get; set; }
    }

    public class MessagesViewModel
    {
        public IEnumerable<Message> Messages { get; set; }
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
        public List<User> Users { get; set; }
        public int Page { get; set; }
        public int PageCount { get; set; }
        public int Total { get; set; }
        public string Search { get; set; }
    }
}