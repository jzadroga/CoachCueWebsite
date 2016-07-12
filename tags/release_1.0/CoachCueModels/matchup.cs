using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class matchup
    {
        public static matchup Get(int matchupID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            matchup match = new matchup();

            try
            {
                var mt = db.matchups.Where(m => m.matchupID == matchupID).FirstOrDefault();
                if (mt != null)
                    match = mt;
            }
            catch (Exception) { }

            return match;
        }

        public static matchup Get(int player1ID, int player2ID, int player1GameID, int player2GameID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            matchup match = new matchup();

            try
            {
                var mt = db.matchups.Where(m => (m.player1ID == player1ID && m.player2ID == player2ID && m.player1gameschedulleID == player1GameID && m.player2gamescheduleID == player2GameID) || ( m.player2ID == player1ID && m.player1ID == player2ID && m.player2gamescheduleID == player1GameID && m.player1gameschedulleID == player2GameID) ).FirstOrDefault();
                if (mt != null)
                    match = mt;
            }
            catch (Exception) { }

            return match;
        }

        public static WeeklyMatchups GetWeeklyMatchupByID(int matchupID, int? userID)
        {
            WeeklyMatchups weeklyMatchup = new WeeklyMatchups();

            try
            {
                weeklyMatchup = GetWeeklyMatchup( Get(matchupID), true, userID);
            }
            catch (Exception) { }

            return weeklyMatchup;
        }

        public static WeeklyMatchups AddUserMatchup(int player1ID, int player2ID, int scoringTypeID, int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            WeeklyMatchups matchup = new WeeklyMatchups();

            try
            {
                int game1 = gameschedule.GetPlayerGame(player1ID);
                int game2 = gameschedule.GetPlayerGame(player2ID);

                //check first if the matchup is already created
                matchup existingMatchup = Get(player1ID, player2ID, game1, game2);
                if (existingMatchup.matchupID != 0)
                {
                    matchup = GetWeeklyMatchup(existingMatchup, false, userID);
                    matchup.ExistingMatchup = true;
                }
                else
                {
                    matchup mtup = new matchup();
                    mtup.player1ID = player1ID;
                    mtup.player2ID = player2ID;
                    mtup.player1gameschedulleID = game1;
                    mtup.player2gamescheduleID = game2;
                    mtup.scoringTypeID = scoringTypeID;
                    mtup.statusID = status.GetID("Active", "matchups");
                    mtup.dateCreated = DateTime.Now;
                    mtup.createdBy = userID;

                    db.matchups.InsertOnSubmit(mtup);
                    db.SubmitChanges();

                    matchup = GetWeeklyMatchup(mtup, false, userID);
                    matchup.ExistingMatchup = false;
                }
            }
            catch (Exception) { }

            return matchup;
        }

        public static void Add(int player1ID, int player2ID, int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                int game1 = gameschedule.GetPlayerGame(player1ID);
                int game2 = gameschedule.GetPlayerGame(player2ID);

                matchup mtup = new matchup();
                mtup.player1ID = player1ID;
                mtup.player2ID = player2ID;
                mtup.player1gameschedulleID = game1;
                mtup.player2gamescheduleID = game2;
                mtup.statusID = status.GetID("Active", "matchups");
                mtup.dateCreated = DateTime.Now;
                mtup.createdBy = userID;

                db.matchups.InsertOnSubmit(mtup);
                db.SubmitChanges();
            }
            catch (Exception) { }
        }

        public static void UpdatePoints(int matchupID, decimal player1Points, decimal player2Points)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                matchup mtup = db.matchups.Where(mtch => mtch.matchupID == matchupID).FirstOrDefault();
                if (mtup != null)
                {
                    mtup.player1Points = player1Points;
                    mtup.player2Points = player2Points;
                    mtup.statusID = status.GetID("Archive", "matchups");
                }
               
                db.SubmitChanges();
                int correctPlayer = ( player1Points > player2Points ) ? mtup.player1ID : mtup.player2ID;

                //now see who got it correct and update
                List<users_matchup> usermtups = db.users_matchups.Where(mtch => mtch.matchupID == matchupID).ToList();
                foreach( users_matchup item in usermtups )
                {
                    item.correctMatchup = ( item.selectedPlayerID == correctPlayer ) ? true : false;
                }

                db.SubmitChanges();
            }
            catch (Exception) { }
        }

        public static List<matchup> List(bool activeOnly)
        {
            List<matchup> matchups = new List<matchup>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.matchups
                          where mt.status.statusName != "Deleted"
                          select mt;

                if (activeOnly)
                    ret = ret.Where(mtch => mtch.status.statusName == "Active");

                matchups = ret.ToList();
            }
            catch (Exception)
            {
            }

            return matchups;
        }

        public static List<MatchupByWeek> GetUserMatchup(int matchupID)
        {
            List<MatchupByWeek> weekMatchups = new List<MatchupByWeek>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var ret = (from mtch in db.matchups
                           where mtch.matchupID == matchupID
                           select mtch).OrderByDescending(mtch => mtch.dateCreated).ToList();

                weekMatchups = getWeeklyMatchups(ret);
            }
            catch (Exception) { }

            return weekMatchups;
        }

        public static List<WeeklyMatchups> GetUserMatchupsByWeek(int userID, int weekID)
        {
            List<WeeklyMatchups> weekMatchups = new List<WeeklyMatchups>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var ret = (from mtch in db.matchups
                           where mtch.createdBy == userID
                           select mtch).OrderByDescending(mtch => mtch.dateCreated).ToList();

                
                if( ret.Count() > 0 )
                {
                    var weeklyMatchups = ret.ToList().Where(r => r.gameschedule.weekNumber == weekID);
                    foreach (matchup item in weeklyMatchups)
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, userID);
                        match.Status = item.status.statusName;
                        match.AllowVote = false;
                        //only let them ivite people if the game isnt over
                        match.AllowInvite = (item.gameschedule.gameDate > DateTime.Now && item.gameschedule1.gameDate > DateTime.Now ) ? true : false;
                        weekMatchups.Add(match); 
                    }
                }
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static List<WeeklyMatchups> GetMatchupsByWeek(int userID, int weekID)
        {
            List<WeeklyMatchups> weekMatchups = new List<WeeklyMatchups>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var weeklyMatchups = (from mtch in db.matchups
                           where mtch.gameschedule.weekNumber == weekID &&
                           ( mtch.status.statusName == "Active" || mtch.status.statusName == "Archive")
                           select mtch).OrderByDescending(mtch => mtch.dateCreated).ToList();


                if (weeklyMatchups.Count() > 0)
                {
                    foreach (matchup item in weeklyMatchups)
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, null);
                        match.AllowVote = false;
                        if (userID != 0)
                        {
                            match.HasVoted = HasVoted(userID, match.MatchupID);
                            if (!match.HasVoted)
                            {
                                //only let them ivite people if the game isnt over
                                DateTime timeUtc = DateTime.UtcNow;
                                TimeZoneInfo easterZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                                DateTime eastTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easterZone);

                                match.AllowVote = (item.gameschedule.gameDate > eastTime && item.gameschedule1.gameDate > eastTime) ? true : false;
                            }
                        }
                        
                        weekMatchups.Add(match);
                    }
                }
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static List<MatchupByWeek> GetUserMatchups(int userID)
        {
            List<MatchupByWeek> weekMatchups = new List<MatchupByWeek>();
            
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var ret = (from mtch in db.matchups                       
                            where mtch.createdBy == userID
                            select mtch).OrderByDescending(mtch => mtch.dateCreated).ToList();

                weekMatchups = getWeeklyMatchups(ret);
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static List<MatchupByWeek> GetSelectedMatchups(int userID)
        {
            List<MatchupByWeek> weekMatchups = new List<MatchupByWeek>();
            
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var ret = (from usrmtch in db.users_matchups                       
                            where usrmtch.userID == userID
                           select usrmtch).OrderByDescending(mtch => mtch.dateCreated).ToList();

                //sort the matchups by weeks
                for (int i = 1; i <= 16; i++)
                {
                    var weeklyMatchups = ret.Where(r => r.matchup.gameschedule.weekNumber == i);
                    List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();

                    foreach (users_matchup usermatchup in weeklyMatchups)
                    {
                        matchup item = usermatchup.matchup;
                        bool correct = false;
                        if (usermatchup.correctMatchup.HasValue)
                            correct = usermatchup.correctMatchup.Value;

                        PlayerMatchup player1 = new PlayerMatchup();
                        player1.Image = (item.nflplayer.twitteraccount != null) ? item.nflplayer.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer.nflteam.teamSlug + ".jpg";
                        player1.PlayerID = item.nflplayer.playerID;
                        player1.PlayerName = item.nflplayer.firstName + " " + item.nflplayer.lastName;
                        player1.PlayerDescription = item.nflplayer.position + " " + item.nflplayer.nflteam.teamName;
                        player1.Href = (userID != 0) ? "#" : "#register-modal";
                        player1.CssClass = (userID != 0) ? "stream-select-starter" : "";
                        player1.GameInfo = item.gameschedule.nflteam.Mascot + " @ " + item.gameschedule.nflteam1.Mascot + " " + item.gameschedule.gameDate.ToString("ddd") + " " + item.gameschedule.gameDate.ToShortTimeString();
                        player1.Selected = (item.nflplayer.playerID == usermatchup.selectedPlayerID) ? true : false;

                        PlayerMatchup player2 = new PlayerMatchup();
                        player2.Image = (item.nflplayer1.twitteraccount != null) ? item.nflplayer1.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer1.nflteam.teamSlug + ".jpg";
                        player2.PlayerID = item.nflplayer1.playerID;
                        player2.PlayerName = item.nflplayer1.firstName + " " + item.nflplayer1.lastName;
                        player2.PlayerDescription = item.nflplayer1.position + " " + item.nflplayer1.nflteam.teamName;
                        player2.Href = (userID != 0) ? "#" : "#register-modal";
                        player2.CssClass = (userID != 0) ? "stream-select-starter" : "";
                        player2.GameInfo = item.gameschedule1.nflteam.Mascot + " @ " + item.gameschedule1.nflteam1.Mascot + " " + item.gameschedule1.gameDate.ToString("ddd") + " " + item.gameschedule1.gameDate.ToShortTimeString();
                        player2.Selected = (item.nflplayer1.playerID == usermatchup.selectedPlayerID) ? true : false;

                        userMatchups.Add(new WeeklyMatchups { MatchUpCorrect = correct, MatchupID = item.matchupID, Player1 = player1, Player2 = player2, ScoringFormat = item.matchupscoringtype.scoringType, Status = item.status.statusName });
                    }

                    weekMatchups.Add(new MatchupByWeek { WeekNumber = i, Matchups = userMatchups });
                }
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static bool HasVoted(int userID, int matchupID)
        {
            bool voted = false;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var ret = (from usrMatch in db.users_matchups
                           where usrMatch.userID == userID
                           select usrMatch.matchupID).ToList();

                if (ret.Count() > 0)
                {
                    usrMtchs = ret.ToList();
                    if (usrMtchs.Contains(matchupID))
                        voted = true;
                }
            }
            catch (Exception) { }

            return voted;
        }

        //gets all the most recent matchups per player - should also get if following a user?
        public static List<WeeklyMatchups> GetRecentList(int playerID, int? userID, DateTime? fromDate = null)
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var mtQuery = from mtch in db.matchups
                              where (mtch.player1ID == playerID || mtch.player2ID == playerID) && mtch.status.statusName == "Active"
                              select mtch;

                if (fromDate.HasValue)
                    mtQuery = mtQuery.Where(match => match.dateCreated > fromDate);

                if (mtQuery.Count() > 0)
                {
                    foreach (matchup item in mtQuery.OrderByDescending(mtch => mtch.dateCreated).ToList())
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, userID);
                        userMatchups.Add(match);
                    }
                }
            }
            catch (Exception) { }

            return userMatchups;
        }

        //gets all the most recent matchups that the user created or created by someone they follow
        public static List<WeeklyMatchups> GetRecentList(int userID,  List<int> followIDs, DateTime? fromDate = null)
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var mtQuery = from mtch in db.matchups
                           where mtch.createdBy == userID || followIDs.Contains(mtch.createdBy)
                           select mtch;

                if (fromDate.HasValue)
                    mtQuery = mtQuery.Where(match => match.dateCreated > fromDate);

                if (mtQuery.Count() > 0)
                {
                    foreach (matchup item in mtQuery.OrderByDescending(mtch => mtch.dateCreated).ToList())
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, userID);
                        userMatchups.Add(match);
                    }
                }
            }
            catch (Exception) { }

            return userMatchups;
        }

        public static List<WeeklyMatchups> GetUserMatchups(int? userID)
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var ret = (from mt in db.matchups
                          where mt.status.statusName == "Active" && mt.gameschedule.gameDate > DateTime.Now orderby mt.dateCreated descending
                          select mt).ToList();

                //no user so just display a random one
                if (!userID.HasValue)
                {
                    int random = new Random().Next(0, ret.Count - 1);
                    ret = ret.GetRange(random, 1);
                }
                else
                {
                    usrMtchs = (from usrMatch in db.users_matchups
                        where usrMatch.userID == userID
                        select usrMatch.matchupID).ToList();
                }

                foreach (matchup item in ret)
                {
                    //don't add matchups that the user already selected
                    if (userID.HasValue)
                    {
                        if( usrMtchs.Contains( item.matchupID ) )
                            continue;
                    }

                    PlayerMatchup player1 = new PlayerMatchup();
                    player1.Image = (item.nflplayer.twitteraccount != null) ? item.nflplayer.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer.nflteam.teamSlug + ".jpg";
                    player1.PlayerID = item.nflplayer.playerID;
                    player1.PlayerName = item.nflplayer.firstName + " " + item.nflplayer.lastName;
                    player1.PlayerDescription = item.nflplayer.position + " " + item.nflplayer.nflteam.teamName;
                    player1.Href = (userID.HasValue) ? "#" : "#register-modal";
                    player1.CssClass = (userID.HasValue) ? "stream-select-starter" : "";
                    player1.GameInfo = item.gameschedule.nflteam1.Mascot + " @ " + item.gameschedule.nflteam.Mascot + " " + item.gameschedule.gameDate.ToString("ddd") + " " + item.gameschedule.gameDate.ToShortTimeString();

                    PlayerMatchup player2 = new PlayerMatchup();
                    player2.Image = (item.nflplayer1.twitteraccount != null) ? item.nflplayer1.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer1.nflteam.teamSlug + ".jpg";
                    player2.PlayerID = item.nflplayer1.playerID;
                    player2.PlayerName = item.nflplayer1.firstName + " " + item.nflplayer1.lastName;
                    player2.PlayerDescription = item.nflplayer1.position + " " + item.nflplayer1.nflteam.teamName;
                    player2.Href = (userID.HasValue) ? "#" : "#register-modal";
                    player2.CssClass = (userID.HasValue) ? "stream-select-starter" : "";
                    player2.GameInfo = item.gameschedule1.nflteam1.Mascot + " @ " + item.gameschedule1.nflteam.Mascot + " " + item.gameschedule1.gameDate.ToString("ddd") + " " + item.gameschedule1.gameDate.ToShortTimeString();

                    userMatchups.Add(new WeeklyMatchups { MatchupID = item.matchupID, Player1 = player1, Player2 = player2, ScoringFormat = item.matchupscoringtype.scoringType });
                }

                //add one empty final item so we know the list is over
                userMatchups.Add(new WeeklyMatchups { MatchupID = 0 });

            }
            catch (Exception)
            {
            }

            return userMatchups;
        }

        private static List<MatchupByWeek> getWeeklyMatchups(List<matchup> matchupList)
        {
            List<MatchupByWeek> weekMatchups = new List<MatchupByWeek>();

            //sort the matchups by weeks
            try
            {
                for( int i = 1; 1 <=16; i++)
                {
                    var weeklyMatchups = matchupList.Where(r => r.gameschedule.weekNumber == i);
                    List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
  
                    foreach (matchup item in weeklyMatchups)
                    {
                        userMatchups.Add(GetWeeklyMatchup(item, false));
                    }

                    weekMatchups.Add(new MatchupByWeek { WeekNumber = i, Matchups = userMatchups });
                }
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static WeeklyMatchups GetWeeklyMatchup(matchup item, bool showAllVotes, int? userID = null)
        {
            WeeklyMatchups weeklyMatchup = new WeeklyMatchups();
            CoachCueDataContext db = new CoachCueDataContext();
 
            IEnumerable<UserVoteData> usersVoted = new List<UserVoteData>();

            PlayerMatchup player1 = new PlayerMatchup();
            player1.Image = (item.nflplayer.twitteraccount != null) ? item.nflplayer.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer.nflteam.teamSlug + ".jpg";
            player1.PlayerID = item.nflplayer.playerID;
            player1.PlayerName = item.nflplayer.firstName + " " + item.nflplayer.lastName;
            player1.PlayerDescription = item.nflplayer.position.positionName + " " + item.nflplayer.nflteam.teamName;
            player1.GameInfo = item.gameschedule.nflteam.Mascot + " @ " + item.gameschedule.nflteam1.Mascot + " " + item.gameschedule.gameDate.ToString("ddd") + " " + item.gameschedule.gameDate.ToShortTimeString();
            player1.Href = "#register-modal";
            player1.CssClass = string.Empty;
            if (userID.HasValue)
            {
                if (userID.Value != 0)
                {
                    player1.Following = (from usracnt in db.users_accounts
                                         where usracnt.userID == userID && usracnt.accountID == item.nflplayer.playerID
                                         select usracnt).Count();
                    player1.Href = "#";
                    player1.CssClass = "stream-select-starter";
                }
            }
            

            PlayerMatchup player2 = new PlayerMatchup();
            player2.Image = (item.nflplayer1.twitteraccount != null) ? item.nflplayer1.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer1.nflteam.teamSlug + ".jpg";
            player2.PlayerID = item.nflplayer1.playerID;
            player2.PlayerName = item.nflplayer1.firstName + " " + item.nflplayer1.lastName;
            player2.PlayerDescription = item.nflplayer1.position.positionName + " " + item.nflplayer1.nflteam.teamName;
            player2.GameInfo = item.gameschedule1.nflteam.Mascot + " @ " + item.gameschedule1.nflteam1.Mascot + " " + item.gameschedule1.gameDate.ToString("ddd") + " " + item.gameschedule1.gameDate.ToShortTimeString();
            player2.Href = "#register-modal";
            player2.CssClass = string.Empty;
            if (userID.HasValue)
            {
                if (userID.Value != 0)
                {
                    player2.Following = (from usracnt in db.users_accounts
                                         where usracnt.userID == userID && usracnt.accountID == item.nflplayer1.playerID
                                         select usracnt).Count();
                    player2.Href = "#";
                    player2.CssClass = "stream-select-starter";
                }
            }
            //get the users who have voted on this matchup
            usersVoted = item.users_matchups.Select(um => um).Select(usrMatch => new UserVoteData
            {
                email = usrMatch.user.email,
                DateCreated = usrMatch.dateCreated,
                fullName = usrMatch.user.fullName,
                profileImg = "/assets/img/avatar/" + usrMatch.user.avatar.imageName,
                username = usrMatch.user.userName,
                userID = usrMatch.userID,
                correctPercentage = ( usrMatch.user.CorrectPercentage != 0 ) ? usrMatch.user.CorrectPercentage + "%" : string.Empty,
                CorrectMatchup = (usrMatch.correctMatchup.HasValue) ? usrMatch.correctMatchup.Value : false,
                SelectedPlayerID = usrMatch.selectedPlayerID,
                SelectedPlayer = (userID.HasValue) ? usrMatch.nflplayer.fullName : usrMatch.nflplayer.firstName.Substring(0, 1) + ". " + usrMatch.nflplayer.lastName
            });

            player1.TotalVotes = usersVoted.Where(usrVote => usrVote.SelectedPlayerID == player1.PlayerID).Count();
            player2.TotalVotes = usersVoted.Where(usrVote => usrVote.SelectedPlayerID == player2.PlayerID).Count();

            int voteCount = usersVoted.Count();
            if (voteCount <= 0)
                usersVoted = new List<UserVoteData>();

            weeklyMatchup = new WeeklyMatchups {  ShowFollow = (userID.HasValue) ? true : false,
                    WeekNumber = item.gameschedule.weekNumber, 
                    NoVotes = (voteCount > 0) ? false : true, 
                    TotalVotes = voteCount,
                    ShowCoaches = (showAllVotes) ?  usersVoted.OrderByDescending(vt => vt.DateCreated).ToList() :  usersVoted.OrderByDescending(vt => vt.DateCreated).Take(2).ToList(),
                    HideCoaches = (showAllVotes) ? new List<UserVoteData>() : usersVoted.OrderByDescending(vt => vt.DateCreated).Skip(2).ToList(),
                    Coaches = usersVoted.OrderBy( vt => vt.DateCreated ).ToList(), 
                    InvitedCoaches = notification.GetInvitedToAnswer(item.matchupID),
                    MatchupID = item.matchupID, 
                    Player1 = player1, 
                    Player2 = player2,
                    Status = item.status.statusName,
                    ScoringFormat = item.matchupscoringtype.scoringType,
                    DateCreated = item.dateCreated,
                    CreatedBy = item.user,
                    GameDate = (item.gameschedule.gameDate < item.gameschedule1.gameDate) ? item.gameschedule.gameDate : item.gameschedule1.gameDate
            };

            //see if the user has a vote
             if( userID.HasValue ){
                UserVoteData userVote = usersVoted.Where(uv => uv.userID == userID.Value).FirstOrDefault();
                if (userVote != null)
                    weeklyMatchup.UserSelectedPlayer = userVote.SelectedPlayer;
            }

            return weeklyMatchup;
        }
    }

    public class MatchupByWeek
    {
        public int WeekNumber { get; set; }
        public List<WeeklyMatchups> Matchups { get; set; }
    }

    public class WeeklyMatchups
    {
        public PlayerMatchup Player1 { get; set; }
        public PlayerMatchup Player2 { get; set; }
        public int MatchupID { get; set; }
        public bool MatchUpCorrect { get; set; }
        public IEnumerable<UserVoteData> Coaches { get; set; }
        public IEnumerable<UserVoteData> ShowCoaches { get; set; }
        public IEnumerable<UserVoteData> HideCoaches { get; set; }
        public IEnumerable<UserVoteData> InvitedCoaches { get; set; }
        public bool NoVotes { get; set; }
        public int WeekNumber { get; set; }
        public bool ShowFollow { get; set; }
        public bool AllowVote { get; set; }
        public bool AllowInvite { get; set; }
        public bool HasVoted { get; set; }
        public string ScoringFormat { get; set; }
        public DateTime DateCreated { get; set; }
        public user CreatedBy { get; set; }
        public string UserSelectedPlayer { get; set; }
        public DateTime GameDate { get; set; }
        public bool ExistingMatchup { get; set; }
        public string Status { get; set; }
        public int TotalVotes { get; set; }
    }

    public class PlayerMatchup
    {
        public string PlayerName { get; set; }
        public int PlayerID { get; set; }
        public string Image { get; set; }
        public string PlayerDescription { get; set; }
        public string Href { get; set; }
        public string CssClass {get; set;}
        public string GameInfo { get; set; }
        public bool Selected { get; set; }
        public int TotalVotes { get; set; }
        public int Following { get; set; }
        public string linkFullName
        {
            get
            {
                string name = this.PlayerName.Replace(".", "").Replace("-", "").Replace("'", "");
                return name;
            }
        }
    }
}