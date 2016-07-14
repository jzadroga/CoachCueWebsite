using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
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
                weeklyMatchup = GetWeeklyMatchup(Get(matchupID), true, true, userID);
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
                    matchup = GetWeeklyMatchup(existingMatchup, false, false, userID);
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
                    mtup.dateCreated = DateTime.UtcNow.GetEasternTime();
                    mtup.createdBy = userID;

                    db.matchups.InsertOnSubmit(mtup);
                    db.SubmitChanges();

                    matchup = GetWeeklyMatchup(mtup, false, false, userID);
                    matchup.ExistingMatchup = false;
                }
            }
            catch (Exception ex) 
            {
                string msg = ex.Message;
            }

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
                mtup.dateCreated = DateTime.UtcNow.GetEasternTime();
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

                //clear the cache for the update values
                HttpContext.Current.Cache.Remove("matchup" + matchupID.ToString());

                //clear the leaderboard cache too
                string leaderBoardCacheID = "leaders" + "week" + mtup.gameschedule.weekNumber.ToString();
                HttpContext.Current.Cache.Remove(leaderBoardCacheID);
            }
            catch (Exception) { }
        }

        public static void Delete(int matchupID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                matchup mtup = db.matchups.Where(mtch => mtch.matchupID == matchupID).FirstOrDefault();
                if (mtup != null)
                {
                    mtup.statusID = status.GetID("Deleted", "matchups");
                    db.SubmitChanges();
                }

                HttpContext.Current.Cache.Remove("matchup" + matchupID.ToString());
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

        public static List<StreamContent> GetAllUserMatchups(int userID, int number, int currentUserID)
        {
            List<WeeklyMatchups> usrMatchups = new List<WeeklyMatchups>();
            List<StreamContent> streamContent = new List<StreamContent>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                List<matchup> userMatchups = (from mtch in db.matchups
                           where mtch.createdBy == userID
                                              select mtch).OrderByDescending(mtch => mtch.dateCreated).Take(number).ToList();


                if (userMatchups.Count() > 0)
                {
                    ///changed from top to bottom part and knocked off half the time

                    /*foreach (matchup item in userMatchups)
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, false, currentUserID);
                        match.Status = item.status.statusName;
                        match.AllowVote = false;
                        //only let them ivite people if the game isnt over
                        match.AllowInvite = (item.gameschedule.gameDate > DateTime.UtcNow.GetEasternTime() && item.gameschedule1.gameDate > DateTime.UtcNow.GetEasternTime() && userID == currentUserID) ? true : false;
                        usrMatchups.Add(match);
                    }

                    streamContent = usrMatchups.Select(usrmtch => new StreamContent
                    {
                        MatchupItem = usrmtch,
                        DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                        ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                        UserName = usrmtch.CreatedBy.userName,
                        FullName = usrmtch.CreatedBy.fullName,
                        ContentType = stream.GetMatchupContentType(usrmtch),
                        DateCreated = usrmtch.LastDate
                    }).ToList();*/

                    user currentUser = user.Get(currentUserID);

                    streamContent = userMatchups.Select(usrmtch => new StreamContent
                    {
                        MatchupItem = GetWeeklyMatchup(usrmtch, false, false, currentUserID),
                        DateTicks = usrmtch.dateCreated.Ticks.ToString(),
                        ProfileImg = usrmtch.user.avatar.imageName,
                        UserName = usrmtch.user.userName,
                        FullName = usrmtch.user.fullName,
                        UserProfileImg = (currentUser.userID != 0 ) ? currentUser.avatar.imageName : string.Empty
                    }).ToList();

                    foreach( StreamContent content in streamContent)
                    {
                        content.ContentType = stream.GetMatchupContentType(content.MatchupItem);
                        content.DateCreated = content.MatchupItem.LastDate;
                    }
                }
            }
            catch( Exception){}

            return streamContent;
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
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, false, userID);
                        match.Status = item.status.statusName;
                        match.AllowVote = false;
                        //only let them ivite people if the game isnt over
                        match.AllowInvite = (item.gameschedule.gameDate > DateTime.UtcNow.GetEasternTime() && item.gameschedule1.gameDate > DateTime.UtcNow.GetEasternTime()) ? true : false;
                        weekMatchups.Add(match); 
                    }
                }
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static VotedPlayersByPosition GetPlayerProfileVotes(nflplayer player, int weekID, int positionID)
        {
            VotedPlayersByPosition playerVote = new VotedPlayersByPosition();
            playerVote.CurrentPlayer = new VotedPlayers { Percent = "0%", Votes = 0, Player = player };
            playerVote.ComparedPlayers = new List<VotedPlayers>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                List<Trending> votes = new List<Trending>();

                votes = (from vote in db.users_matchups
                            where vote.matchup.gameschedule.weekNumber == weekID && vote.matchup.gameschedule.seasonID == 4 && vote.nflplayer.position.positionID == positionID
                            group vote by vote.selectedPlayerID into g
                            orderby g.Count() descending
                            select new Trending { playerID = g.Key, count = g.Count() }).ToList();  

                //first get the player votes
                Trending selectedPlayer = votes.Where(ply => ply.playerID == player.playerID).FirstOrDefault();
                if( selectedPlayer != null )
                {
                    playerVote.CurrentPlayer.Votes = selectedPlayer.count;
                }

                //now get compares
                List<Trending> comparedPlayers = votes.Where(ply => ply.playerID != player.playerID).Take(4).ToList();

                if (selectedPlayer != null)
                {
                    comparedPlayers.Add(selectedPlayer); //add back the selected player to get the percentage
                    comparedPlayers = comparedPlayers.OrderByDescending(vt => vt.count).ToList();
                }

                if (comparedPlayers.Count > 0)
                {
                    int count = 0;
                    string percent = "100%";
                    decimal topVotes = 0;
                    foreach (Trending userVote in comparedPlayers)
                    {
                        if (count == 0)
                        {
                            topVotes = userVote.count;
                            percent = "100%";
                        }
                        else
                            percent = ((Convert.ToDecimal(userVote.count) / topVotes) * 100).ToString() + "%";

                        if (userVote.playerID == player.playerID)
                            playerVote.CurrentPlayer.Percent = percent;
                        else
                            playerVote.ComparedPlayers.Add(new VotedPlayers { Percent = percent, Votes = userVote.count, Player = nflplayer.Get(userVote.playerID) });
                        
                        count++;
                    }
                }
            }
            catch (Exception) { }

            return playerVote;
        }

        public static List<VotedPlayers> GetTopMathupVotes(int weekID, string positionName = "")
        {
            List<VotedPlayers> playerVotes = new List<VotedPlayers>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                List<Trending> userVotes = new List<Trending>();

                if( !string.IsNullOrEmpty( positionName ))
                {
                    userVotes = (from vote in db.users_matchups
                                 where vote.matchup.gameschedule.weekNumber == weekID && vote.matchup.gameschedule.seasonID == 4 && vote.nflplayer.position.positionName == positionName
                                    group vote by vote.selectedPlayerID into g
                                    orderby g.Count() descending
                                    select new Trending { playerID = g.Key, count = g.Count() }).Take(5).ToList();  
                }
                else
                {
                    userVotes = (from vote in db.users_matchups
                                 where vote.matchup.gameschedule.weekNumber == weekID && vote.matchup.gameschedule.seasonID == 4
                                    group vote by vote.selectedPlayerID into g
                                    orderby g.Count() descending
                                    select new Trending { playerID = g.Key, count = g.Count() }).Take(5).ToList();
                }
                if (userVotes.Count > 0)
                {
                    int count = 0;
                    string percent = "100%";
                    decimal topVotes = 0;
                    foreach (Trending userVote in userVotes)
                    {
                        if (count == 0)
                        {
                            topVotes = userVote.count;
                            percent = "100%";
                        }
                        else
                            percent = ((Convert.ToDecimal(userVote.count) / topVotes) * 100).ToString() + "%";

                        playerVotes.Add(new VotedPlayers { Percent = percent, Votes = userVote.count, Player = nflplayer.Get(userVote.playerID) });
                        count++;
                    }
                }
            }
            catch (Exception) { }

            return playerVotes;
        }

        public static List<WeeklyMatchups> GetMatchupsByWeek(int userID, int weekID, bool detailView)
        {
            List<WeeklyMatchups> weekMatchups = new List<WeeklyMatchups>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var usrMtchs = new List<int>();

                var weeklyMatchups = (from mtch in db.matchups
                           where mtch.gameschedule.weekNumber == weekID && mtch.gameschedule.seasonID == 4 &&
                           ( mtch.status.statusName == "Active" || mtch.status.statusName == "Archive")
                           select mtch).OrderByDescending(mtch => mtch.dateCreated).ToList();

                int? currentUser = (userID != 0) ? (int?)userID : null;

                if (weeklyMatchups.Count() > 0)
                {
                    foreach (matchup item in weeklyMatchups)
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, false, currentUser);
                        match.AllowVote = false;

                        DateTime eastTime = DateTime.UtcNow.GetEasternTime();
                        bool currentMatch =  (item.gameschedule.gameDate > eastTime && item.gameschedule1.gameDate > eastTime) ? true : false;
                        if (userID != 0)
                        {
                            match.HasVoted = HasVoted(userID, match.MatchupID);
                            if (!match.HasVoted)
                            {
                                //only let them ivite people if the game isnt over
                                match.AllowVote = currentMatch;
                            }
                            match.AllowInvite = (match.CreatedBy.userID == userID && currentMatch) ? true : false;
                        }

                        match.DetailView = detailView;
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
                        player1.PlayerDescription = item.nflplayer.position.positionName + " | " + item.nflplayer.nflteam.teamSlug;
                        player1.Href = (userID != 0) ? "#" : "#register-modal";
                        player1.CssClass = (userID != 0) ? "stream-select-starter" : "";
                        player1.GameInfo = item.gameschedule.nflteam.Mascot + " @ " + item.gameschedule.nflteam1.Mascot + " " + item.gameschedule.gameDate.ToString("ddd") + " " + item.gameschedule.gameDate.ToShortTimeString();
                        player1.Selected = (item.nflplayer.playerID == usermatchup.selectedPlayerID) ? true : false;

                        PlayerMatchup player2 = new PlayerMatchup();
                        player2.Image = (item.nflplayer1.twitteraccount != null) ? item.nflplayer1.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer1.nflteam.teamSlug + ".jpg";
                        player2.PlayerID = item.nflplayer1.playerID;
                        player2.PlayerName = item.nflplayer1.firstName + " " + item.nflplayer1.lastName;
                        player2.PlayerDescription = item.nflplayer1.position.positionName + " | " + item.nflplayer1.nflteam.teamSlug;
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
        public static List<WeeklyMatchups> GetRecentList(int playerID, int? userID, bool futureTimeline, DateTime? fromDate = null)
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var mtQuery = from mtch in db.matchups
                              where (mtch.player1ID == playerID || mtch.player2ID == playerID) && mtch.status.statusName == "Active"
                              select mtch;

                if (fromDate.HasValue)
                {
                    if( futureTimeline )
                        mtQuery = mtQuery.Where(match => match.dateCreated > fromDate);
                    else
                        mtQuery = mtQuery.Where(match => match.dateCreated < fromDate);
                }

                if (mtQuery.Count() > 0)
                {
                    foreach (matchup item in mtQuery.OrderByDescending(mtch => mtch.dateCreated).ToList())
                    {
                        WeeklyMatchups match = GetWeeklyMatchup(item, false, false, userID);
                        if( userMatchups.Where( mt => mt.MatchupID == item.matchupID ).Count() == 0 )
                            userMatchups.Add(match);
                    }
                }
            }
            catch (Exception) { }

            return userMatchups;
        }

        //get the matchups the user has created that have been voted in the past
        public static List<WeeklyMatchups> GetPastList(int userID, List<int> followIDs, List<int> followPlayerIDs, DateTime fromDate)
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var mtQuery = from mtch in db.matchups
                              join usrmatch in db.users_matchups on
                                mtch.matchupID equals usrmatch.matchupID
                              where ((mtch.createdBy == userID || followIDs.Contains(mtch.createdBy)) && mtch.dateCreated < fromDate && usrmatch.dateCreated < fromDate) ||
                              ((followPlayerIDs.Contains(mtch.nflplayer.playerID) || followPlayerIDs.Contains(mtch.nflplayer1.playerID)) && mtch.dateCreated < fromDate)
                              select mtch;

                if (mtQuery.Count() > 0)
                {
                    foreach (matchup item in mtQuery.OrderByDescending(mtch => mtch.dateCreated).Take(10).ToList())
                    {
                        //don't add dups
                        if (userMatchups.Where(mt => mt.MatchupID == item.matchupID).Count() == 0)
                        {
                            WeeklyMatchups match = GetWeeklyMatchup(item, false, false, userID);
                            userMatchups.Add(match);
                        }
                    }
                }
            }
            catch (Exception) { }

            return userMatchups;
        }

        //get the matchups the user has created or created by users they follow and about players they follow
        public static List<WeeklyMatchups> GetRecentList(int userID, List<int> followIDs, List<int> followPlayerIDs, DateTime fromDate )
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var mtQuery = from mtch in db.matchups
                             join usrmatch in db.users_matchups on
                                mtch.matchupID equals usrmatch.matchupID
                                where ((mtch.createdBy == userID || followIDs.Contains(mtch.createdBy)) && mtch.dateCreated >= fromDate) ||
                               ((followPlayerIDs.Contains(mtch.nflplayer.playerID) || followPlayerIDs.Contains(mtch.nflplayer1.playerID)) && mtch.dateCreated >= fromDate)
                              where mtch.dateCreated >= fromDate && mtch.status.statusName != "Deleted"
                              select mtch;

                if (mtQuery.Count() > 0)
                {                    
                    foreach (matchup item in mtQuery.OrderByDescending(mtch => mtch.dateCreated).Take(20).ToList())
                    {
                        //don't add dups
                        if (userMatchups.Where(mt => mt.MatchupID == item.matchupID).Count() == 0)
                        {
                            WeeklyMatchups match = GetWeeklyMatchup(item, false, false, userID);
                            userMatchups.Add(match);
                        }
                    }
                }
            }
            catch (Exception) { }

            return userMatchups;
        }

        //get related matchups
        public static List<StreamContent> GetRelated(int userID, WeeklyMatchups currentMatchup)
        {
            List<StreamContent> streamItems = new List<StreamContent>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                user currentUser = user.Get(userID);

                List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();

                var mtQuery = from mtch in db.matchups
                              where mtch.status.statusName != "Deleted" && mtch.dateCreated >= DateTime.UtcNow.GetEasternTime().AddDays(-60) 
                                && mtch.matchupID != currentMatchup.MatchupID && (mtch.nflplayer.positionID == currentMatchup.Player1.PlayerID || mtch.nflplayer1.positionID == currentMatchup.Player2.PositionID)
                              select mtch;

                if (mtQuery.Count() > 0)
                {
                    foreach (matchup item in mtQuery.OrderByDescending(mtch => mtch.dateCreated).Take(10).ToList())
                    {
                        //don't add dups
                        if (userMatchups.Where(mt => mt.MatchupID == item.matchupID).Count() == 0)
                        {
                            WeeklyMatchups match = GetWeeklyMatchup(item, false, false, userID);
                            userMatchups.Add(match);
                        }
                    }
                }

                streamItems = userMatchups.Select(usrmtch => new StreamContent
                {
                    MatchupItem = usrmtch,
                    DateTicks = usrmtch.DateCreated.Ticks.ToString(),
                    ProfileImg = usrmtch.CreatedBy.avatar.imageName,
                    UserProfileImg = (currentUser.userID != 0) ? currentUser.avatar.imageName : string.Empty,
                    UserName = usrmtch.CreatedBy.userName,
                    FullName = usrmtch.CreatedBy.fullName,
                    ContentType = stream.GetMatchupContentType(usrmtch),
                    DateCreated = usrmtch.LastDate
                }).ToList();
            }
            catch( Exception )
            {

            }

            return streamItems;
        }

        //get the matchups based on the date
        public async static Task<List<WeeklyMatchups>> GetList(int userID, DateTime fromDate, bool futureTimeline)
        {
            List<WeeklyMatchups> userMatchups = new List<WeeklyMatchups>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var mtQuery = from mtch in db.matchups                          
                              where mtch.status.statusName != "Deleted"
                              select mtch;

                mtQuery = (!futureTimeline) ? mtQuery.Where(mtch => mtch.dateCreated < fromDate) : mtQuery.Where(mtch => mtch.dateCreated >= fromDate);
             

                if (mtQuery.Count() > 0)
                {
                    var matchups = mtQuery.OrderByDescending(mtch => mtch.dateCreated).Take(40).ToList();
                    foreach (matchup item in matchups.ToList())
                    {
                        //don't add dups
                        if (userMatchups.Where(mt => mt.MatchupID == item.matchupID).Count() == 0)
                        {
                            WeeklyMatchups match = GetWeeklyMatchup(item, false, false, userID); // await GetWeeklyMatchupAsync(item, false, false, userID);
                            userMatchups.Add(match);
                        }
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
                           where mt.status.statusName == "Active" && mt.gameschedule.gameDate > DateTime.UtcNow.GetEasternTime()
                           orderby mt.dateCreated descending
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
                    player1.PlayerDescription = item.nflplayer.position.positionName + " | " + item.nflplayer.nflteam.teamSlug;
                    player1.Href = (userID.HasValue) ? "#" : "#register-modal";
                    player1.CssClass = (userID.HasValue) ? "stream-select-starter" : "";
                    player1.GameInfo = item.gameschedule.nflteam1.Mascot + " @ " + item.gameschedule.nflteam.Mascot + " " + item.gameschedule.gameDate.ToString("ddd") + " " + item.gameschedule.gameDate.ToShortTimeString();

                    PlayerMatchup player2 = new PlayerMatchup();
                    player2.Image = (item.nflplayer1.twitteraccount != null) ? item.nflplayer1.twitteraccount.profileImageUrl : "/assets/img/teams/" + item.nflplayer1.nflteam.teamSlug + ".jpg";
                    player2.PlayerID = item.nflplayer1.playerID;
                    player2.PlayerName = item.nflplayer1.firstName + " " + item.nflplayer1.lastName;
                    player2.PlayerDescription = item.nflplayer1.position.positionName + " | " + item.nflplayer1.nflteam.teamSlug;
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
                        userMatchups.Add(GetWeeklyMatchup(item, false, false));
                    }

                    weekMatchups.Add(new MatchupByWeek { WeekNumber = i, Matchups = userMatchups });
                }
            }
            catch (Exception)
            {
            }

            return weekMatchups;
        }

        public static async Task<WeeklyMatchups> GetWeeklyMatchupAsync(matchup item, bool showAllVotes, bool getMessages, int? userID = null)
        {
            //first check the cache
            WeeklyMatchups weeklyMatchup = new WeeklyMatchups();

            CoachCueDataContext db = new CoachCueDataContext();
            int selectedUserID = (userID.HasValue) ? userID.Value : 0;
            bool isCompleted = (item.status.statusName == "Archive") ? true : false;

            IEnumerable<UserVoteData> usersVoted = new List<UserVoteData>();

            PlayerMatchup player1 = getPlayerMatchupInfo(item.gameschedule, item.nflplayer, item.player1Points, item.player2Points, isCompleted, getMessages, userID);
            PlayerMatchup player2 = getPlayerMatchupInfo(item.gameschedule1, item.nflplayer1, item.player1Points, item.player2Points, isCompleted, getMessages, userID);

            //get the users who have voted on this matchup              
            usersVoted = item.users_matchups.Select(um => um).Select(usrMatch => new UserVoteData
            {
                email = usrMatch.user.email,
                DateCreated = usrMatch.dateCreated,
                fullName = usrMatch.user.fullName,
                profileImg = "/assets/img/avatar/" + usrMatch.user.avatar.imageName,
                username = usrMatch.user.userName,
                userID = usrMatch.userID,
                correctPercentage = (usrMatch.user.CorrectPercentage != 0) ? usrMatch.user.CorrectPercentage + "%" : string.Empty,
                CorrectMatchup = (usrMatch.correctMatchup.HasValue) ? usrMatch.correctMatchup.Value : false,
                SelectedPlayerID = usrMatch.selectedPlayerID,
                SelectedPlayer = (userID.HasValue) ? usrMatch.nflplayer.fullName : usrMatch.nflplayer.firstName.Substring(0, 1) + ". " + usrMatch.nflplayer.lastName
            });

            player1.TotalVotes = usersVoted.Where(usrVote => usrVote.SelectedPlayerID == player1.PlayerID).Count();
            player2.TotalVotes = usersVoted.Where(usrVote => usrVote.SelectedPlayerID == player2.PlayerID).Count();

            int voteCount = usersVoted.Count();
            if (voteCount <= 0)
                usersVoted = new List<UserVoteData>();

            List<message> messages = await db.messages.Where(msg => msg.messageContextID == item.matchupID).ToListAsync();

            weeklyMatchup = new WeeklyMatchups
            {
                ShowFollow = (userID.HasValue) ? true : false,
                LastDate = (voteCount > 0) ? usersVoted.OrderByDescending(vt => vt.DateCreated).FirstOrDefault().DateCreated : item.dateCreated,
                WeekNumber = item.gameschedule.weekNumber,
                NoVotes = (voteCount > 0) ? false : true,
                TotalVotes = voteCount,
                //ShowCoaches = (showAllVotes) ? usersVoted.OrderByDescending(vt => vt.DateCreated).ToList() : usersVoted.OrderByDescending(vt => vt.DateCreated).Take(2).ToList(),
                // HideCoaches = (showAllVotes) ? new List<UserVoteData>() : usersVoted.OrderByDescending(vt => vt.DateCreated).Skip(2).ToList(),
                Coaches = usersVoted.OrderBy(vt => vt.DateCreated).ToList(),
                //InvitedCoaches = notification.GetInvitedToAnswer(item.matchupID),
                MatchupID = item.matchupID,
                Player1 = player1,
                Player2 = player2,
                Status = item.status.statusName,
                ScoringFormat = item.matchupscoringtype.scoringType,
                DateCreated = item.dateCreated,
                CreatedBy = item.user,
                AllowInvite = (item.user.userID == selectedUserID && item.gameschedule.gameDate > DateTime.UtcNow.GetEasternTime() && item.gameschedule1.gameDate > DateTime.UtcNow.GetEasternTime()) ? true : false,
                MessageCount = messages.Count(),
                Messages = messages,
                GameDate = (item.gameschedule.gameDate < item.gameschedule1.gameDate) ? item.gameschedule.gameDate : item.gameschedule1.gameDate
            };

            //see if the user has a vote
            if (userID.HasValue)
            {
                UserVoteData userVote = usersVoted.Where(uv => uv.userID == userID.Value).FirstOrDefault();
                if (userVote != null)
                    weeklyMatchup.UserSelectedPlayer = userVote.SelectedPlayer;
            }

            return weeklyMatchup;
        }


        public static WeeklyMatchups GetWeeklyMatchup(matchup item, bool showAllVotes, bool getMessages, int? userID = null)
        {
            //first check the cache
            WeeklyMatchups weeklyMatchup = new WeeklyMatchups();
            
            CoachCueDataContext db = new CoachCueDataContext();
            int selectedUserID = (userID.HasValue) ? userID.Value : 0;
            bool isCompleted = (item.status.statusName == "Archive") ? true : false;

            IEnumerable<UserVoteData> usersVoted = new List<UserVoteData>();

            PlayerMatchup player1 = getPlayerMatchupInfo(item.gameschedule, item.nflplayer, item.player1Points, item.player2Points, isCompleted, getMessages, userID);
            PlayerMatchup player2 = getPlayerMatchupInfo(item.gameschedule1, item.nflplayer1, item.player1Points, item.player2Points, isCompleted, getMessages, userID);

            //get the users who have voted on this matchup              
            usersVoted = item.users_matchups.Select(um => um).Select(usrMatch => new UserVoteData
            {
                email = usrMatch.user.email,
                DateCreated = usrMatch.dateCreated,
                fullName = usrMatch.user.fullName,
                profileImg = "/assets/img/avatar/" + usrMatch.user.avatar.imageName,
                username = usrMatch.user.userName,
                userID = usrMatch.userID,
                correctPercentage = (usrMatch.user.CorrectPercentage != 0) ? usrMatch.user.CorrectPercentage + "%" : string.Empty,
                CorrectMatchup = (usrMatch.correctMatchup.HasValue) ? usrMatch.correctMatchup.Value : false,
                SelectedPlayerID = usrMatch.selectedPlayerID,
                SelectedPlayer = (userID.HasValue) ? usrMatch.nflplayer.fullName : usrMatch.nflplayer.firstName.Substring(0, 1) + ". " + usrMatch.nflplayer.lastName
            });
               
            player1.TotalVotes = usersVoted.Where(usrVote => usrVote.SelectedPlayerID == player1.PlayerID).Count();
            player2.TotalVotes = usersVoted.Where(usrVote => usrVote.SelectedPlayerID == player2.PlayerID).Count();

            int voteCount = usersVoted.Count();
            if (voteCount <= 0)
                usersVoted = new List<UserVoteData>();

            List<message> messages = db.messages.Where(msg => msg.messageContextID == item.matchupID).ToList();

            weeklyMatchup = new WeeklyMatchups
            {
                ShowFollow = (userID.HasValue) ? true : false,
                LastDate = (voteCount > 0) ? usersVoted.OrderByDescending(vt => vt.DateCreated).FirstOrDefault().DateCreated : item.dateCreated,
                WeekNumber = item.gameschedule.weekNumber,
                NoVotes = (voteCount > 0) ? false : true,
                TotalVotes = voteCount,
                //ShowCoaches = (showAllVotes) ? usersVoted.OrderByDescending(vt => vt.DateCreated).ToList() : usersVoted.OrderByDescending(vt => vt.DateCreated).Take(2).ToList(),
               // HideCoaches = (showAllVotes) ? new List<UserVoteData>() : usersVoted.OrderByDescending(vt => vt.DateCreated).Skip(2).ToList(),
                Coaches = usersVoted.OrderBy(vt => vt.DateCreated).ToList(),
                //InvitedCoaches = notification.GetInvitedToAnswer(item.matchupID),
                MatchupID = item.matchupID,
                Player1 = player1,
                Player2 = player2,
                Status = item.status.statusName,
                ScoringFormat = item.matchupscoringtype.scoringType,
                DateCreated = item.dateCreated,
                CreatedBy = item.user,
                AllowInvite = (item.user.userID == selectedUserID && item.gameschedule.gameDate > DateTime.UtcNow.GetEasternTime() && item.gameschedule1.gameDate > DateTime.UtcNow.GetEasternTime()) ? true : false,
                MessageCount = messages.Count(),
                Messages = messages,
                GameDate = (item.gameschedule.gameDate < item.gameschedule1.gameDate) ? item.gameschedule.gameDate : item.gameschedule1.gameDate
            };

            //see if the user has a vote
            if (userID.HasValue)
            {
                UserVoteData userVote = usersVoted.Where(uv => uv.userID == userID.Value).FirstOrDefault();
                if (userVote != null)
                    weeklyMatchup.UserSelectedPlayer = userVote.SelectedPlayer;
            }

            return weeklyMatchup;
        }

        public static List<UserVoteData> GetMatchupVotes(int matchupID)
        {
            List<UserVoteData> voterInfo = new List<UserVoteData>();

            try
            {
                matchup item = matchup.Get(matchupID);

                //get the users who have voted on this matchup
                List<UserVoteData> usersVoted = item.users_matchups.Select(um => um).Select(usrMatch => new UserVoteData
                {
                    Verified = usrMatch.user.verified,
                    email = usrMatch.user.email,
                    DateCreated = usrMatch.dateCreated,
                    fullName = usrMatch.user.fullName,
                    profileImg = "/assets/img/avatar/" + usrMatch.user.avatar.imageName,
                    username = usrMatch.user.userName,
                    userID = usrMatch.userID,
                    correctPercentage = (usrMatch.user.CorrectPercentage != 0) ? usrMatch.user.CorrectPercentage + "%" : string.Empty,
                    CorrectMatchup = (usrMatch.correctMatchup.HasValue) ? usrMatch.correctMatchup.Value : false,
                    SelectedPlayerID = usrMatch.selectedPlayerID,
                    SelectedPlayer = usrMatch.nflplayer.firstName.Substring(0, 1) + ". " + usrMatch.nflplayer.lastName
                }).ToList();

                if (usersVoted.Count() > 0)
                    voterInfo = usersVoted;
            }
            catch (Exception) { }

            return voterInfo;
        }

        private static PlayerMatchup getPlayerMatchupInfo(gameschedule game, nflplayer player, decimal? player1Points, decimal? player2Points, bool isCompleted, bool getMessages, int? userID)
        {
            PlayerMatchup playerMatch = new PlayerMatchup();

            playerMatch.Image = player.profilePicXLarge;
            playerMatch.PlayerID = player.playerID;
            playerMatch.PositionID = player.positionID;

            playerMatch.DisplayPlayerName = (HttpContext.Current.Request.Browser.IsMobileDevice) ? player.firstName + "<br />" + player.lastName : player.firstName + " " + player.lastName;
            playerMatch.PlayerName = player.firstName + " " + player.lastName;
            playerMatch.PlayerDescription = player.position.positionName + " | " + player.nflteam.teamSlug;

            if (player.nflteam.Mascot == game.nflteam.Mascot)
                playerMatch.GameInfo = (game.weekNumber != 0) ? "@" + game.nflteam1.teamSlug + " " + game.gameDate.ToString("ddd") + " " + game.gameDate.ToShortTimeString() : "Draft";
            else
                playerMatch.GameInfo = (game.weekNumber != 0) ? game.nflteam.teamSlug + " " + game.gameDate.ToString("ddd") + " " + game.gameDate.ToShortTimeString() : "Draft";

            playerMatch.Href = "#register-modal";

            if (isCompleted && player1Points.HasValue && player2Points.HasValue)
            {
                playerMatch.CssClass = (player1Points.Value > player2Points.Value) ? "matchup-correct" : "matchup-wrong";
            }
            else
                playerMatch.CssClass = string.Empty;

            if (getMessages)
            {
                playerMatch.PlayerStream = stream.GetNewsStream(player, true, true, userID);
            }
            if (userID.HasValue)
            {
                if (userID.Value != 0)
                {
                    // player1.Following = (from usracnt in db.users_accounts
                    //                      where usracnt.userID == userID && usracnt.accountID == item.nflplayer.playerID
                    //                      select usracnt).Count();
                    playerMatch.Href = "#";
                    playerMatch.CssClass += (string.IsNullOrEmpty(playerMatch.CssClass)) ? "stream-select-starter" : " stream-select-starter";
                }
            }

            return playerMatch;
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
        public int MessageCount { get; set; }
        public List<message> Messages { get; set; }
        public string ScoringFormat { get; set; }
        public DateTime DateCreated { get; set; }
        public user CreatedBy { get; set; }
        public string UserSelectedPlayer { get; set; }
        public DateTime GameDate { get; set; }
        public bool ExistingMatchup { get; set; }
        public string Status { get; set; }
        public int TotalVotes { get; set; }
        public bool HideDetails { get; set; }
        public DateTime LastDate { get; set; }
        public bool DetailView { get; set; }
        public string PlayerIDs 
        {
            get
            {
                return this.Player1.PlayerID.ToString() + "," + this.Player2.PlayerID.ToString();
            }
        }
    }

    public class PlayerMatchup
    {
        public List<StreamContent> PlayerStream { get; set; }
        public string PlayerName { get; set; }
        public string DisplayPlayerName { get; set; }
        public int PlayerID { get; set; }
        public string Image { get; set; }
        public string PlayerDescription { get; set; }
        public string Href { get; set; }
        public string CssClass {get; set;}
        public string GameInfo { get; set; }
        public bool Selected { get; set; }
        public int TotalVotes { get; set; }
        public int Following { get; set; }
        public int PositionID { get; set;}
        public string linkFullName
        {
            get
            {
                string name = this.PlayerName.Replace(".", "").Replace("-", "").Replace("'", "");
                return name;
            }
        }
        public string matchupLink
        {
            get
            {
                string name = this.PlayerName.Replace(".", "").Replace("'", "").Replace(" ", "-");
                return name;
            }
        }
    }

    public class VotedPlayers
    {
        public nflplayer Player { get; set; }
        public int Votes { get; set; }
        public string Percent { get; set; }
    }

    public class VotedPlayersByPosition
    {
        public VotedPlayers CurrentPlayer { get; set; }
        public List<VotedPlayers> ComparedPlayers { get; set; }
    }
}