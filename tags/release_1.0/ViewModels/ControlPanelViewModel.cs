using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoachCue.Model;

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
        public List<nflteam> Teams { get; set; }
        public int SelectedTeamID { get; set; }
        public List<twitteraccount> Journalists { get; set; }
    }

    public class TeamRosterModel
    {
        public List<nflteam> Teams { get; set; }
        public int SelectedTeamID { get; set; }
        public List<nflplayer> Players { get; set; }
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