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
using CoachCue.Models;
using CoachCue.Service;

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

                model.TopVotedPlayers = new List<VotedPlayers>(); // (Request.Browser.IsMobileDevice) ? new List<VotedPlayers>() : matchup.GetTopMathupVotes(gameschedule.GetCurrentWeekID(), false);
                model.TrendingItems = new List<AccountData>(); // (Request.Browser.IsMobileDevice) ? new List<AccountData>() : user.GetAccountsFromPlayers(nflplayer.GetTrending(5), (userID != 0) ? (int?)userID : null);
                model.TopCoaches = new List<LeaderboardCoach>(); //(Request.Browser.IsMobileDevice) ? new List<LeaderboardCoach>() : user.GetTopCoachesByWeek(5, gameschedule.GetCurrentWeekID()-1, 5);
            }

            base.OnActionExecuted(filterContext);
        }

        public void SetMatchupPageData(Matchup matchup)
        {
            string title = matchup.Type + " ";
            string players = string.Empty;
            for (int i = 0; i < matchup.Players.Count; i++)
            {
                if (matchup.Players.Count > 2 && i < matchup.Players.Count - 2)
                    players += matchup.Players[i].Name + ", ";
                else
                    players += ((i + 1) != matchup.Players.Count) ? matchup.Players[i].Name + " or " : matchup.Players[i].Name;
            }

            title = title + players;

            ViewBag.Description = "Fantasy football " + title;
            ViewBag.Keywords = "fantasy football," + matchup.Type + "," + players.Replace("or", ",");
            ViewBag.Title = title;

            ViewBag.twitterCard = "summary";
            ViewBag.twitterSite = "@CoachCue";
            ViewBag.twitterTitle = players;
            ViewBag.twitterDescription = "Vote now on " + matchup.Type + " at <a href='http://coachcue.com/" + matchup.Link + "'>Coachcue.com</a>";
            ViewBag.twitterImage = "http://coachcue.com/assets/img/twittercard-matchup1.png";
            ViewBag.twitterUrl = "http://coachcue.com/" + matchup.Link;
        }
    }

}
