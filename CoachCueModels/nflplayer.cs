﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using CoachCue.Repository;
using CoachCue.Models;

namespace CoachCue.Model
{
    public partial class nflplayer
    {
        public string PositionCss
        {
            get
            {
                string cssClass = "other-position";

                if (this.position.positionName == "QB" || this.position.positionName == "TE" || this.position.positionName == "RB" || this.position.positionName == "WR" || this.position.positionName == "K" || this.position.positionName == "FB")
                    cssClass = "fantasy-position";

                return cssClass;
            }
        }

        public bool HasTwitter
        {
            get
            {
                bool hasTwitter = false;

                if (this.hasTwitterAccount.HasValue)
                {
                    if (this.hasTwitterAccount.Value == true)
                        hasTwitter = true;
                }

                return hasTwitter;
            }
        }

        public string twitterUsername
        {
            get { return (this.twitteraccount != null) ? this.twitteraccount.twitterUsername : string.Empty; }
        }

        public string twitterDisplayUsername
        {
            get { return (this.twitteraccount != null) ? "@" + this.twitteraccount.twitterUsername : string.Empty; }
        }

        public string twitterProfilePic
        {
            get { return (this.twitteraccount != null) ? this.twitteraccount.profileImageUrl : string.Empty; }
        }

        public string profilePic
        {
            get { return (this.twitteraccount != null) ? this.twitteraccount.profileImageUrl : "/assets/img/teams/" + this.nflteam.teamSlug + ".jpg"; }
        }

        public string profilePicXLarge
        {
            get { return (this.twitteraccount != null) ? this.twitteraccount.profileImageUrl.Replace("normal", "400x400") : "/assets/img/teams/" + this.nflteam.teamSlug + ".jpg"; }
        }

        public string profilePicLarge
        {
            get { return (this.twitteraccount != null) ? this.twitteraccount.profileImageUrl.Replace("normal", "bigger") : "/assets/img/teams/" + this.nflteam.teamSlug + ".jpg"; }
        }

        public string twitterDescription
        {
            get { return (this.twitteraccount != null) ? this.twitteraccount.description : string.Empty; }
        }

        public string displayNumber
        {
            get { return (this.number.HasValue) ? "#" + this.number.Value.ToString() : string.Empty; }
        }

        public string twitterLink
        {
            get { return (this.twitteraccount != null) ? "http://twitter.com/" + this.twitteraccount.twitterUsername : "#"; }
        }

        public string fullName
        {
            get { return this.firstName + " " + this.lastName; }
        }

        public string shortName
        {
            get { return this.firstName.Substring(0,1) + ". " + this.lastName; }
        }

        public string linkFullName
        {
            get 
            { 
                string name = this.fullName.Replace(".", "").Replace("-","").Replace("'","");
                return name; 
            }
        }

        public bool isFollowing
        {
            get
            {
                bool following = false;
                
                int userID = (HttpContext.Current.User.Identity.IsAuthenticated) ? user.GetUserID(HttpContext.Current.User.Identity.Name) : 0;
                if( userID != 0 )
                {
                    CoachCueDataContext db = new CoachCueDataContext();
                    int followCount = (from usracnt in db.users_accounts
                                            where usracnt.userID == userID && usracnt.accountID == this.playerID
                                            select usracnt).Count(); 
                    if( followCount > 0 )
                        following = true;
                }
                return following;
            }
        }

        public static nflplayer Get(int playerID)
        {
            nflplayer player = new nflplayer();
            try
            {
                string cacheID = "nflplayer" + playerID.ToString();

                if (HttpContext.Current.Cache[cacheID] != null)
                    player = (nflplayer)HttpContext.Current.Cache[cacheID];
                else
                {
                    CoachCueDataContext db = new CoachCueDataContext();
                    player = db.nflplayers.Where(ply => ply.playerID == playerID).FirstOrDefault();
                    HttpContext.Current.Cache.Insert(cacheID, player, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(800, 0, 0));
                }
            }
            catch (Exception) { }

            return player;
        }

        public static int FollowersCount(int playerID)
        {
            int count = 0;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var followers = from flws in db.users_accounts
                                where flws.accountID == playerID && flws.accounttype.accountName == "players"
                                select flws;

                count = followers.Count();
            }
            catch (Exception) { }

            return count;
        }

        public static string GetWhereClause()
        {
        string whereClause = "";

        return whereClause;
        }


