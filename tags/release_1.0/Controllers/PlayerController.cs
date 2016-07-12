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

namespace CoachCue.Controllers
{
    public class PlayerController : BaseController
    {
        public ActionResult Index(int id, string name)
        {
            PlayerViewModel playerVM = new PlayerViewModel();
            playerVM.LoggedIn = false;

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            playerVM.TrendingItems = user.GetAccountsFromPlayers(nflplayer.GetTrending(15), (userID != 0) ? (int?)userID : null);
            playerVM.PlayerDetail = nflplayer.Get(id);
            playerVM.PlayerStream = stream.GetPlayerStream(id, userID, false);
            playerVM.Followers = nflplayer.FollowersCount(id);

            return View(playerVM);
        }

        public ActionResult List()
        {
            SearchResultViewModel listVM = new SearchResultViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            listVM.Accounts = user.GetAccountsFromPlayers(nflplayer.GetTrending(50), (userID != 0) ? (int?)userID : null);


            return View(listVM);
        }

        [NoCacheAttribute]
        public ActionResult Message([DefaultValue(0)]int msgID, [DefaultValue(false)] bool rply)
        {
            ConversationViewModel cVM = new ConversationViewModel();
            cVM.ReplyMessages = new List<message>();
            cVM.ParentMessage = new message();
            cVM.IsAuth = false;
            cVM.SingleMessage = false;
            cVM.IsReply = rply;
            cVM.ShowReply = true;

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            if (userID != 0)
            {
                cVM.IsAuth = true;
                cVM.Avatar = user.Get(userID).avatar.imageName;
            }

            if (msgID != 0)
            {
                cVM.ParentMessage = message.Get(msgID);

                if( !rply )
                cVM.ReplyMessages = message.GetConversation(msgID);
            }

            return View(cVM);
        }
    }
}
