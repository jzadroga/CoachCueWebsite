using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoachCueModels;
using HtmlAgilityPack;
using LinqToTwitter;

namespace CoachCue.Model
{
    public partial class twitteraccount
    {
        public static int GetID(string positionName)
        {
            int positionID = 0;

            CoachCueDataContext db = new CoachCueDataContext();
            var ps = db.positions.Where(pos => pos.positionName.ToLower() == positionName.ToLower());

            if (ps.Count() > 0)
                positionID = ps.FirstOrDefault().positionID;

            return positionID;
        }

        public static twitteraccount Save(int? accountID, string twitterID, string twitterUsername, string twitterName, string profileImageUrl, int accountType, string statusType, string description)
        {
            twitteraccount twtAcnt = null;
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                //first see if we need to update or add
                if (accountID.HasValue)
                {
                    if( accountID != 0 )
                        twtAcnt = db.twitteraccounts.Where(twt => twt.twitterAccountID == accountID).FirstOrDefault();
                }

                if (twtAcnt == null)
                {
                    twtAcnt = new twitteraccount();
                    twtAcnt.twitterID = twitterID;
                    twtAcnt.twitterUsername = twitterUsername;
                    twtAcnt.twitterName = twitterName;
                    twtAcnt.twitterAccountTypeID = accountType;
                    twtAcnt.statusID = status.GetID(statusType, "twitteraccounts");
                    twtAcnt.profileImageUrl = profileImageUrl;
                    twtAcnt.description = description;
                    twtAcnt.dateModified = DateTime.UtcNow.GetEasternTime();

                    db.twitteraccounts.InsertOnSubmit(twtAcnt);
                }
                else
                {
                    twtAcnt.twitterID = twitterID;
                    twtAcnt.twitterUsername = twitterUsername;
                    twtAcnt.twitterName = twitterName;
                    twtAcnt.profileImageUrl = profileImageUrl;
                    twtAcnt.description = description;
                    twtAcnt.dateModified = DateTime.UtcNow.GetEasternTime();
                }

                db.SubmitChanges();
            }
            catch (Exception) { }

            return twtAcnt;
        }

        //gets all possible Twitter account matches
        public static List<twitteraccount> GetTwitterAccounts( int playerID, TwitterContext twitterCtx )
        {
            nflplayer playerAccount = nflplayer.Get(playerID);
            List<twitteraccount> accounts = new List<twitteraccount>();

            try
            {
                List<User> twitterUsers = new List<User>();

               /* if (playerAccount.twitterAccountID.HasValue)
                {
                    //ulong accountID = Convert.ToUInt64(twitteraccount.Get(playerAccount.twitterAccountID.Value).twitterID);
                    twitteraccount acnt = twitteraccount.Get(playerAccount.twitterAccountID.Value);

                    twitterUsers = (from usr in twitterCtx.User
                                    where usr.Type == UserType.Lookup &&
                                    usr.ScreenNameList == acnt.twitterUsername
                                    select usr).ToList();
                }
                else
                {*/
                    //see if the player has a twitter account, find the player account on twitter
                    string playerName = playerAccount.firstName + " " + playerAccount.lastName;
                    twitterUsers = (from usr in twitterCtx.User
                                    where usr.Type == UserType.Search &&
                                    usr.Query == playerName && usr.Verified == true
                                    select usr).ToList();
                //}

                if (twitterUsers.Count() > 0)
                {
                    foreach (var user in twitterUsers)
                    {
                        accounts.Add(new twitteraccount
                        {
                            twitterAccountID = (playerAccount.twitterAccountID.HasValue) ? (int)playerAccount.twitterAccountID : 0,
                            twitterID = user.UserID.ToString(),
                            twitterUsername = user.ScreenName,
                            twitterName = user.Name,
                            profileImageUrl = user.ProfileImageUrl,
                            description = user.Description
                        });
                    }
                }
            }
            catch( Exception ex )
            {
                string err = ex.Message;
            }

            return accounts;
        }

        public static twitteraccount Get(int twitterAccountID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            twitteraccount account = new twitteraccount();
            try
            {
                var ret = from mt in db.twitteraccounts
                          where mt.twitterAccountID == twitterAccountID
                          select mt;

                twitteraccount accnt = ret.FirstOrDefault();
                if (accnt != null)
                    account = accnt;
            }
            catch (Exception) { }

            return account;
        }

