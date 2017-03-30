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
    public class TopVotedPlayersController : BaseController
    {
        public async Task<ActionResult> Index()
        {
            TopPlayersViewModel topVotedVM = new TopPlayersViewModel();
            await LoadBaseViewModel(topVotedVM);

            topVotedVM.Players = matchup.GetTopMathupVotes(gameschedule.GetCurrentWeekID(), true);

            return View(topVotedVM);
        }
    }
}