        public static List<nflplayer> GetTrending(int count, string position =  "")
        {
            List<nflplayer> players = new List<nflplayer>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
            
                //cache results every five minutes
                string cacheID = "trendingPlayers" + count.ToString() + position;
                if (HttpContext.Current.Cache[cacheID] != null)
                    players = (List<nflplayer>)HttpContext.Current.Cache[cacheID];
                else
                {
                    //build trend list from most listed playerID in the messages, matchups and users_accounts tables
                    var messageTrends = (string.IsNullOrEmpty(position)) ? from n in db.messages
                                                                           join plyr in db.message_players on
                                                                                  n.messageID equals plyr.messageID
                                                                           where n.dateCreated > DateTime.UtcNow.GetEasternTime().AddDays(-5)
                                                                           group n by plyr.playerID into g
                                                                           orderby g.Count() descending
                                                                           select new Trending { playerID = g.Key, count = g.Count() } :
                                        from n in db.messages
                                        join plyr in db.message_players on
                                            n.messageID equals plyr.messageID
                                        where n.dateCreated > DateTime.UtcNow.GetEasternTime().AddDays(-5) &&
                                        plyr.nflplayer.position.positionName == position
                                        group n by plyr.playerID into g
                                        orderby g.Count() descending
                                        select new Trending { playerID = g.Key, count = g.Count() };                    
                    List<Trending> trendList = messageTrends.ToList();

                    var matchupPlayer1Trends = (string.IsNullOrEmpty(position)) ? from n in db.matchups
                                                                                  where n.dateCreated > DateTime.UtcNow.GetEasternTime().AddDays(-1)
                                                                                  group n by n.player1ID into g
                                                                                  orderby g.Count() descending
                                                                                  select new Trending { playerID = g.Key, count = g.Count() } :
                                               from n in db.matchups
                                               where n.dateCreated > DateTime.UtcNow.GetEasternTime().AddDays(-1) && n.nflplayer.position.positionName == position
                                               group n by n.player1ID into g
                                               orderby g.Count() descending
                                               select new Trending { playerID = g.Key, count = g.Count() };
                    List<Trending> trendPlayer1List = matchupPlayer1Trends.ToList();

                    var matchupPlayer2Trends = (string.IsNullOrEmpty(position)) ? from n in db.matchups
                                                                                  where n.dateCreated > DateTime.UtcNow.GetEasternTime().AddDays(-1)
                                                                                  group n by n.player2ID into g
                                                                                  orderby g.Count() descending
                                                                                  select new Trending { playerID = g.Key, count = g.Count() } :
                                               from n in db.matchups
                                               where n.dateCreated > DateTime.UtcNow.GetEasternTime().AddDays(-1) && n.nflplayer1.position.positionName == position
                                               group n by n.player2ID into g
                                               orderby g.Count() descending
                                               select new Trending { playerID = g.Key, count = g.Count() };
                    List<Trending> trendPlayer2List = matchupPlayer2Trends.ToList();

                    //combine all the data and sort to get the trend list
                    trendList.AddRange(trendPlayer1List);
                    trendList.AddRange(trendPlayer2List);
                    trendList = (from row in trendList
                                 group row by new { row.playerID } into grp
                                 select new Trending
                                 {
                                     playerID = grp.Key.playerID,
                                     count = grp.Sum(row => row.count)
                                 }).OrderByDescending(grp => grp.count).Take(count).ToList();


                    foreach (Trending trendItem in trendList)
                    {
                        players.Add(nflplayer.Get(trendItem.playerID));
                    }

                    HttpContext.Current.Cache.Insert(cacheID, players, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(2, 0, 0));
                }
            }
            catch (Exception)
            { }
            
