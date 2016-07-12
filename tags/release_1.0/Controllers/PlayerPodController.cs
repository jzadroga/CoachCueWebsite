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
    public class PlayerPodController : Controller
    {
        public ActionResult Index(int id, string name)
        {
            PlayerViewModel playerVM = new PlayerViewModel();
            playerVM.LoggedIn = false;

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            playerVM.TrendingItems = user.GetAccountsFromPlayers(nflplayer.GetTrending(15), (userID != 0) ? (int?)userID : null);
            playerVM.PlayerDetail = nflplayer.Get(id);
            playerVM.PlayerStream = stream.GetPlayerStream(id, userID, false).Take(12).ToList();
            playerVM.Followers = nflplayer.FollowersCount(id);

            return View(playerVM);
        }

        public ActionResult Sample()
        {
            return View();
        }
    }
}
