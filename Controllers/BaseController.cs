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
using CoachCue.Repository;

namespace CoachCue.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var model = filterContext.Controller.ViewData.Model as BaseViewModel;
            if (model != null)
            {
                model.IsMobile = Request.Browser.IsMobileDevice;
               // model.UserData = CoachCueUserData.GetUserData(User.Identity.Name);

                if (!string.IsNullOrEmpty(model.UserData.UserId))
                {
                   // model.MatchupCount = userItem.MatchupCreatedCount;
                   // model.MessageCount = userItem.MessageCount;
                  //  model.TotalStarters = userItem.TotalMatchupVotes;
                  //  model.CorrectStarters = userItem.TotalCorrectVotes;
                  //  model.NoticeCount = userItem.NotificationCount;
                }

                model.TopVotedPlayers = new List<VotedPlayers>(); // (Request.Browser.IsMobileDevice) ? new List<VotedPlayers>() : matchup.GetTopMathupVotes(gameschedule.GetCurrentWeekID(), false);
                model.TrendingItems = new List<AccountData>(); // (Request.Browser.IsMobileDevice) ? new List<AccountData>() : user.GetAccountsFromPlayers(nflplayer.GetTrending(5), (userID != 0) ? (int?)userID : null);
                model.PlayerID = getPlayerID(filterContext);
                model.TopCoaches = new List<LeaderboardCoach>(); //(Request.Browser.IsMobileDevice) ? new List<LeaderboardCoach>() : user.GetTopCoachesByWeek(5, gameschedule.GetCurrentWeekID()-1, 5);
            }

            base.OnActionExecuted(filterContext);
        }

        private string getPlayerID(ActionExecutedContext filterContext)
        {
            string playerID = string.Empty;

            if (filterContext.RouteData != null)
            {
                if (filterContext.RouteData.Values.Count() == 4)
                {
                    var routeAction = filterContext.RouteData.Values["action"];
                    var routeController = filterContext.RouteData.Values["controller"];
                    if (routeAction != null && routeController != null)
                    {
                        if (routeAction.ToString().ToLower() == "index" && routeController.ToString().ToLower() == "player")
                        {
                            var routePlayer = filterContext.RouteData.Values["id"];
                            if (routePlayer != null)
                                playerID = routePlayer.ToString();
                        }
                    }
                }
            }

            return playerID;
        }
    }
}