            return players;
        }

        public static List<nflplayer> ListByTeam(string teamSlug)
        {
            List<nflplayer> players = new List<nflplayer>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var plyrs = from plys in db.nflplayers
                            where plys.status.statusName == "Active"
                            && plys.nflteam.teamSlug.ToLower() == teamSlug.ToLower()
                            select plys;

                if (plyrs.Count() > 0)
                    players = plyrs.OrderBy(ply => ply.lastName).ToList();
            }
            catch (Exception) { }

            return players;
        }

        public static List<PlayersByPostion> ListFantasyOffense()
        {
            List<PlayersByPostion> posPlayers = new List<PlayersByPostion>();
            List<nflplayer> players = new List<nflplayer>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var plyrs = from plys in db.nflplayers
                            where plys.status.statusName == "Active"
                            && plys.position.positiontype.positionTypeName == "Offense" &&
                            (plys.position.positionName == "QB" || plys.position.positionName == "TE" || plys.position.positionName == "RB"
                            || plys.position.positionName == "WR" || plys.position.positionName == "K" || plys.position.positionName == "FB")
                            select plys;

                if (plyrs.Count() > 0)
                {
                    players = plyrs.OrderBy(ply => ply.lastName).ToList();

                    posPlayers.Add(new PlayersByPostion { Position = "QB", Players = players.Where(pos => pos.position.positionName == "QB").ToList() });
                    posPlayers.Add(new PlayersByPostion { Position = "TE", Players = players.Where(pos => pos.position.positionName == "TE").ToList() });
                    posPlayers.Add(new PlayersByPostion { Position = "RB", Players = players.Where(pos => pos.position.positionName == "RB").ToList() });
                    posPlayers.Add(new PlayersByPostion { Position = "WR", Players = players.Where(pos => pos.position.positionName == "WR").ToList() });
                    posPlayers.Add(new PlayersByPostion { Position = "Kicker", Players = players.Where(pos => pos.position.positionName == "K").ToList() });
                    posPlayers.Add(new PlayersByPostion { Position = "FB", Players = players.Where(pos => pos.position.positionName == "FB").ToList() });
                }
            }
            catch (Exception) { }

            return posPlayers;
        }

        public static void Delete(int playerID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var ret = from mt in db.nflplayers
                          where mt.playerID == playerID
                          select mt;

                nflplayer accnt = ret.FirstOrDefault();
                accnt.statusID = status.GetID("Deleted", "nflplayers");

                db.SubmitChanges();
            }
            catch (Exception) { }
        }

        public static int DeleteTwitterAccount(int playerID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            int accountID = 0;
            try
            {
                var ret = from mt in db.nflplayers
                          where mt.playerID == playerID
                          select mt;

                nflplayer accnt = ret.FirstOrDefault();
                accountID = accnt.twitterAccountID.Value;

                accnt.twitterAccountID = null;
                accnt.hasTwitterAccount = false;

                db.SubmitChanges();
            }
            catch (Exception) { }

            return accountID;
        }

        public static List<string> GetPlayerNotConditions(string firstName, string lastName)
        {
            List<string> names = new List<string>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                //find all players with the same lastname but different last names and players with the first name of last name of the player
                var fn = from players in db.nflplayers
                         where (players.lastName == lastName && players.firstName != firstName) || players.firstName == lastName
                         select players;

                if (fn.Count() > 0)
                {
                    foreach (nflplayer player in fn.ToList())
                    {
                        if (!names.Contains(player.lastName) && !names.Contains(player.firstName))
                            names.Add((player.firstName == lastName) ? player.lastName : player.firstName);

                        //also add the team city and mascot
                        //names.Add(player.nflteam.Mascot);
                        //names.Add(player.nflteam.ShortCity);
                    }
                }
            }
            catch (Exception) { }

            return names;
        }

        public static nflplayer FindUpdatePlayer(string firstName, string lastName, string position)
        {
            nflplayer player = null;
            CoachCueDataContext db = new CoachCueDataContext();

            //check by first name, last name and position
            var playerSearch = db.nflplayers.Where(ply => ply.lastName.ToLower() == lastName.ToLower()
                && ply.firstName.ToLower() == firstName.ToLower()
                && ply.position.positionName.ToLower() == position.ToLower());

            if (playerSearch.Count() > 0)
                player = playerSearch.FirstOrDefault();
            
            return player;
        }

        public static List<nflplayer> GetRoster(int teamID, bool offenseOnly)
        {
            List<nflplayer> players = new List<nflplayer>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var playerList = db.nflplayers.Where(ply => ply.teamID == teamID && ply.status.statusName == "Active");
                if (offenseOnly)
                    playerList = playerList.Where(plyrs => plyrs.position.positionName == "QB" || plyrs.position.positionName == "TE" || plyrs.position.positionName == "RB" || plyrs.position.positionName == "WR" || plyrs.position.positionName == "K" || plyrs.position.positionName == "FB");

                players = playerList.ToList();
            }
            catch (Exception) { }

            return players;
        }

        public static void UpdateTwitterAccount(int playerID, int twitterAccountID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            //first see if we need to update or add
            nflplayer player = db.nflplayers.Where(ply => ply.playerID == playerID).FirstOrDefault();
            if (player != null)
            {
                if (twitterAccountID != 0)
                {
                    player.twitterAccountID = twitterAccountID;
                    player.hasTwitterAccount = true;
                }
                else
                    player.hasTwitterAccount = false;
            }

            db.SubmitChanges();
        }

        public static void RemoveTwitterAccount(int playerID, int twitterAccountID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            //first see if we need to update or add
            nflplayer player = db.nflplayers.Where(ply => ply.playerID == playerID).FirstOrDefault();
            if (player != null)
            {
                player.twitterAccountID = null;
                player.hasTwitterAccount = false;
            }

            db.SubmitChanges();
        }

        public static List<AccountData> Search(string searchTerm, int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<AccountData> accounts = new List<AccountData>();

            // List<string> searchTerms = searchTerm.Split(' ').ToList();
            string searchString = searchTerm.Replace(" ", "").ToLower();


            var acnts = from plyrs in db.nflplayers
                        where plyrs.status.statusName == "Active" &&
                        (searchString.IndexOf(plyrs.firstName.ToLower()) != -1 || searchString.IndexOf(plyrs.lastName.ToLower()) != -1) &&
                        (plyrs.position.positionName == "QB" || plyrs.position.positionName == "TE" || plyrs.position.positionName == "RB" || plyrs.position.positionName == "WR" || plyrs.position.positionName == "K" || plyrs.position.positionName == "FB")
                        select new AccountData
                        {
                            team = plyrs.nflteam.teamName,
                            position = plyrs.position.positionName,
                            profileImg = (plyrs.twitteraccount != null) ? plyrs.twitteraccount.profileImageUrl : "/assets/img/teams/" + plyrs.nflteam.teamSlug + ".jpg",
                            username = (plyrs.twitteraccount != null) ? "@" + plyrs.twitteraccount.twitterUsername : string.Empty,
                            user = (plyrs.twitteraccount != null) ? plyrs.twitteraccount.twitterName : string.Empty,
                            twitterLink = (plyrs.twitteraccount != null) ? "http://twitter.com/" + plyrs.twitteraccount.twitterUsername : "#",
                            accountID = plyrs.playerID,
                            fullName = plyrs.firstName + " " + plyrs.lastName,
                            following = (userID != 0) ? (from usracnt in db.users_accounts
                                                         where usracnt.userID == userID && usracnt.accountID == plyrs.playerID
                                                         select usracnt).Count() : 0
                        };

            if (acnts.Count() > 0)
                accounts = acnts.ToList();

            return accounts;
        }

        public static List<AccountData> TypeAheadSearch(string searchTerm, int? userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<AccountData> accounts = new List<AccountData>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                string searchString = searchTerm.ToLower();

                //string cacheID = "searchPlayers" + searchString;
                // if (HttpContext.Current.Cache[cacheID] != null)
                //     accounts = (List<AccountData>)HttpContext.Current.Cache[cacheID];
                // else
                // {
                var acnts = from plyrs in db.nflplayers
                            where plyrs.status.statusName == "Active" &&
                            (plyrs.firstName + " " + plyrs.lastName).ToLower().Contains(searchString) &&
                            (plyrs.position.positionName == "QB" || plyrs.position.positionName == "TE" || plyrs.position.positionName == "RB" || plyrs.position.positionName == "WR" || plyrs.position.positionName == "K" || plyrs.position.positionName == "FB")
                            select plyrs;

                if (acnts.Count() > 0)
                {
                    accounts = user.GetAccountsFromPlayers(acnts.Take(10).ToList(), (userID != 0) ? (int?)userID : null);
                    //HttpContext.Current.Cache.Insert(cacheID, accounts, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(10, 0, 0));
                }
                // }
            }

            return accounts;
        }

        public static List<AccountData> GetAllAccounts()
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<AccountData> accounts = new List<AccountData>();

            try
            {
                var acnts = from plyrs in db.nflplayers
                            where plyrs.status.statusName == "Active" &&
                            (plyrs.position.positionName == "DEF" || plyrs.position.positionName == "QB" || plyrs.position.positionName == "TE" || plyrs.position.positionName == "RB" || plyrs.position.positionName == "WR" || plyrs.position.positionName == "K" || plyrs.position.positionName == "FB")
                            select plyrs;

                accounts = user.GetAccountsFromPlayers( acnts.ToList(), null);
            }
            catch (Exception) { }

            return accounts;
        }

        public static List<nflplayer> Search(string searchTerm, bool offenseOnly = true)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<nflplayer> playersFound = new List<nflplayer>();

            var players = db.nflplayers.Where(ply => ply.firstName.Contains(searchTerm) || ply.lastName.Contains(searchTerm));
            if (offenseOnly)
            {
                players = players.Where(ply =>
                    (ply.position.positionName == "QB" || ply.position.positionName == "TE" || ply.position.positionName == "RB" || ply.position.positionName == "WR" || ply.position.positionName == "K" || ply.position.positionName == "FB"));
            }

            List<nflplayer> search = players.ToList();
            if (search.Count > 0)
                playersFound = search;

            return playersFound;
        }


    }

    public class Trending
    {
        public int playerID { get; set; }
        public int count { get; set; }
    }

    public class PlayersByPostion
    {
        public string Position { get; set; }
        public List<nflplayer> Players { get; set; }
    }

    public class TrendingPlayers
    {
        public DateTime LastUpdate { get; set; }
        public List<nflplayer> Players { get; set; }
    }
}
