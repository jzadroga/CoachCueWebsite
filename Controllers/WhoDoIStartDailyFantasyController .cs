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
    public class WhoDoIStartDailyFantasyController : BaseController
    {
        public async Task<ActionResult> Index(string week, string players)
        {
            MyMatchupViewModel myMatchVM = new MyMatchupViewModel();
            await LoadBaseViewModel(myMatchVM);

            myMatchVM.Matchup = await StreamService.GetDetailStream(myMatchVM.UserData, "whodoistartdailyfantasy/" + week + "/" + players);

            if (myMatchVM.Matchup.MatchupItem == null)
            {
                myMatchVM.Matchup = new StreamContent();
                myMatchVM.Matchup.MatchupItem = new Models.Matchup()
                {
                    Active = true,
                    Id = "0",
                    Type = "Who Do I Start? (Daily Fantasy)"
                };
            }

            myMatchVM.RelatedMatchups = await StreamService.GetRelatedStream(myMatchVM.UserData, myMatchVM.Matchup.MatchupItem);
            SetMatchupPageData(myMatchVM.Matchup.MatchupItem);

            return View(myMatchVM);
        }  
    }
}
