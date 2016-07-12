using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Security;

namespace CoachCue.Model
{
    public partial class user
    {
        public int NotificationCount
        {
            get
            {
                int count = 0;

                if( this.notifications1 != null ) {
                    count = this.notifications1.Where(not => not.status.statusName == "Active").Count();
                }

                return count;
            }
        }

        public int MessageCount
        {
            get
            {
                int count = 0;

                if (this.messages != null)
                {
                    count = this.messages.Count();
                }

                return count;
            }
        }

        public int FollowingCount
        {
            get
            {
                int count = 0;

                if (this.users_accounts != null)
                {
                    count = this.users_accounts.Where( usracnt => usracnt.userID == this.userID).Count();
                }

                return count;
            }
        }

        public int TotalMatchupVotes
        {
            get
            {
                int count = 0;

                if (this.users_matchups != null)
                {
                    count = this.users_matchups.Count();
                }

                return count;
            }
        }

        public int TotalCorrectVotes
        {
            get
            {
                int count = 0;

                if (this.users_matchups != null)
                {
                    count = this.users_matchups.Where(um => um.correctMatchup == true).Count();
                }

                return count;
            }
        }

        public int TotalInCorrectVotes
        {
            get
            {
                int count = 0;

                if (this.users_matchups != null)
                {
                    count = this.users_matchups.Where(um => um.correctMatchup == false).Count();
                }

                return count;
            }
        }

        public int MatchupCreatedCount
        {
            get
            {
                int count = 0;

                if (this.matchups != null)
                {
                    count = this.matchups.Where(mtch => mtch.status.statusName == "Active" || mtch.status.statusName == "Archive" ).Count();
                }

                return count;
            }
        }

        public string DisplayUserName
        {
            get { return "@" + this.userName; }
        }

        public string DisplayPercentage
        {
            get
            {
                string displayPercent = string.Empty;
                int percent = this.CorrectPercentage;
                if (percent != 0)
                    displayPercent = percent.ToString() + "%";
                return displayPercent;
            }
        }

        //calculates the correct number of matchups selected
        public int CorrectPercentage
        {
            get
            {
                int percent = 0;

                if (this.users_matchups != null)
                {
                    int allGuesses = this.users_matchups.Where(um => um.correctMatchup == true || um.correctMatchup == false).Count();
                    if (allGuesses == 0)
                        percent = 0;
                    else
                    {
                        int correct = this.users_matchups.Where(um => um.correctMatchup == true).Count();
                        percent = correct * 100 / allGuesses;
                    }
                }

                return percent;
            }
        }

        public int? CorrectWeeklyPercentage
        {
            get
            {
                int? percent = null;

                if (this.users_matchups != null)
                {
                    gameschedule currentWeek = gameschedule.GetCurrentWeek();
                    var weeklyMatchups = this.users_matchups.Where(us => us.matchup.gameschedule1.weekNumber == (currentWeek.weekNumber - 1));

                    if (weeklyMatchups != null)
                    {
                        int allGuesses = weeklyMatchups.Count();
                        if (allGuesses != 0)
                        {
                            int correct = weeklyMatchups.Where(um => um.correctMatchup == true).Count();
                            percent = correct * 100 / allGuesses;
                        }
                    }
                }
               
                return percent;
            }
        }

        public int LoginCount 
        {
            get
            {
                int count = 0;
                if (this.user_logins != null) {
                    count = this.user_logins.Count();
                }

                return count;
            }
        }

        public DateTime? LastLoginDate
        {
            get
            {
                DateTime? last = null;
                if (this.user_logins != null)
                {
                    if (this.user_logins.Count() > 0)
                    {
                        last = this.user_logins.OrderByDescending(ul => ul.loginDate).FirstOrDefault().loginDate;
                    }
                }
                return last;
            }
        }

        public string LastLogin
        {
            get
            {
                string last = string.Empty;
                if (this.user_logins != null)
                {
                    if (this.user_logins.Count() > 0)
                    {
                        DateTime loginDate = this.user_logins.OrderByDescending(ul => ul.loginDate).FirstOrDefault().loginDate;
                        last = loginDate.ToShortDateString() + " " + loginDate.ToShortTimeString();
                    }
                }
                return last;
            }
        }

        public string DateJoined
        {
            get
            {
                string created = string.Empty;
                DateTime eastTime = this.dateCreated.Value;

                created = eastTime.ToString("MMMM d, yyyy");
                return created;
            }
        }

        public string DateRegistered
        {
            get
            {
                string created = string.Empty;
                DateTime eastTime = this.dateCreated.Value;

                created = eastTime.ToShortDateString() + " " + eastTime.ToShortTimeString();
                return created;
            }
        }

        public bool isFollowing
        {
            get
            {
                bool following = false;

                int userID = (HttpContext.Current.User.Identity.IsAuthenticated) ? user.GetUserID(HttpContext.Current.User.Identity.Name) : 0;
                if (userID != 0)
                {
                    CoachCueDataContext db = new CoachCueDataContext();
                    int followCount = (from usracnt in db.users_accounts
                                       where usracnt.userID == userID && usracnt.accountID == this.userID
                                       select usracnt).Count();
                    if (followCount > 0)
                        following = true;
                }
                return following;
            }
        }

        public static List<user> List()
        {
            List<user> userList = new List<user>();
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var usrs = db.users.Where(us => us.status.statusName == "Active").ToList();

                if (usrs.Count() > 0)
                    userList = usrs;
            }
            catch (Exception) { }

