using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoachCue.Model
{
    public partial class users_view
    {
        public static users_view Add(string type, int entityID, int? userID )
        {
            CoachCueDataContext db = new CoachCueDataContext();

            users_view view = new users_view();

            try
            {
                int typeID = accounttype.GetID(type);
                view.dateViewed = DateTime.UtcNow.GetEasternTime();
                view.viewObjectID = entityID;
                view.accounttypeID = typeID;    
                if( userID.HasValue )
                    view.userID = userID;

                view.ipAddress = HttpContext.Current.Request.UserHostAddress;

                db.users_views.InsertOnSubmit(view);
                db.SubmitChanges();
            }
            catch (Exception ex) 
            { 
                string s = ex.Message; 
            }

            return view;
        }

        public static List<AccountData> GetRecent(int userID)
        {
            List<AccountData> recentList = new List<AccountData>();
            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                string cacheID = "viewed" + userID.ToString();
                if (HttpContext.Current.Cache[cacheID] != null)
                    recentList = (List<AccountData>)HttpContext.Current.Cache[cacheID];
                else
                {
                    var acnts = from views in db.users_views
                                where views.userID == userID
                                orderby views.dateViewed descending
                                select views;

                    if (acnts.Count() > 0)
                    {
                        var uniqueAcnts = acnts.GroupBy(x => x.viewObjectID).Select(x => x.First()).ToList();

                        List<int> followingPlayers = (from usracnt in db.users_accounts
                                                      where usracnt.userID == userID
                                                      select usracnt.accountID).ToList();

                        List<int> followingUsers = (from usrs in db.users
                                                    where usrs.status.statusName == "Active"
                                                    join usracnt in db.users_accounts on
                                                    usrs.userID equals usracnt.accountID
                                                    where usracnt.userID == userID
                                                    select usrs.userID).ToList();

                        foreach (users_view view in uniqueAcnts.Take(5).ToList())
                        {
                            if (view.accounttype.accountName == "players")
                            {
                                nflplayer player = nflplayer.Get(view.viewObjectID);
                                int followingPlayer = 0;
                                if (followingPlayers.Count() > 0)
                                    followingPlayer = (followingPlayers.Contains(player.playerID)) ? 1 : 0;

                                recentList.Add(new AccountData
                                {
                                    name = player.fullName,
                                    value = player.fullName,
                                    accountID = player.playerID,
                                    college = player.college,
                                    fullName = player.fullName,
                                    shortName = player.shortName,
                                    number = player.displayNumber,
                                    position = player.position.positionName,
                                    profileImg = player.profilePic,
                                    team = player.nflteam.teamName,
                                    username = player.twitterUsername,
                                    profileImgLarge = player.profilePicLarge,
                                    following = followingPlayer,
                                    teamSlug = player.nflteam.teamSlug,
                                    accountType = "players",
                                    link = "/Player/" + player.playerID.ToString() + "/" + player.linkFullName,
                                    currentUserID = userID
                                });
                            }
                            else
                            {
                                user usr = user.Get(view.viewObjectID);
                                int followingUser = 0;
                                if (followingUsers.Count() > 0)
                                    followingUser = (followingUsers.Contains(usr.userID)) ? 1 : 0;

                                recentList.Add(new AccountData
                                {
                                    username = usr.userName,
                                    fullName = usr.fullName,
                                    accountID = usr.userID,
                                    following = followingUser,
                                    profileImg = usr.avatar.imageName,
                                    accountType = "users",
                                    currentUserID = userID
                                });
                            }
                        }
                    }

                    HttpContext.Current.Cache.Insert(cacheID, recentList, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(400, 0, 0));
                }
            }
            catch (Exception)
            {
            }

            return recentList;
        }
    }
}