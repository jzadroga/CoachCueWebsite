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
using System.Threading.Tasks;
using CoachCue.Repository;
using CoachCue.Models;
using CoachCue.Service;

namespace CoachCue.Controllers
{
    public class HomeController : BaseController
    {
        public string COACHCUE_AUTH_COOKIE = "coachcue_auth";

        public async Task<ActionResult> Index([DefaultValue("")] string gu, [DefaultValue("")] string tb)
        {
            HomeViewModel homeVM = new HomeViewModel();
            homeVM.ShowWelcome = false;
            homeVM.LoggedIn = false;
            homeVM.LoadMatchups = (!string.IsNullOrEmpty(tb)) ? true : false;

            if (!string.IsNullOrEmpty(tb))
                HttpContext.Session["media"] = tb;

            if(!string.IsNullOrEmpty(homeVM.UserData.UserId))
            {
                homeVM.LoggedIn = true;
               
                int logins = user.SaveLogin(Convert.ToInt32(homeVM.UserData.UserId));
                //if (logins <= 1)
                //    homeVM.ShowWelcome = true;
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
            }

            //homeVM.Stream = await StreamService.GetHomeStream(homeVM.UserData);

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