            return userList;
        }

        public static int UserPageCount()
        {
            int pageSize = 100;
            CoachCueDataContext db = new CoachCueDataContext();
            int totalCount = db.users.Where(us => us.status.statusName != "Deleted").Count();

            return (int)(totalCount/pageSize);
        }

        public static int GetCount()
        {
            CoachCueDataContext db = new CoachCueDataContext();
            int totalCount = db.users.Where(us => us.status.statusName != "Deleted").Count();

            return totalCount;
        }


        public static List<user> ListByPage(int pageIndex, string sort)
        {
            List<user> userList = new List<user>();
            try
            {
                int pageSize = 100;
                CoachCueDataContext db = new CoachCueDataContext();

                var usrs = db.users.Where(us => us.status.statusName != "Deleted").OrderBy( usr => usr.email).ToList();
                if( !string.IsNullOrEmpty( sort ))
                {
                    switch (sort)
                    {
                        case "logins":
                            usrs = usrs.OrderByDescending(usr => usr.LoginCount).ToList();
                            break;
                        case "last":
                            usrs = usrs.OrderByDescending(usr => usr.LastLoginDate).ToList();
                            break;
                        case "created":
                            usrs = usrs.OrderByDescending(usr => usr.dateCreated).ToList();
                            break;
                    }
                }

                if (usrs.Count() > 0)
                    userList = usrs.Skip((pageIndex) * pageSize).Take(pageSize).ToList();
            }
            catch (Exception) { }

            return userList;
        }

        public static user Get(int userID)
        {
            user account = new user();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.userID == userID && mt.status.statusName == "Active"
                          select mt;

                user userItem = ret.FirstOrDefault();
                if (userItem != null)
                    account = userItem;
            }
            catch (Exception) { }

