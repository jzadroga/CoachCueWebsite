using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoachCue.Model;
using System.ComponentModel;
using CoachCue.ViewModels;
using System.Net;
using System.IO;
using CoachCue.Helpers;
using System.Web.Script.Serialization;
using CoachCue.Utility;
using System.Globalization;
using System.Threading.Tasks;
using CoachCue.Service;
using CoachCue.Repository;

namespace CoachCue.Controllers
{
    public class UserTaskController : Controller
    {
        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult Follow(int accountID, string type)
        {
            //add to the folow list and return the players stream
            int userID = user.GetUserID(User.Identity.Name);
            int totalAccounts = user.Follow(userID, accountID, type);

            return Json(new
            {
                StreamData = new List<StreamContent>(),
                ID = accountID,
                Type = "message",
                LastUpdateTicks = DateTime.UtcNow.GetEasternTime().Ticks,
                AddPlayerHeader = false
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult Unfollow(int accountID, string type)
        {
            user.UnFollow(user.GetUserID(User.Identity.Name), accountID, type);
            return Json(new { Result = true, ID = accountID }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public JsonResult Get()
        {
            if (User.Identity.IsAuthenticated)
                return Json(new { Auth = true, Accounts = user.GetAccounts(user.GetUserID(User.Identity.Name)) }, JsonRequestBehavior.AllowGet);

            //get random accounts for users just visiting
            return Json(new { Auth = false, Accounts = user.GetRandomAccounts(10) }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult GetUserData(int userID)
        {
            UserData userData = new UserData();
  
            user userItem = user.Get(userID);
            userData.username = userItem.userName;
            userData.userID = userItem.userID;
            userData.profileImg = "/assets/img/avatar/" + userItem.avatar.imageName;
            userData.correctPercentage = (userItem.CorrectPercentage != 0) ? userItem.CorrectPercentage.ToString() + "%" : string.Empty;
            userData.fullName = userItem.fullName;
  
            return Json(new { User = userData }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult InviteAnswer(int userID, int matchupID)
        {
            UserData userData = new UserData();

            notification notice = notification.Add("voteRequested", matchupID, user.GetUserID(User.Identity.Name), userID, DateTime.UtcNow.GetEasternTime());
            if (!string.IsNullOrEmpty(notice.notificationGUID))
            {
                //emails are all sent later
                EmailHelper.SendVoteRequestEmail(user.GetUserID(User.Identity.Name), userID, matchupID, notice.notificationGUID);

                user userItem = user.Get(userID);
                userData.username = userItem.userName;
                userData.userID = userItem.userID;
                userData.profileImg = "/assets/img/avatar/" + userItem.avatar.imageName;
                userData.correctPercentage = (userItem.CorrectPercentage != 0) ? userItem.CorrectPercentage.ToString() + "%" : string.Empty;
                userData.fullName = userItem.fullName;
            }

            return Json(new { User = userData, Matchup = matchupID }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public async Task<ActionResult> SetStreamMatchupChoice(string id, string player, string matchup)
        {
            //adding a guest account to see what happens with voting
            bool showLogin = false;

            var userData = (User.Identity.IsAuthenticated) ? await CoachCueUserData.GetUserData(User.Identity.Name) : null;
             
            var matchupItem = await MatchupService.AddVote(id, player, matchup, userData);

            return Json(new
            {
                //StreamData = streamData,
                PlayerID = id,
                ID = matchup,
                Matchup = matchupItem,
                Type = "matchupSelected",
                Inline = false,
                UserVotedID = (userData != null) ? userData.UserId : string.Empty,
                ShowSignup = showLogin
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        [HttpPost]
        public ActionResult GetStreamUpdateCount(string tms)
        {
            bool updated = false;
            int updateCount = 0;

            int? userID = null;
            if (User.Identity.IsAuthenticated)
            {
                userID = user.GetUserID(User.Identity.Name);
                updateCount = stream.GetUpdateStreamCount((int)userID, tms);
                if (updateCount > 0)
                    updated = true;
            }

            return Json(new
            {
                UpdatesFound = updated,
                UpdateCount = updateCount,
                LastDate = DateTime.UtcNow.GetEasternTime().Ticks
            });
        }

        public JsonResult GetTwitterStream(int playerID)
        {
            nflplayer player = nflplayer.Get(playerID);
            PlayerPopover popoverData = new PlayerPopover();

            popoverData.TwitterContent = stream.GetPlayerTwitterStream(player);
            //popoverData.Voting = matchup.GetPlayerProfileVotes(player, gameschedule.GetCurrentWeekID(), player.position.positionID);

            string streamData = this.PartialViewToString("_PlayerPopover", popoverData);

            return Json(new
            {
                StreamData = streamData,
                PlayerID = playerID
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public JsonResult GetMatchupStream(string pos)
        {
            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 43;
            
            List<StreamContent> streamContent = stream.GetStream(userID, true, pos);
      
            string streamData = this.PartialViewToString("_StreamItemList", streamContent);
                        
            return Json(new
            {
                StreamData = streamData,
                Type = "list",
                LastDate = DateTime.UtcNow.GetEasternTime().AddSeconds(60).Ticks
            }, JsonRequestBehavior.AllowGet);            
        }
        
        [NoCacheAttribute]
        public JsonResult ValidEmail(string email)
        {
            bool valid = user.IsValidEmail(email);

            return Json(new { Valid = valid }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public ActionResult GetStream(string tms, bool ftr)
        {
            List<StreamContent> streamList = new List<StreamContent>();

            int? userID = null;
            if (User.Identity.IsAuthenticated)
            {
                DateTime lastDate = new DateTime(Convert.ToInt64(tms));
                userID = user.GetUserID(User.Identity.Name);
                streamList = stream.GetStream((int)userID, ftr, string.Empty, lastDate);
            }

            string streamData = this.PartialViewToString("_StreamItemList", streamList);
            return Json(new
            {
                StreamData = streamData,
                ID = 0,
                Type = "list",
                LastDate = DateTime.UtcNow.GetEasternTime().AddSeconds(60).Ticks
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        [HttpPost]
        public async Task<ActionResult> SaveMessage(string plyID, string msg, string prnt, string type, bool inline, HttpPostedFileBase img)
        {   
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);

            var message = await MessageService.Save(userData, plyID, msg, type, prnt, img);

            StreamContent streamItem = new StreamContent
            {
                MessageItem = message,
                DateTicks = message.DateCreated.Ticks.ToString(),
                ProfileImg = userData.ProfileImage,
                UserName = userData.UserName,
                FullName = userData.Name,
                ContentType = "message",
                DateCreated = message.DateCreated,
                TimeAgo = twitter.GetRelativeTime(message.DateCreated),
                HideActions = (type == "matchup") ? true : false,
                UserProfileImg = userData.ProfileImage
            };

            if (inline && !string.IsNullOrEmpty(prnt)) 
                streamItem.CssClass = "conversation-message";

            string streamData = string.Empty;

            if (inline)
                streamItem.Source = "Mention";

            if (type == "matchup")
                streamItem.ContentType = "matchupMessage";
            else if (type == "general" && !string.IsNullOrEmpty(prnt))
                streamItem.ContentType = "replyMessage";

            streamData = this.PartialViewToString("_StreamItem", streamItem);
           
            return Json(new
            {
                StreamData = streamData,
                ID = message.Id,
                Type = type,
                Inline = inline,
                MentionNotices = new List<MentionNotice>(),
            //LastUpdateTicks = streamItem.DateTicks,
                AddPlayerHeader = false,
                ParentID = prnt
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [HttpPost]
        public async Task<ActionResult> SendMessageNotifications(string messageId)
        {
            var notifications = await NotificationService.GetByMessage(messageId);

            //send off mention/reply emails
            if( notifications.Count() > 0 )
                await EmailHelper.SendMessageNotificationEmails(notifications.ToList());

            return Json(new
            {
                Sent = true
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [HttpPost]
        public async Task<ActionResult> SendMatchupVoteEmail(string mtchid, string voterid)
        {
            var notification = await NotificationService.GetByMatchup(mtchid, voterid);

            //send off voting emails
            if(notification != null)
                await EmailHelper.SendMatchupVoteEmail(notification);

            return Json(new
            {
                Sent = true
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public async Task<ActionResult> AddMatchupItem(string player1, string player2, string player3, string player4, string type)
        {
            var userData = await CoachCueUserData.GetUserData(User.Identity.Name);

            //add all the players included
            List<string> players = new List<string>();
            players.Add(player1);
            players.Add(player2);
            if (!string.IsNullOrEmpty(player3))
                players.Add(player3);
            if (!string.IsNullOrEmpty(player4))
                players.Add(player4);

            var matchup = await MatchupService.Save(userData, players, type);

            //usrMatchup.AllowVote = true;

            StreamContent streamItem = new StreamContent
            {
                MatchupItem = matchup,
                DateTicks = matchup.DateCreated.Ticks.ToString(),
                ProfileImg = userData.ProfileImage,
                UserName = userData.UserName,
                FullName = userData.Name,
                ContentType = "matchup",
                DateCreated = matchup.DateCreated,
                TimeAgo = twitter.GetRelativeTime(matchup.DateCreated),
                HideActions = false,
                UserProfileImg = userData.ProfileImage
            };

            string streamData = this.PartialViewToString("_StreamItem", streamItem);

            //get list of users to invite
            var invites = await UserService.GetRandomTopVotes(10);
            string userInvites = this.PartialViewToString("_InviteVote", invites.ToList());

            return Json(new
            {
                MatchupData = streamData,
                InviteData = userInvites,
                ID = player1,
                Type = "matchup",
                Existing = false, //usrMatchup.ExistingMatchup,
                MatchupID = matchup.Id //usrMatchup.MatchupID
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult GetUpdateNotificatons()
        {
            int? userID = user.GetUserID(User.Identity.Name);
            List<notification> notices = notification.GetByUserID((int)userID, true);
            string noticeData = this.PartialViewToString("_NoticeList", notices.Take(5).ToList());

            return Json(new
            {
                Notices = noticeData
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public JsonResult GetWeeklyStarters(int accountID)
        {
            List<MatchupByWeek> matchups = new List<MatchupByWeek>();
            if (accountID != 0)
                matchups = matchup.GetSelectedMatchups(accountID);

            return Json(new { WeeklyMatchup = matchups }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult GetUserMatchups(int accountID)
        {
            List<MatchupByWeek> matchups = new List<MatchupByWeek>();
            if (accountID != 0)
                matchups = matchup.GetUserMatchups(accountID);

            return Json(new { WeeklyMatchup = matchups }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult GetUserMatchup(int matchupID)
        {
            List<MatchupByWeek> matchups = new List<MatchupByWeek>();
            if (matchupID != 0)
                matchups = matchup.GetUserMatchup(matchupID);

            return Json(new { WeeklyMatchup = matchups }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAccounts(string query)
        {
            int userID = 0;
            if (User.Identity.IsAuthenticated)
                userID = user.GetUserID(User.Identity.Name);

            return Json(nflplayer.Search(query, userID), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 2592000, VaryByParam = "*")]
        public JsonResult SearchPlayers(string query)
        {
            int? userID = null;
            if (User.Identity.IsAuthenticated)
                userID = user.GetUserID(User.Identity.Name);

            return Json(nflplayer.TypeAheadSearch(query, userID), JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 2592000, VaryByParam = "*")]
        public JsonResult SearchPlayerItems(string query)
        {
             int? userID = null;
             if (User.Identity.IsAuthenticated)
                 userID = user.GetUserID(User.Identity.Name);

            List<AccountData> players = nflplayer.TypeAheadSearch(query, userID);
            string streamData = this.PartialViewToString("_PlayerItemList", players);

            return Json(new
            {
                Results = streamData,
            }, JsonRequestBehavior.AllowGet);
        }

        [OutputCache(Duration = 4000, VaryByParam = "*")]
        public JsonResult SearchUsers(string query)
        {
            int? userID = null;
            if (User.Identity.IsAuthenticated)
                userID = user.GetUserID(User.Identity.Name);

            return Json(user.TypeAheadSearch(query, userID), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchAll(string query)
        {
            int? userID = null;
            if (User.Identity.IsAuthenticated)
                userID = user.GetUserID(User.Identity.Name);

            List<AccountData> results = search.TypeAheadSearch(query, userID);
            string streamData = this.PartialViewToString("_SearchItemList", results);

            return Json(new
            {
                Results = streamData,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTrending(int number, string pos)
        {
            int? userID = null;
            if (User.Identity.IsAuthenticated)
                userID = user.GetUserID(User.Identity.Name);

            List<nflplayer> trendingPlayers = nflplayer.GetTrending(number, pos);
            List<AccountData> trendingAccounts = user.GetAccountsFromPlayers(trendingPlayers, (userID != 0) ? (int?)userID : null);

            string streamData = this.PartialViewToString("_PlayerItemList", trendingAccounts);

            return Json(new
            {
                Results = streamData,
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        public ActionResult GetMatchupList()
        {
            List<MatchupByWeek> matchups = matchup.GetUserMatchups(user.GetUserID(User.Identity.Name));
            return PartialView("_MacthupList", matchups);
        }
    }
}
