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

       // [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult SetStreamMatchupChoice(int playerID, int matchupID)
        {
            //adding a guest account to see what happens with voting
            bool showLogin = false;
            bool isAuthenticated = User.Identity.IsAuthenticated;
            int currentUser = (isAuthenticated) ? user.GetUserID(User.Identity.Name) : 15754;

            if (!isAuthenticated)
            {
                string hostAddress = Request.UserHostAddress;
                if (Session[hostAddress] == null)
                {
                    showLogin = true;
                    Session[hostAddress] = "set";
                }
            }

            WeeklyMatchups userVote = user.AddStreamSelectedMatchup(currentUser, playerID, matchupID);
            StreamContent streamItem = stream.ConvertToStream(userVote, playerID, true, currentUser);

            string streamData = this.PartialViewToString("_StreamItem", streamItem);
            return Json(new
            {
                StreamData = streamData,
                ID = matchupID,
                UserVotedID = currentUser,
                Type = "matchupSelected",
                Inline = false,
                ShowSignup = showLogin
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public JsonResult SetMatchupChoice(int playerID, int matchupID)
        {
            UserVoteData userVote = user.AddMatchup(user.GetUserID(User.Identity.Name), playerID, matchupID);
            return Json(userVote, JsonRequestBehavior.AllowGet);
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
        public JsonResult GetMatchupStream(string tms, bool update)
        {
            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            
            int currentWeek = gameschedule.GetCurrentWeekID();
            List<StreamContent> streamContent = stream.GetStreamMatchupsByWeek(userID, currentWeek);
            
            if( currentWeek > 1 && streamContent.Count() < 10 )  
            {
                List<StreamContent> lastWeekMatchupList = stream.GetStreamMatchupsByWeek(userID, currentWeek - 1);
                streamContent.AddRange(lastWeekMatchupList.OrderByDescending( lw => lw.MatchupItem.TotalVotes).Take(7).ToList());
            }

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
                streamList = stream.GetStream((int)userID, ftr, lastDate);
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
        public ActionResult SaveMessage(string plyID, string msg, int prnt, string type, bool inline)
        {   
            int userID = user.GetUserID(User.Identity.Name);
            SavedMessage userMessage = message.Save(userID, plyID, msg, type, (prnt == 0) ? null : (int?)prnt);

            StreamContent streamItem = stream.ConvertToStream(userMessage.UserMessage, type, userID);
            if (inline && prnt != 0) 
                streamItem.CssClass = "conversation-message";

            string streamData = string.Empty;

            if (inline)
                streamItem.Source = "Mention";

            if (type == "matchup")
                streamItem.ContentType = "matchupMessage";
            else if (type == "general" && prnt != 0)
                streamItem.ContentType = "replyMessage";

            streamData = this.PartialViewToString("_StreamItem", streamItem);
           
            return Json(new
            {
                StreamData = streamData,
                ID = plyID,
                Type = type,
                Inline = inline,
                MentionNotices = userMessage.MentionNotices,
                //LastUpdateTicks = streamItem.DateTicks,
                AddPlayerHeader = false,
                ParentID = prnt
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [HttpPost]
        public ActionResult SendMentionEmail(List<MentionNotice> mentions) 
        {
            if (mentions != null)
            {
                //send off mention emails
                foreach (MentionNotice mention in mentions)
                {
                    EmailHelper.SendMentionEmail(mention.fromUser, mention.toUser, mention.messageID, mention.noticeGuid);
                }
            }
   
            return Json(new
            {
                Sent = true
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [HttpPost]
        public ActionResult SendFollowEmail(int follow)
        {
            int userID = user.GetUserID(User.Identity.Name);

            //send off mention emails
            EmailHelper.SendFollowEmail(userID, follow);

            return Json(new
            {
                Sent = true
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [HttpPost]
        public ActionResult SendMatchupMessageEmail(List<MentionNotice> mentions)
        {
            if (mentions != null)
            {
                //send off mention emails
                foreach (MentionNotice mention in mentions)
                {
                    EmailHelper.SendMatchupMessageEmail(mention.fromUser, mention.toUser, mention.messageID, mention.noticeGuid);
                }
            }

            return Json(new
            {
                Sent = true
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [HttpPost]
        public ActionResult SendMatchupVoteEmail(int mtchid, int voterid)
        {
            //send off mention emails
            EmailHelper.SendMatchupVoteEmail(voterid, mtchid);

            return Json(new
            {
                Sent = true
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public ActionResult GetConversation(int objID, int srcID, string type)
        {
            ConversationViewModel cVM = new ConversationViewModel();
            cVM.Messages = message.GetConversation(objID, srcID, type);
            cVM.SourceMessageID = objID;
            cVM.Type = type;
            cVM.ShowInlineMessage = false;
            cVM.IsTopMessage = (srcID == 0) ? true : false;

            if (User.Identity.IsAuthenticated)
            {
                cVM.Avatar = user.GetByEmail(User.Identity.Name).avatar.imageName;
                cVM.ShowInlineMessage = true;
            }

            string streamData = this.PartialViewToString("_MessageConversation", cVM);

            return Json(new
            {
                StreamData = streamData,
                ID = objID,
                Type = "message",
                IsTopMessage = cVM.IsTopMessage
            }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public ActionResult GetDetails(int objID)
        {
            VoteList voterInfo = new VoteList();
            voterInfo.Votes = matchup.GetMatchupVotes(objID);
            voterInfo.Status = "Active";
            voterInfo.MatchupID = objID;

            string detailsData = this.PartialViewToString("_UserVoteList", voterInfo);

            return Json(new
            {
                DetailsData = detailsData,
            }, JsonRequestBehavior.AllowGet);
        }

        [SiteAuthorization]
        [NoCacheAttribute]
        public ActionResult AddMatchupItem(int player1, int player2, int scoringTypeID)
        {
            int? userID = user.GetUserID(User.Identity.Name);
            WeeklyMatchups usrMatchup = matchup.AddUserMatchup(player1, player2, scoringTypeID, (int)userID);
            usrMatchup.AllowVote = true;
            StreamContent streamItem = stream.ConvertToStream(usrMatchup, player1, false, (int)userID);
            string streamData = this.PartialViewToString("_StreamItem", streamItem);

            return Json(new
            {
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
