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
            
            playerVM.PlayerDetail = nflplayer.Get(id);
            playerVM.PlayerStream = stream.GetPlayerStream(id, userID, false);
            playerVM.Followers = nflplayer.FollowersCount(id);
            playerVM.TwitterContent = stream.GetPlayerTwitterStream(playerVM.PlayerDetail);

            return View(playerVM);
        }

        public ActionResult List()
        {
            SearchResultViewModel listVM = new SearchResultViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            listVM.Accounts = user.GetAccountsFromPlayers(nflplayer.GetTrending(50), (userID != 0) ? (int?)userID : null);


            return View(listVM);
        }

    }
}
