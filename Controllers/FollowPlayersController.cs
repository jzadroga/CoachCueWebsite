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
using CoachCue.Utility;

namespace CoachCue.Controllers
{
    public class FollowPlayersController : BaseController
    {
        [SiteAuthorization]
        public ActionResult Index()
        {
            SearchResultViewModel srvm = new SearchResultViewModel();
            srvm.ShowFollow = true;
            int userID = user.GetUserID(User.Identity.Name);
            srvm.Accounts = user.GetFollowingPlayers(userID);

            return View(srvm);
        }
    }
}
