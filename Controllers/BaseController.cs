using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using CoachCue.ViewModels;
using System.Net;
using System.IO;
using CoachCue.Repository;
using CoachCue.Models;
using CoachCue.Service;
using System.Threading.Tasks;

namespace CoachCue.Controllers
{
    public class BaseController : Controller
    {
        public async Task<BaseViewModel> LoadBaseViewModel(BaseViewModel bVM)
        {
            bVM.IsMobile = Request.Browser.IsMobileDevice;
            bVM.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);

            bVM.TrendingItems = (Request.Browser.IsMobileDevice) ? new List<Player>() :  await StreamService.GetTrendingStream();
            bVM.TopCoaches = new List<LeaderboardCoach>(); //(Request.Browser.IsMobileDevice) ? new List<LeaderboardCoach>() : user.GetTopCoachesByWeek(5, gameschedule.GetCurrentWeekID()-1, 5);

            return bVM;
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
