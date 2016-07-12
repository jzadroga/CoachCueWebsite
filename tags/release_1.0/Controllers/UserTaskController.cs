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

            if (type.ToLower() == "players")
            {
                nflplayer player = nflplayer.Get(accountID);
                List<StreamContent> streamList = stream.GetPlayerStream(player, userID, true, null);
                DateTime lastUpdated = (streamList.Count() > 0) ? streamList[0].DateCreated : DateTime.Now;

                List<PlayerStream> playerStream = new List<PlayerStream>();
                playerStream.Add(new PlayerStream
                {
                    BuildPlayerHeader = true,
                    Player = player,
                    LastUpdate = lastUpdated,
                    StreamItems = streamList
                });

                string streamData = this.PartialViewToString("_StreamItemList", playerStream);

                return Json(new
                {
                    StreamData = streamData,
                    ID = accountID,
                    Type = "message",
                    LastUpdateTicks = lastUpdated.Ticks,
                    AddPlayerHeader = true,
                    Inline = false
                }, JsonRequestBehavior.AllowGet);
            }

            return Json(new
            {
                StreamData = new List<StreamContent>(),
                ID = accountID,
                Type = "message",
                LastUpdateTicks = DateTime.Now.Ticks,
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
        public JsonResult InviteAnswer(int userID, int matchupID)
        {
            notification notice = notification.Add( "voteRequested", matchupID, user.GetUserID(User.Identity.Name), userID, DateTime.Now);
            EmailHelper.SendVoteRequestEmail(user.GetUserID(User.Identity.Name), userID, matchupID, notice.notificationGUID);

            user userItem = user.Get(userID);
            UserData userData = new UserData();
            userData.username = userItem.userName;
            userData.userID = userItem.userID;
            userData.profileImg = "/assets/img/avatar/" + userItem.avatar.imageName;
            userData.correctPercentage = ( userItem.CorrectPercentage != 0 ) ? userItem.CorrectPercentage.ToString() + "%" : string.Empty;
            userData.fullName = userItem.fullName;

            return Json(new { User = userData, Matchup = matchupID }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult SetMatchupChoice(int playerID, int matchupID)
        {
            UserVoteData userVote = user.AddMatchup(user.GetUserID(User.Identity.Name), playerID, matchupID);
            return Json(userVote, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult SetStreamMatchupChoice(int playerID, int matchupID)
        {
            WeeklyMatchups userVote = user.AddStreamSelectedMatchup(user.GetUserID(User.Identity.Name), playerID, matchupID);
            StreamContent streamItem = stream.ConvertToStream(userVote, playerID, true);

            string streamData = this.PartialViewToString("_StreamItem", streamItem);
            return Json(new
            {
                StreamData = streamData,
                ID = matchupID,
                Type = "matchupSelected",
                Inline = false
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        [HttpPost]
        public ActionResult GetStreamUpdateCount(List<int> ids, List<string> tms)
        {
            bool updated = false;
            int updateCount = 0;

            int? userID = null;
            if (User.Identity.IsAuthenticated)
            {
                userID = user.GetUserID(User.Identity.Name);
                DateTime lastDate = DateTime.Now;
                updateCount = stream.GetUpdateStreamCount((int)userID, ids, tms);
                if (updateCount > 0)
                    updated = true;
            }

            return Json(new
            {
                UpdatesFound = updated,
                UpdateCount = updateCount
            });
        }

        [NoCacheAttribute]
        public JsonResult ValidEmail(string email)
        {
            bool valid = user.IsValidEmail(email);

            return Json(new { Valid = valid }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public ActionResult GetStream()
        {
            List<PlayerStream> streamList = new List<PlayerStream>();

            int? userID = null;
            if (User.Identity.IsAuthenticated)
            {
                userID = user.GetUserID(User.Identity.Name);
                streamList = stream.GetPlayersStream((int)userID);
            }

            string streamData = this.PartialViewToString("_StreamItemList", streamList);
            return Json(new
            {
                StreamData = streamData,
                ID = 0,
                Type = "list"
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult SaveMessage(int plyID, string msg, bool inline, int prnt)
        {   
            message userMessage = message.Save(user.GetUserID(User.Identity.Name), plyID, msg, (prnt == 0) ? null : (int?)prnt);
            string streamData = string.Empty;
            StreamContent streamItem = stream.ConvertToStream(userMessage, plyID);

            if (prnt == 0)
                streamData = this.PartialViewToString("_StreamItem", streamItem);
            else
            {
                List<message> msgs = message.GetConversation(prnt);
                streamData = this.PartialViewToString("_MessageConversation", msgs);
            }

            return Json(new
            {
                StreamData = streamData,
                ID = plyID,
                Type = "message",
                Inline = inline,
                LastUpdateTicks = streamItem.DateTicks,
                AddPlayerHeader = false,
                ParentID = prnt
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult SaveMessageReply(int plyID, string msg, int parent, bool showReply)
        {   
            message userMessage = message.Save(user.GetUserID(User.Identity.Name), plyID, msg, parent);

            ConversationViewModel cVM = new ConversationViewModel();
            List<message> messages = new List<message>();
            messages.Add(userMessage);
            cVM.ReplyMessages = messages;
            cVM.ParentMessage = new message();
            cVM.IsAuth = false;
            cVM.SingleMessage = true;
            cVM.ShowReply = showReply;

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            if (userID != 0)
            {
                cVM.IsAuth = true;
                cVM.Avatar = user.Get(userID).avatar.imageName;
            }

            string streamData = this.PartialViewToString("_MessageCtrl", cVM);

            return Json(new
            {
                StreamData = streamData,
                ID = plyID,
                Type = "message",
                AddPlayerHeader = false,
                ParentID = parent
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public ActionResult GetConversation(int msgID)
        {
            List<message> msgs = message.GetConversation(msgID);
            if (User.Identity.IsAuthenticated)
                ViewData["UserImage"] = user.GetUserByEmail(User.Identity.Name).avatar.imageName;
            
            string streamData = this.PartialViewToString("_MessageConversation", msgs);

            return Json(new
            {
                StreamData = streamData,
                ID = msgID,
                Type = "message"
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult AddMatchup(int player1, int player2, int scoringTypeID)
        {
            StreamContent streamItem = new StreamContent();
            string streamData = string.Empty;

            int? userID = user.GetUserID(User.Identity.Name);
            WeeklyMatchups usrMatchup = matchup.AddUserMatchup(player1, player2, scoringTypeID, (int)userID);
            streamItem = stream.ConvertToStream(usrMatchup, player1, false);
            streamData = this.PartialViewToString("_StreamItem", streamItem);
  
            return Json(new
            {
                StreamData = streamData,
                ID = player1,
                Type = "matchup",
                LastUpdateTicks = streamItem.DateTicks,
                AddPlayerHeader = false,
                Inline = true,
                Existing = usrMatchup.ExistingMatchup,
                MatchupID = usrMatchup.MatchupID
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult AddMatchupItem(int player1, int player2, int scoringTypeID)
        {
            StreamContent streamItem = new StreamContent();

            int? userID = user.GetUserID(User.Identity.Name);
            WeeklyMatchups usrMatchup = matchup.AddUserMatchup(player1, player2, scoringTypeID, (int)userID);
            usrMatchup.AllowVote = true;
            string streamData = this.PartialViewToString("_MatchupItem", usrMatchup);
            string carouselData = this.PartialViewToString("_CarouselMatchupItem", usrMatchup);

            return Json(new
            {
                CarouselData = carouselData,
                MatchupData = streamData,
                ID = player1,
                Type = "matchup",
                Existing = usrMatchup.ExistingMatchup,
                MatchupID = usrMatchup.MatchupID
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