        public static void Delete(int twitterAccountID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                var ret = from mt in db.twitteraccounts
                          where mt.twitterAccountID == twitterAccountID
                          select mt;

                twitteraccount accnt = ret.FirstOrDefault();
                if (accnt != null)
                {
                    db.twitteraccounts.DeleteOnSubmit(accnt);
                    db.SubmitChanges();
                }
            }
            catch (Exception) { }
        }

        public static List<twitteraccount> ListJournalistsByTeam(int teamID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<twitteraccount> accounts = new List<twitteraccount>();

            var acnts = from twt in db.twitteraccounts
                        join twtacnts in db.nflteams_twitteraccounts on
                        twt.twitterAccountID equals twtacnts.twitterAccountID
                        where twt.status.statusName == "Active"
                        && twtacnts.teamID == teamID && twt.twitteraccounttype.accountType == "Journalist"
                        select twt;

            if (acnts.Count() > 0)
                accounts = acnts.ToList();

            return accounts;
        }

        public static List<twitteraccount> ListByTeam(int teamID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<twitteraccount> accounts = new List<twitteraccount>();

            var acnts = from twt in db.twitteraccounts
                        join twtacnts in db.nflteams_twitteraccounts on
                        twt.twitterAccountID equals twtacnts.twitterAccountID
                        where twt.status.statusName == "Active"
                        && twtacnts.teamID == teamID
                        select twt;

            if (acnts.Count() > 0)
                accounts = acnts.ToList();

            return accounts;
        }

        public static void BuildPlayerTwitterAccount(nflplayer playerAccount, TwitterContext twitterCtx)
        {
            try
            {
                if (playerAccount.twitterAccountID.HasValue)
                {
                    //update twitter account
                    var plyAcnt = from usr in twitterCtx.User
                                    where usr.ScreenName == playerAccount.twitteraccount.twitterUsername && usr.Type == UserType.Lookup
                                    select usr;

                    if (plyAcnt != null)
                    {
                        LinqToTwitter.User twitAcnt = plyAcnt.FirstOrDefault();
                        twitteraccount.Save( playerAccount.twitterAccountID, playerAccount.twitteraccount.twitterID, twitAcnt.ScreenName, twitAcnt.Name, twitAcnt.ProfileImageUrl, 1, "Active", twitAcnt.Description);
                    }
                }
                else
                {
                    //see if the player has a twitter account, find the player account on twitter
                    string playerName = playerAccount.firstName + " " + playerAccount.lastName;
                    var users = (from usr in twitterCtx.User
                                where usr.Type == UserType.Search &&
                                usr.Query == playerName && usr.Verified == true
                                select usr).ToList();

                    if (users.Count() > 0)
                    {
                        foreach (var user in users.ToList())
                        {
                            //double check the last name before adding the twitter account
                            string[] userName = user.Name.Split(' ');
                            if (userName.Count() > 1)
                            {
                                if( userName[1].ToLower() == playerAccount.lastName.ToLower() )    
                                {
                                    twitteraccount twtAcnt = twitteraccount.Save(null, user.UserID.ToString(), user.ScreenName, user.Name, user.ProfileImageUrl, 1, "Active", user.Description);
                                    nflplayer.UpdateTwitterAccount(playerAccount.playerID, twtAcnt.twitterAccountID);
                                    break; //only add the first one found in case there are more with the same last name
                                }
                            }
                        }
                    }
                    else
                        //set the flag so we don't keep looking at this player
                        nflplayer.UpdateTwitterAccount(playerAccount.playerID, 0);
                }
            }
            catch (Exception) { }
        }

        public static List<TwitterUser> GetUserNamesByTeam(int teamID)
        {
            //need to cache these values
            CoachCueDataContext db = new CoachCueDataContext();
            List<TwitterUser> accounts = new List<TwitterUser>();

            var usernames = from twt in db.nflteams_twitteraccounts
                            where twt.teamID == teamID || twt.nflteam.teamName == "All"
                            join acnts in db.twitteraccounts on
                            twt.twitterAccountID equals acnts.twitterAccountID
                            where acnts.status.statusName == "Active"
                            select new TwitterUser { userName = acnts.twitterUsername };

            if (usernames.Count() > 0)
                accounts = usernames.ToList();

            return accounts;
        }
    }

    public class TwitterUser
    {
        public string userName { get; set; }
    }
}
