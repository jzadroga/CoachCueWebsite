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
using System.Web.Routing;
using CoachCue.Utility;
using System.Threading.Tasks;
using CoachCue.Repository;
using CoachCue.Service;

namespace CoachCue.Controllers
{
    public class WhoDoIStartStandardController : BaseController
    {
        public async Task<ActionResult> Index(string week, string players)
        {
            MyMatchupViewModel myMatchVM = new MyMatchupViewModel();

            myMatchVM.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);
            myMatchVM.Matchup = await StreamService.GetDetailStream(myMatchVM.UserData, "whodoistartstandard/" + week + "/" + players);

            if (myMatchVM.Matchup.MatchupItem == null)
                return RedirectToAction("Index", "Home");

            myMatchVM.RelatedMatchups = await StreamService.GetRelatedStream(myMatchVM.UserData, myMatchVM.Matchup.MatchupItem);

            SetMatchupPageData(myMatchVM.Matchup.MatchupItem);

            return View(myMatchVM);
        }

        public ActionResult TopPlayers()
        {
            return View(new TopPlayersViewModel() {
                    Players = matchup.GetTopMathupVotes(gameschedule.GetCurrentWeekID(), true)
            });
        }

        public ActionResult News()
        {
            return View(new TrendingNewsViewModel()
            {
                Players = user.GetAccountsFromPlayers(nflplayer.GetTrending(40), null)
            });
        }
    }
}
