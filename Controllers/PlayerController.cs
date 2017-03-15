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
using System.Threading.Tasks;
using CoachCue.Service;
using CoachCue.Repository;

namespace CoachCue.Controllers
{
    public class PlayerController : BaseController
    {
        public async Task<ActionResult> Index(string team, string name)
        {
            PlayerViewModel playerVM = new PlayerViewModel();
            playerVM.LoggedIn = false;

            playerVM.UserData = await CoachCueUserData.GetUserData(User.Identity.Name);

            playerVM.PlayerDetail = await PlayerService.GetByDescription(team, name);
            playerVM.PlayerStream = await StreamService.GetPlayerStream(playerVM.UserData, playerVM.PlayerDetail.Id);
            playerVM.TwitterContent = new List<StreamContent>();//stream.GetPlayerTwitterStream(playerVM.PlayerDetail);

            return View(playerVM);
        }

        public ActionResult List()
        {
            SearchResultViewModel listVM = new SearchResultViewModel();

            int userID = (User.Identity.IsAuthenticated) ? user.GetUserID(User.Identity.Name) : 0;
            listVM.Accounts = user.GetAccountsFromPlayers(nflplayer.GetTrending(50), (userID != 0) ? (int?)userID : null);


            return View(listVM);
        }

    }
}
