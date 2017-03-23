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
using CoachCue.Repository;
using System.Threading.Tasks;

namespace CoachCue.Controllers
{
    public class SearchController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            SearchResultViewModel srvm = new SearchResultViewModel();
            srvm.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);

            srvm.ShowFollow = false;

            int userID = 0;
            /*if (User.Identity.IsAuthenticated)
            {
                userID = user.GetUserID(User.Identity.Name);
                srvm.ShowFollow = true;
            }*/
            //srvm.Accounts = nflplayer.Search(searchterm, userID);
            srvm.SearchTerm = string.Empty;

            return View(srvm);
        }
    }
}
