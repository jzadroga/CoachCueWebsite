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
using System.Web.Security;
using System.Configuration;
using CoachCue.Helpers;
using CoachCue.Utility;

namespace CoachCue.Controllers
{
    public class HomeController : BaseController
    {
        public string COACHCUE_AUTH_COOKIE = "coachcue_auth";

        public ActionResult Index([DefaultValue("")] string gu)
        {
            HomeViewModel homeVM = new HomeViewModel();
            homeVM.ShowWelcome = false;
            homeVM.LoggedIn = false;
            homeVM.Stream = new List<PlayerStream>();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            List<nflplayer> trendingPlayers = nflplayer.GetTrending(50);

            homeVM.AllTrendingItems = user.GetAccountsFromPlayers(trendingPlayers, (userID != 0) ? (int?)userID : null);
            homeVM.TrendingItems = homeVM.AllTrendingItems.Take(10).ToList();

            if( userID != 0 )
            {
                homeVM.LoggedIn = true;
               
                int logins = user.SaveLogin(userID);
                //if (logins <= 1)
                //    homeVM.ShowWelcome = true;

                List<PlayerStream> players = stream.GetPlayersStream(userID);
                homeVM.Stream = (players.Count() <= 0) ? stream.GetPlayersStream(trendingPlayers.Take(8).ToList()) : players;
            }
            else
            {
                //also try the cookie
                HttpCookie cookie = HttpContext.Request.Cookies[COACHCUE_AUTH_COOKIE];
                if (cookie != null)
                {
                    if (!string.IsNullOrEmpty(cookie.Values["userGUID"]))
                    {
                        string userGuid = cookie.Values["userGUID"].ToString();
                        string redirectURL = "~/Home";

                        return Redirect("~/Account/LoginByCookie?usr=" + userGuid + "&url=" + redirectURL);
                    }
                }
                
                //not logged in so get random players
                homeVM.Stream = stream.GetPlayersStream(trendingPlayers.Take(8).ToList());
            }

            //user is coming in from an invite email
            homeVM.ShowRegistration = (userID == 0 && gu == "xyyyy-567-0gtr") ? true : false;
            homeVM.ShowFriendInvite = (gu.ToLower() == "invitefriend") ? true : false;

            return View(homeVM);         
        }

        [NoCacheAttribute]
        public JsonResult ContactUs(string email, string message)
        {
            string bodyMsg = "Message from: " + email;
            bodyMsg += " Message: " + message;

            string msg = EmailHelper.Send("info@coachcue.com", bodyMsg, "Contact from site", "Info");
            return Json(new { Result = msg }, JsonRequestBehavior.AllowGet);
        }

        [NoCacheAttribute]
        public ActionResult AddMatchup(int player1, int player2, int scoringTypeID)
        {
            WeeklyMatchups usrMatchup = new WeeklyMatchups();

            List<WeeklyMatchups> matchups = new List<WeeklyMatchups>();

            if (User.Identity.IsAuthenticated)
            {
                int? userID = user.GetUserID(User.Identity.Name);
                usrMatchup = matchup.AddUserMatchup(player1, player2, scoringTypeID, (int)userID);
                matchups = matchup.GetUserMatchups(userID);

                //make sure the one just created is first in the list
                for (int i = 0; i < matchups.Count(); i++)
                {
                    if (matchups[i].MatchupID == usrMatchup.MatchupID && usrMatchup.MatchupID != 0)
                    {
                        WeeklyMatchups newMatchup = matchups[i];
                        matchups.Remove(newMatchup);
                        matchups.Insert(0, newMatchup);
                        break;
                    }
                }
            }

            return PartialView("_MatchupList", matchups);
        }
    }
}