            return account;
        }

        public static int GetFollowersCount(int userID)
        {
            int count = 0;

            try
            {
                string cacheID = "followerCount" + userID.ToString();
                if (HttpContext.Current.Cache[cacheID] != null)
                    count = (int)HttpContext.Current.Cache[cacheID];
                else
                {

                    CoachCueDataContext db = new CoachCueDataContext();

                    var followers = from flws in db.users_accounts
                                    where flws.accountID == userID && flws.accounttype.accountName == "users"
                                    select flws;

                    count = followers.Count();
                    HttpContext.Current.Cache.Insert(cacheID, count, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(80, 0, 0));
                }
            }
            catch (Exception) { }

            return count;
        }

        public static List<AccountData> GetAccountsFromPlayers(List<nflplayer> players, int? userID)
        {
            List<AccountData> playerAccounts = new List<AccountData>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                bool checkUser = false;
                int followingPlayer = 0;
                List<int> followingPlayers = new List<int>();

                //check to see if we need to figure out if the user is following the player
                if( userID.HasValue )
                {
                    if( userID.Value != 0 )
                    {
                        checkUser = true;
                        followingPlayers = (from usracnt in db.users_accounts
                                               where usracnt.userID == userID && usracnt.accounttype.accountName == "players"
                                               select usracnt.accountID).ToList();
                    }
                }

                foreach (nflplayer player in players)
                {
                    if( checkUser && followingPlayers.Count() > 0 )
                        followingPlayer = (followingPlayers.Contains(player.playerID)) ? 1 : 0;

                    playerAccounts.Add(new AccountData
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
                         following = (checkUser) ? followingPlayer : 0,
                         teamSlug = player.nflteam.teamSlug,
                         accountType = "players",
                         link = "/Player/" + player.playerID.ToString() + "/" + player.linkFullName
                    });
                }
            }
            catch (Exception) { }

            return playerAccounts;
        }

        public static users_setting GetSettings(int userID)
        {
            users_setting settings = new users_setting();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users_settings
                          where mt.userID == userID
                          select mt;

                users_setting userItem = ret.FirstOrDefault();
                if (userItem != null)
                    settings = userItem;
            }
            catch (Exception) { }

            return settings;
        }

        public static user GetByEmail(string email)
        {
            user account = new user();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.email.ToLower() == email.ToLower() && mt.status.statusName == "Active"
                          select mt;

                user userItem = ret.FirstOrDefault();
                if (userItem != null)
                    account = userItem;
            }
            catch (Exception) { }

            return account;
        }

        public static user GetByAccountUsername(string username)
        {
            user account = new user();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.userName.ToLower() == username.ToLower() && mt.status.statusName == "Active"
                          select mt;

                user userItem = ret.FirstOrDefault();
                if (userItem != null)
                    account = userItem;
            }
            catch (Exception) { }

            return account;
        }

        public static user GetByGuid(string guid)
        {
            user account = new user();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.userGuid == guid && mt.status.statusName == "Active"
                          select mt;

                user userItem = ret.FirstOrDefault();
                if (userItem != null)
                    account = userItem;
            }
            catch (Exception) { }

            return account;
        }

        public static user NotificationLogin(string guid)
        {
            int toUser = notification.GetByGuid(guid).sentTo;
            return user.Get(toUser);
        }

        public static bool EmailExists(string email)
        {
            bool exists = true;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.email.ToLower() == email.ToLower() && mt.status.statusName == "Active"
                          select mt;

                user userItem = ret.FirstOrDefault();

                if (userItem == null)
                    exists = false;
                else
                {
                    if (userItem.userID == 0)
                        exists = false;
                }
            }
            catch (Exception) { }

            return exists;
        }

        public static user Create(string name, string email, string password, string openID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            user account = new user();

            try
            {
                avatar newAvatar = CreateAvatar();

                account.email = email;
                account.fullName = name;
                account.userName = email.Substring(0, email.IndexOf('@'));
                account.password = (string.IsNullOrEmpty(password)) ? Guid.NewGuid().ToString().Take(10).ToString() : password;
                account.avatarID = newAvatar.avatarID;
                account.statusID = status.GetID("Active", "users");
                account.dateCreated = DateTime.UtcNow.GetEasternTime();
                account.userGuid = Guid.NewGuid().ToString();
                account.openID = openID;
                account.isAdmin = false;

                if( HttpContext.Current.Session["media"] != null )
                {
                    if (!string.IsNullOrEmpty(HttpContext.Current.Session["media"].ToString()))
                        account.referrer = HttpContext.Current.Session["media"].ToString();
                }

                db.users.InsertOnSubmit(account);
                db.SubmitChanges();

                //now add the settings
                users_setting usrset = new users_setting();
                usrset.userID = account.userID;
                usrset.emailNotifications = true;
                db.users_settings.InsertOnSubmit(usrset);
                db.SubmitChanges();

                //finally have user follow coachcue default account & mine
                user.Follow(account.userID, 43, "users");
                user.Follow(account.userID, 1, "users");
                user.Follow(account.userID, 12488, "users");
                user.Follow(account.userID, 12495, "users");
            }
            catch (Exception) { }

            return account;
        }

        public static avatar CreateAvatar()
        {
            avatar acntAvatar = new avatar();

            CoachCueDataContext db = new CoachCueDataContext();

            try
            {
                acntAvatar.imageName = "profile.jpg";
                acntAvatar.statusID = status.GetID("Active", "avatars");
                db.avatars.InsertOnSubmit(acntAvatar);

                db.SubmitChanges();
            }
            catch (Exception) { }

            return acntAvatar;
        }

        public static string UpdateAvatar(int userID, HttpPostedFileBase avatarFile)
        {
            string fileName = string.Empty;
            try
            {
                if (avatarFile.ContentLength > 716800)
                    return fileName;

                //if( avatarFile.ContentType

                CoachCueDataContext db = new CoachCueDataContext();

                fileName = userID.ToString() + "_" + avatarFile.FileName.Substring(avatarFile.FileName.LastIndexOf("\\") + 1);
                avatarFile.SaveAs( HttpContext.Current.Request.PhysicalApplicationPath + "assets\\img\\avatar\\" + fileName);
            
                user userItem = db.users.Where(usr => usr.userID == userID).FirstOrDefault();
                if (userItem != null)
                {
                    avatar updatedAvatar = avatar.Save(new avatar { avatarID = userItem.avatarID, imageName = fileName });
                    userItem.avatarID = updatedAvatar.avatarID;
                    db.SubmitChanges();
                    HttpContext.Current.Session["CurrentUser"] = null;
                }
            }
            catch (Exception)
            {
                fileName = string.Empty;
            }

            return fileName;
        }

        public static bool UpdatePassword(int userID, string password)
        {
            bool saved = false;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                user userItem = db.users.Where(usr => usr.userID == userID).FirstOrDefault();
                if (userItem != null)
                {
                    userItem.password = password;

                    db.SubmitChanges();
                    HttpContext.Current.Session["CurrentUser"] = null;
                    saved = true;
                }
            }
            catch (Exception) 
            {
                saved = false;
            }

            return saved;
        }

        public static bool UpdateEmailSettings(int userID, string recieveNotification)
        {
            bool saved = false;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                users_setting userItem = db.users_settings.Where(usr => usr.userID == userID).FirstOrDefault();
                if (userItem != null)
                {
                    userItem.emailNotifications = ( recieveNotification == "1" ) ? true : false;

                    db.SubmitChanges();
                    HttpContext.Current.Session["CurrentUser"] = null;
                    saved = true;
                }
            }
            catch (Exception) 
            {
                saved = false;
            }

            return saved;
        }

        //updates the basic account information
        public static bool UpdateProfile(int userID, string fullName, string email, string username)
        {
            bool saved = false;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                user userItem = db.users.Where(usr => usr.userID == userID).FirstOrDefault();
                if (userItem != null)
                {
                    userItem.fullName = fullName;

                    //update the identity by logging out and logging back in if email has changed
                    if (email.ToLower() != userItem.email.ToLower())
                    {
                        IFormsAuthenticationService FormsService = new FormsAuthenticationService();
                        FormsService.SignOut();
                        FormsService.SignIn(email, false);
                    }

                    userItem.email = email;
                    userItem.userName = username;

                    db.SubmitChanges();
                    HttpContext.Current.Session["CurrentUser"] = null;
                    saved = true;

                    HttpContext.Current.Cache.Remove("userID" + userItem.email.ToLower());
                }
            }
            catch (Exception) 
            {
                saved = false;
            }

            return saved;
        }

        public static int Follow(int userID, int accountID, string type)
        {
            int totalAccount = 0;
            try
            {
                flushAccountCache(userID.ToString());
                CoachCueDataContext db = new CoachCueDataContext();
                
                users_account usrAccount = new users_account();
                usrAccount.userID = userID;
                usrAccount.accountID = accountID;
                usrAccount.accountTypeID = accounttype.GetID(type);

                db.users_accounts.InsertOnSubmit(usrAccount);

                db.SubmitChanges();
                totalAccount++;

                HttpContext.Current.Cache.Remove("followerCount" + accountID.ToString());
            }
            catch (Exception ex) 
            {
                string s = ex.Message;
            }

            return totalAccount;
        }

        public static void UnFollow(int userID, int accountID, string type)
        {
            try
            {
                flushAccountCache(userID.ToString());
                CoachCueDataContext db = new CoachCueDataContext();

                users_account usrAccount = (from usrAcnt in db.users_accounts
                                             where usrAcnt.userID == userID && usrAcnt.accountID == accountID
                                             select usrAcnt).FirstOrDefault();

                if (usrAccount != null)
                {
                    db.users_accounts.DeleteOnSubmit(usrAccount);
                    db.SubmitChanges();
                }

                HttpContext.Current.Cache.Remove("followerCount" + accountID.ToString());
            }
            catch (Exception) { }
        }

        public static bool CheckAddFollow(user userItem, int playerID)
        {
            bool includePlayer = true;

            try
            {
                users_account account = userItem.users_accounts.Where(usracnt => usracnt.accounttype.accountName == "players" && usracnt.accountID == playerID).FirstOrDefault();
                if (account != null)
                {
                    //already following so return true
                    if (account.accountID == playerID)
                        return false;
                }

                //not following so add to list
                user.Follow(userItem.userID, playerID, "players");
            }
            catch (Exception) { }

            return includePlayer;
        }

        public static bool CheckAddFollow(WeeklyMatchups usrMatchup)
        {
            bool includePlayer = true;

            try
            {
                users_account account = usrMatchup.CreatedBy.users_accounts.Where(usracnt => usracnt.accounttype.accountName == "players" && 
                    ( usracnt.accountID == usrMatchup.Player1.PlayerID || usracnt.accountID == usrMatchup.Player2.PlayerID)).FirstOrDefault();
                
                if (account != null)
                {
                    //already following so return true
                    if (account.accountID == usrMatchup.Player1.PlayerID || account.accountID == usrMatchup.Player2.PlayerID)
                        return false;
                }

                //not following so add the first player to list
                user.Follow(usrMatchup.CreatedBy.userID, usrMatchup.Player1.PlayerID, "players");
            }
            catch (Exception) { }

            return includePlayer;
        }

        public static List<FollowData> GetAccounts(int userID)
        {
            List<FollowData> accounts = new List<FollowData>();

            try
            {
                //check the cache first
                string cacheID = userID + "Account";
                if (HttpContext.Current.Cache[cacheID] != null)
                    accounts = (List<FollowData>)HttpContext.Current.Cache[cacheID];
                else
                {
                    CoachCueDataContext db = new CoachCueDataContext();
                    var acnts = from usracnt in db.users_accounts
                                where usracnt.userID == userID
                                join plyrs in db.nflplayers on
                                usracnt.accountID equals plyrs.playerID
                                where plyrs.status.statusName == "Active" && usracnt.accounttype.accountName == "players"
                                select new FollowData { player = plyrs };

                    //string s = ((System.Data.Objects.ObjectQuery)acnts).ToTraceString();

                    if (acnts.Count() > 0)
                    {
                        accounts = acnts.ToList();
                        HttpContext.Current.Cache.Insert(cacheID, accounts, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(4, 0, 0));
                    }
                }
            }
            catch (Exception){}

            return accounts;
        }

        public static List<users_account> GetFollowAll(int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<users_account> accounts = new List<users_account>();

            try
            {
                var accountList = from usrs in db.users_accounts
                               where usrs.user.status.statusName == "Active"
                               && usrs.userID == userID
                               select usrs;

                if (accountList.Count() > 0)
                    accounts = accountList.ToList();
            }
            catch (Exception) { }

            return accounts;
        }

        public static List<int> GetFollowUsers(int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();

            List<int> accountIDs = (from usrs in db.users_accounts
                                    where usrs.accounttype.accountName == "users" && usrs.user.status.statusName == "Active"
                                    && usrs.userID == userID
                                    select usrs.accountID).ToList();
            return accountIDs;
        }

        public static List<AccountData> GetFollowingPlayers(int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<AccountData> accounts = new List<AccountData>();

            try
            {
                var acnts = from usracnt in db.users_accounts
                            where usracnt.userID == userID && usracnt.accounttype.accountName == "players"
                            join plyrs in db.nflplayers on
                            usracnt.accountID equals plyrs.playerID
                            where plyrs.status.statusName == "Active"
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
                                following = 1,
                                teamSlug = plyrs.nflteam.teamSlug,
                                accountType = "players"
                            };

                if (acnts.Count() > 0)
                    accounts = acnts.ToList();
            }
            catch (Exception) { }

            return accounts;
        }

        public static List<user> GetFollowingUsers(int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<user> coaches = new List<user>();

            try
            {
                var acnts = from usrs in db.users
                            where usrs.status.statusName == "Active"
                            join usracnt in db.users_accounts on
                            usrs.userID equals usracnt.accountID 
                            where usracnt.userID == userID
                            select usrs;
                
                if (acnts.Count() > 0)
                    coaches = acnts.ToList();
            }
            catch (Exception) { }

            return coaches;
        }

        public static int GetFollowingCount(int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            int count = 0;

            try
            {
                var acnts = from usracnt in db.users_accounts
                            where usracnt.userID == userID
                            select usracnt.accountID;

                count = acnts.Count();
            }
            catch (Exception) { }

            return count;
        }

        public static List<FollowData> GetRandomAccounts(int total)
        {
            List<FollowData> accounts = new List<FollowData>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();
                var acnts = from plyrs in db.nflplayers
                            where plyrs.featured == true && plyrs.status.statusName == "Active" && plyrs.hasTwitterAccount == true && plyrs.position.positiontype.positionTypeName == "Offense"
                            && (plyrs.position.positionName == "QB" || plyrs.position.positionName == "TE" || plyrs.position.positionName == "RB" || plyrs.position.positionName == "WR")
                            select new FollowData { player = plyrs };

                int skipTo = new Random().Next(0, acnts.Count() - 10);
                int seed = new Random().Next();
                accounts = acnts.OrderBy(s => (~(s.accountID & seed)) & (s.accountID | seed)).Skip(skipTo).Take(total).ToList();
            }
            catch (Exception ex) 
            {
                string msg = ex.Message;
                msg = string.Empty;
            }

            return accounts;
        }


        public static bool IsValidEmail(string email)
        {
            bool valid = false;

            if (string.IsNullOrEmpty(email))
                return valid;

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.email.ToLower() == email.ToLower()
                          select mt;

                if (ret.Count() <= 0)
                {
                    try
                    {
                        string address = new MailAddress(email).Address;
                        valid = true;
                    }
                    catch (FormatException)
                    {
                        //address is invalid
                        valid = false;
                    }              
                }
            }
            catch (Exception) { }

            return valid;
        }

        public static bool IsValidUser(string username, string password)
        {
            bool valid = false;
            string COACHCUE_AUTH_COOKIE = "coachcue_auth";

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.email == username && mt.password == password && mt.status.statusName == "Active"
                          select mt;

                user userItem = ret.FirstOrDefault();

                if (userItem != null)
                {
                    valid = true;

                    //also set the cookie
                    HttpCookie coachCueCookie = new HttpCookie(COACHCUE_AUTH_COOKIE);
                    coachCueCookie.HttpOnly = false; // Not accessible by JS.
                    coachCueCookie.Values["userGUID"] = userItem.userGuid;
                    coachCueCookie.Expires = DateTime.UtcNow.GetEasternTime().AddYears(3);

                    HttpContext.Current.Response.Cookies.Add(coachCueCookie);
                }
            }
            catch (Exception)
            {
                valid = false;
            }

            return valid;
        }

        public static user GetUserByOpenID(string openID)
        {
            user userItem = null;
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                          where mt.openID == openID && mt.status.statusName == "Active"
                          select mt;

                userItem = ret.FirstOrDefault();
            }
            catch (Exception)
            {
                userItem = null;
            }

            return userItem;
        }

        public static user GetUserByEmail(string email)
        {
            user userItem = null;
            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var ret = from mt in db.users
                            where mt.email == email && mt.status.statusName == "Active"
                            select mt;

                userItem = ret.FirstOrDefault();
            }
            catch (Exception)
            {
                userItem = null;
            }

            return userItem;
        }


        public static int GetUserID(string email)
        {
            int userID = 0;
            try
            {
                string cacheID = "userID" + email.ToLower();
                if (HttpContext.Current.Cache[cacheID] != null)
                    userID = (int)HttpContext.Current.Cache[cacheID];
                else
                {
                    CoachCueDataContext db = new CoachCueDataContext();

                    var ret = from mt in db.users
                                where mt.email == email && mt.status.statusName == "Active"
                                select mt;

                    user userItem = ret.FirstOrDefault();

                    if (userItem != null)
                    {
                        userID = userItem.userID;
                        HttpContext.Current.Cache.Insert(cacheID, userID, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(500, 0, 0));
                    }
                }
            }
            catch (Exception)
            {
                userID = 0;
            }

            return userID;
        }

        public static user GetByUsername(string username)
        {
            user userItem = new user();
            try
            {
                //if (HttpContext.Current.Session["CurrentUser"] != null)
                //    userItem = (user)HttpContext.Current.Session["CurrentUser"];

                //if (string.IsNullOrEmpty(userItem.userName))
                //{
                CoachCueDataContext db = new CoachCueDataContext();

                    var ret = from mt in db.users
                              where mt.email == username && mt.status.statusName == "Active"
                              select mt;

                    userItem = ret.FirstOrDefault();

                    if (userItem != null)
                        HttpContext.Current.Session["CurrentUser"] = userItem;
                //}
            }
            catch (Exception){}

            return userItem;
        }

        public static int SaveLogin(int userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            int logins = 0;

            try
            {
                if (HttpContext.Current.Session["RecordedLogin"] == null)
                {
                    user_login login = new user_login();
                    login.userID = userID;
                    login.loginDate = DateTime.UtcNow.GetEasternTime();
                    db.user_logins.InsertOnSubmit(login);

                    db.SubmitChanges();

                    HttpContext.Current.Session["RecordedLogin"] = "true";
                }

                logins = db.user_logins.Where(uslog => uslog.userID == userID).Count();
            }
            catch (Exception) { }

            return logins;
        }

        public static List<user> GetRecentMatchupVoters(int total, int userID)
        {
            List<user> voters = new List<user>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                List<int> ids = db.users_matchups.Where(vote => vote.userID != userID).OrderByDescending(vote => vote.dateCreated).Select(x => x.userID).Distinct().Take(total).ToList();

                voters = (from usrs in db.users
                                  where usrs.status.statusName == "Active" && ids.Contains(usrs.userID)
                                  select usrs).ToList();
            }
            catch (Exception) { }

            return voters;
        }

        public static UserVoteData AddMatchup(int userID, int playerID, int matchupID)
        {
            UserVoteData userVote = new UserVoteData(); ;
            try
            {
                DateTime dateCreated = DateTime.UtcNow.GetEasternTime();
                // flushAccountCache(userID.ToString());
                CoachCueDataContext db = new CoachCueDataContext();

                //double check to see if user already voted
                if (db.users_matchups.Where(um => um.userID == userID && um.matchupID == matchupID).Count() > 0 && userID != 15754)
                {
                    userVote.MatchupID = 0;
                }
                else
                {
                    users_matchup usrMatchup = new users_matchup();
                    usrMatchup.userID = userID;
                    usrMatchup.selectedPlayerID = playerID;
                    usrMatchup.matchupID = matchupID;
                    usrMatchup.dateCreated = dateCreated;

                    db.users_matchups.InsertOnSubmit(usrMatchup);
                    db.SubmitChanges();

                    //now record as a notification - let user know someone has voted on it
                    matchup matchupItem = matchup.Get(matchupID);
                    WeeklyMatchups weeklyMatchupItem = matchup.GetWeeklyMatchup(matchupItem, false, false);

                    notification.Add("voteMatchup", matchupID, userID, matchupItem.createdBy, dateCreated);

                    //finally return information about the matchup
                    user userItem = user.Get(userID);
                    userVote.fullName = userItem.fullName;
                    userVote.correctPercentage = ( userItem.CorrectPercentage != 0 ) ? userItem.CorrectPercentage + "%" : string.Empty;
                    userVote.profileImg = "/assets/img/avatar/" + userItem.avatar.imageName;
                    userVote.SelectedPlayer = nflplayer.Get(playerID).fullName;
                    userVote.CorrectMatchup = false;
                    userVote.MatchupID = matchupID;
                    userVote.userVoteID = userID;
                    userVote.Player1TotalVotes = weeklyMatchupItem.Player1.TotalVotes;
                    userVote.Player2TotalVotes = weeklyMatchupItem.Player2.TotalVotes;
                }
            }
            catch (Exception) { }

            return userVote;
        }

        public static WeeklyMatchups AddStreamSelectedMatchup(int userID, int playerID, int matchupID)
        {
            WeeklyMatchups matchupChoice = new WeeklyMatchups(); ;
            
            try
            {
                AddMatchup(userID, playerID, matchupID);
                matchupChoice = matchup.GetWeeklyMatchup(matchup.Get(matchupID), false, false, userID);
            }
            catch (Exception) { }

            return matchupChoice;
        }

        public static List<LeaderboardCoach> GetTopCoachesByWeek(int number, int week, int seasonID)
        {
            List<LeaderboardCoach> topCoaches = new List<LeaderboardCoach>();
            string cacheID = "leaders" + "week" + week.ToString() + seasonID.ToString();
            try
            {
                if (HttpContext.Current.Cache[cacheID] != null)
                    topCoaches = (List<LeaderboardCoach>)HttpContext.Current.Cache[cacheID];
                else
                {
                    CoachCueDataContext db = new CoachCueDataContext();
                    var coaches = (from usr in db.users
                                   where usr.status.statusName == "Active"
                                   select usr).ToList();

                    foreach (user coach in coaches)
                    {
                        if (coach.users_matchups != null)
                        {
                            LeaderboardCoach topCoach = new LeaderboardCoach();
                            topCoach.Coach = coach;

                            if (week == 0)
                            {
                                topCoach.Total = coach.users_matchups.Where(um => um.correctMatchup == true || um.correctMatchup == false).Count();
                                if (topCoach.Total == 0)
                                    topCoach.Percent = 0;
                                else
                                {
                                    topCoach.Wrong = coach.users_matchups.Where(um => um.correctMatchup == false).Count();
                                    topCoach.Correct = coach.users_matchups.Where(um => um.correctMatchup == true).Count();
                                    topCoach.Percent = topCoach.Correct * 100 / topCoach.Total;
                                }
                            }
                            else
                            {
                                var weeklyMatchups = coach.users_matchups.Where(us => us.matchup.gameschedule1.weekNumber == week && us.matchup.gameschedule1.seasonID == 4 && us.matchup.status.statusName != "Deleted" && (us.correctMatchup == true || us.correctMatchup == false));
                                if (weeklyMatchups != null)
                                {
                                    topCoach.Total = weeklyMatchups.Count();
                                    if (topCoach.Total != 0)
                                    {
                                        topCoach.Wrong = weeklyMatchups.Where(um => um.correctMatchup == false).Count();
                                        topCoach.Correct = weeklyMatchups.Where(um => um.correctMatchup == true).Count();
                                        topCoach.Percent = topCoach.Correct * 100 / topCoach.Total;
                                    }
                                }
                            }

                            if (topCoach.Percent != 0)
                                topCoaches.Add(topCoach);
                        }
                    }

                    HttpContext.Current.Cache.Insert(cacheID, topCoaches, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(600, 0, 0));
                }
            }
            catch (Exception) { }

            return topCoaches.OrderByDescending( ch => ch.Correct ).Take(number).ToList();
        }

        public static TopCoaches GetTopCoaches(int number)
        {
            //cache this for each week
            gameschedule week = gameschedule.GetCurrentWeek();

            TopCoaches topCoaches = new TopCoaches();
            
            topCoaches.OverallTopCoaches = new List<user>();
            topCoaches.WeeklyTopCoaches = new List<user>();
  
            try
            {
                string cacheID = "week" + week.weekNumber + number.ToString();
                if (HttpContext.Current.Cache[cacheID] != null)
                    topCoaches = (TopCoaches)HttpContext.Current.Cache[cacheID];
                else
                {
                    CoachCueDataContext db = new CoachCueDataContext();

                    //look through each user - see what matchups they selected
                    //then see if the player they selected had a higher point total
                    var coaches = (from usr in db.users
                                   where usr.status.statusName == "Active"
                                   select usr).ToList();
                    
                    if (week.weekNumber != 0)
                    {
                        topCoaches.WeekNumber = week.weekNumber;
                        topCoaches.WeeklyTopCoaches = coaches.OrderByDescending(coa => coa.CorrectWeeklyPercentage).Where(coa => coa.CorrectWeeklyPercentage != null).ToList();
                    }

                    topCoaches.OverallTopCoaches = coaches.OrderByDescending(coa => coa.CorrectPercentage).Take(number).ToList();
                    HttpContext.Current.Cache.Insert(cacheID, topCoaches, null, System.Web.Caching.Cache.NoAbsoluteExpiration, new TimeSpan(0, 10, 0));
                }
            }
            catch (Exception) {}

            return topCoaches;
        }

        public static List<UserExportData> GetAllAccounts()
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<UserExportData> accounts = new List<UserExportData>();

            try
            {
                var acnts = from usr in db.users
                            where usr.status.statusName == "Active"
                            select new UserExportData
                            {
                                name = usr.fullName,
                                image = "/assets/img/avatar/" + usr.avatar.imageName,
                                username = usr.userName,
                                value = usr.fullName,
                                userID = usr.userID
                            };

                if (acnts.Count() > 0)
                    accounts = acnts.ToList();
            }
            catch (Exception ex) 
            {
                string err = ex.Message;
            }

            return accounts;
        }

        public static List<UserData> TypeAheadSearch(string searchTerm, int? userID)
        {
            CoachCueDataContext db = new CoachCueDataContext();
            List<UserData> accounts = new List<UserData>();
            bool checkUser = false;
            int followingUser = 0;
            List<int> followingUsers = new List<int>();

            try
            {
                string searchString = searchTerm.ToLower();

                var acnts = from usr in db.users
                            where usr.status.statusName == "Active" &&
                            usr.fullName.ToLower().Contains(searchString)
                            //cant do this until the typeahead js can match the find with the search and the result
                            //(usr.fullName.ToLower().Contains(searchString) ||
                            //usr.userName.ToLower().Contains(searchString) ||
                            //usr.email.ToLower().Contains(searchString))
                            select new UserData
                            {
                                email = usr.email,
                                fullName = usr.fullName,
                                profileImg = usr.avatar.imageName,
                                username = usr.userName,
                                userID = usr.userID,
                                following = 0,
                                //correctPercentage = usr.CorrectPercentage.ToString() + "%"
                            };

                if (acnts.Count() > 0)
                    accounts = acnts.ToList();

                //see if the user is following or not
                if (userID.HasValue)
                {
                    if (userID.Value != 0)
                    {
                        checkUser = true;
                        followingUsers = (from usrs in db.users
                                            where usrs.status.statusName == "Active"
                                            join usracnt in db.users_accounts on
                                            usrs.userID equals usracnt.accountID
                                            where usracnt.userID == userID
                                            select usrs.userID).ToList();

                        foreach (UserData acnt in accounts)
                        {
                            if (checkUser && followingUsers.Count() > 0)
                                followingUser = (followingUsers.Contains(acnt.userID)) ? 1 : 0;

                            acnt.following = followingUser;
                        }
                    }
                }
            }
            catch (Exception) { }

            return accounts;
        }

        public static List<user> GetFollowers( string accountType, int accountID )
        {
            List<user> followers = new List<user>();

            try
            {
                CoachCueDataContext db = new CoachCueDataContext();

                var usrs = from urs in db.users
                                where urs.status.statusName == "Active"
                                join flws in db.users_accounts on
                                urs.userID equals flws.userID
                                where flws.accountID == accountID && flws.accounttype.accountName == accountType
                                select urs;
                
                if (usrs.Count() > 0)
                    followers = usrs.ToList();
            }
            catch (Exception) { }

            return followers;
        }

        public static List<user> GetInviteUser( int userID, int total )
        {
            List<user> invites = new List<user>();
            List<user> following = GetFollowingUsers(userID);

            if (following.Count() > 10)
            {
                //get a random list from who they are following
                int skipTo = new Random().Next(0, 5);
                int seed = new Random().Next();
                invites = following.OrderBy(s => (~(s.userID & seed)) & (s.userID | seed)).Skip(skipTo).Take(total).ToList();
            }
            else
            {
                //get a random list from top 5 most active
                invites = GetRecentMatchupVoters(total, userID);
            }

            return invites;
        }

        public static List<user> GetInviteUser(int userID, matchup matchup, int count)
        {
            List<user> invites = new List<user>();
            int total = count;

            List<user> following = GetFollowingUsers(userID);
            List<user> followingNotInvited = new List<user>();

            following.AddRange( GetRecentMatchupVoters(total, userID) );
            
            //remove users who may already be invited
            List<int> invitedUsers = matchup.users_matchups.Select(um => um.userID).ToList();
            foreach( user invite in following )
            {
                if( !invitedUsers.Contains(invite.userID))
                {
                    followingNotInvited.Add(invite);
                }
            }

            //get a random list from who they are following
            int skipTo = new Random().Next(0, count);
            int seed = new Random().Next();
            invites = followingNotInvited.OrderBy(s => (~(s.userID & seed)) & (s.userID | seed)).Skip(skipTo).Take(total).ToList();
           
            return invites;
        }

        private static void flushAccountCache(string userID)
        {
            string cacheID = userID + "Account";

            if (HttpContext.Current.Cache[cacheID] != null)
            {
                List<FollowData> accounts = (List<FollowData>)HttpContext.Current.Cache[cacheID];

                HttpContext.Current.Cache.Remove(cacheID);
            }
        }
    }

    public class FollowData
    {
        public int teamID { get; set; }
        public int accountID { get; set; }
        public nflplayer player { get; set; }
    }

    public class UserExportData
    {
        public string name { get; set; }
        public string username { get; set; }
        public string image { get; set; }
        public string value { get; set; }
        public int userID { get; set; }
        public string link 
        {
            get
            {
                return "/Coach/" + userID.ToString() + "/" + name;
            }
        }
    }

    public class UserData
    {
        public string value { get; set; }
        public int userID { get; set; }
        public string fullName { get; set; }
        public string profileImg { get; set; }
        public string correctPercentage { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public int following { get; set; }
    }

    public class UserVoteData : UserData
    {
        public string SelectedPlayer { get; set; }
        public int SelectedPlayerID { get; set; }
        public bool CorrectMatchup { get; set; }
        public bool NoVotes { get; set; }
        public int MatchupID { get; set; }
        public int Player1TotalVotes { get; set; }
        public int Player2TotalVotes { get; set; }
        public DateTime DateCreated { get; set; }
        public int userVoteID { get; set; }
        public bool Verified { get; set; }
    }

    public class VoteList
    {
        public List<UserVoteData> Votes { get; set; }
        public string Status { get; set; }
        public int MatchupID { get; set; }
    }

    public class AccountData
    {
        public string name { get; set; }
        public int currentUserID { get; set; }
        public string value { get; set; }
        public string username { get; set; }
        public string fullName { get; set; }
        public string shortName { get; set; }
        public int accountID { get; set; }
        public int following { get; set; }
        public string profileImg { get; set; }
        public string profileImgLarge { get; set; }
        public string user { get; set; }
        public string position { get; set; }
        public string team { get; set; }
        public string college { get; set; }
        public string number { get; set; }
        public string twitterLink { get; set; }
        public string teamSlug { get; set; }
        public string link { get; set; }
        public string accountType { get; set; }
        public string linkFullName
        {
            get
            {
                string name = this.fullName.Replace(".", "").Replace("-", "").Replace("'", "");
                return name;
            }
        }
    }

    public class TopCoaches
    {
        public List<user> OverallTopCoaches { get; set; }
        public List<user> WeeklyTopCoaches { get; set; }
        public int WeekNumber { get; set; }
    }

    public class LeaderboardCoach
    {
        public user Coach { get; set; }
        public int Percent { get; set; }
        public int Correct { get; set; }
        public int Wrong { get; set; }
        public int Total { get; set; }
    }

    #region Services
    // The FormsAuthentication type is sealed and contains static members, so it is difficult to
    // unit test code that calls its members. The interface and helper class below demonstrate
    // how to create an abstract wrapper around such a type in order to make the AccountController
    // code unit testable.

    public interface IMembershipService
    {
        int MinPasswordLength { get; }

        bool ValidateUser(string userName, string password);
        MembershipCreateStatus CreateUser(string userName, string password, string email);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
    }

    public class AccountMembershipService : IMembershipService
    {
        private readonly MembershipProvider _provider;

        public AccountMembershipService()
            : this(null)
        {
        }

        public AccountMembershipService(MembershipProvider provider)
        {
            _provider = provider ?? Membership.Provider;
        }

        public int MinPasswordLength
        {
            get
            {
                return _provider.MinRequiredPasswordLength;
            }
        }

        public bool ValidateUser(string userName, string password)
        {
            //if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            //if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if( string.IsNullOrEmpty( userName )  || string.IsNullOrEmpty( password ) )
                return false;

            return user.IsValidUser(userName, password);
        }

        public MembershipCreateStatus CreateUser(string userName, string password, string email)
        {
            MembershipCreateStatus status = new MembershipCreateStatus();
            return status;
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(oldPassword)) throw new ArgumentException("Value cannot be null or empty.", "oldPassword");
            if (String.IsNullOrEmpty(newPassword)) throw new ArgumentException("Value cannot be null or empty.", "newPassword");

            // The underlying ChangePassword() will throw an exception rather
            // than return false in certain failure scenarios.
            try
            {
                MembershipUser currentUser = _provider.GetUser(userName, true /* userIsOnline */);
                return currentUser.ChangePassword(oldPassword, newPassword);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (MembershipPasswordException)
            {
                return false;
            }
        }
    }

    public interface IFormsAuthenticationService
    {
        void SignIn(string userName, bool createPersistentCookie);
        void SignOut();
    }

    public class FormsAuthenticationService : IFormsAuthenticationService
    {
        public void SignIn(string userName, bool createPersistentCookie)
        {
            if (String.IsNullOrEmpty(userName)) throw new ArgumentException("Value cannot be null or empty.", "userName");

            FormsAuthentication.SetAuthCookie(userName, createPersistentCookie);
        }

        public void SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session["CurrentUser"] = null;
            HttpContext.Current.Session["UserID"] = null;
        }
    }
    #endregion

}